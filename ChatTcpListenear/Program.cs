using System.Net;
using System.Net.Sockets;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

Dictionary<string, string> userPhotos = new();
List<TcpClient> clients = new();
TcpListener server = new(IPAddress.Any, 2004);
server.Start();
Console.WriteLine($"Сервер запущено на {server.LocalEndpoint}");

while (true){
    TcpClient client = await server.AcceptTcpClientAsync();
    _ = Task.Run(() => HandleClientAsync(client));
}

async Task HandleClientAsync(TcpClient client){
    using NetworkStream stream = client.GetStream();
    byte[] buffer = new byte[1024];

    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

    string[] parts = receivedData.Split(' ', 2);
    string name = parts[0];
    string photoFile = parts.Length > 1 ? parts[1] : "";
    if (!string.IsNullOrEmpty(photoFile)){
        userPhotos[name] = photoFile;
    }
    Console.WriteLine($"{name} приєднався(лась) до чату");
    clients.Add(client);
    MessageToUsers($"{name} приєднався(лась) до чату", excludeClient: client);
    while (true){
        try{
            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

            if (receivedData.StartsWith("photo ")){
                string targetUser = receivedData.Substring(6).Trim();
                string photoUrl = userPhotos.ContainsKey(targetUser) ? $"http://myp22.itstep.click/images/{userPhotos[targetUser]}" : "Користувач не має фото або користувач не знайдено";

                await stream.WriteAsync(Encoding.UTF8.GetBytes(photoUrl));
                continue;
            }
            if (receivedData.StartsWith("update ")){
                string[] part = receivedData.Split(' ', 3);
                if (part.Length == 3){
                    string userName = part[1];
                    string newPhotoName = part[2];
                    userPhotos[userName] = newPhotoName;
                    await stream.WriteAsync(Encoding.UTF8.GetBytes("Фото оновлено"));
                }
                continue;
            }
            if (receivedData.ToLower() == "exit"){
                string exitMessage = $"{name} покинув(ла) чат";
                Console.WriteLine(exitMessage);
                MessageToUsers(exitMessage);

                clients.Remove(client);
                userPhotos.Remove(name); 
                client.Close();
               break;
            }
            string message = $"{name}: {receivedData}";
            Console.WriteLine(message);
            MessageToUsers(message, excludeClient: client);
        }
        catch{
            break;
        }
    }
}
void MessageToUsers(string message, TcpClient? excludeClient = null){
    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
    foreach (var cl in clients){
        if (cl == excludeClient) continue;
        try{
            cl.GetStream().WriteAsync(messageBytes);
        }
        catch{
            clients.Remove(cl);
        }
    }
}