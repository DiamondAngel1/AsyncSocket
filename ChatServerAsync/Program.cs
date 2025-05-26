using System.Net;
using System.Net.Sockets;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

List<Socket> clients = new List<Socket>();
var ipEndPoint = new IPEndPoint(IPAddress.Any, 2004);
var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
listener.Bind(ipEndPoint);
listener.Listen(20);

Console.WriteLine($"Сервер запущено {ipEndPoint}");
while (true)
{
    Socket client = await listener.AcceptAsync();
    clients.Add(client);
    _ = Task.Run(() => HandleClientAsync(client));
}

async Task HandleClientAsync(Socket client)
{
    byte[] buffer = new byte[1024];
    int bytesRead = await client.ReceiveAsync(buffer);
    string name = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

    string welcomeMessage = $"{name} приєднався(лась) до чату";
    Console.WriteLine(welcomeMessage);

    byte[] welcomeBytes = Encoding.UTF8.GetBytes(welcomeMessage);
    foreach (var cl in clients)
    {
        if (cl != client)
        {
            try
            {
                cl.Send(welcomeBytes);
            }
            catch (SocketException)
            {
                clients.Remove(cl);
            }
        }
    }
    while (true)
    {
        try
        {
            bytesRead = await client.ReceiveAsync(buffer);
            if (bytesRead == 0) break;

            string message = $"{name}: {Encoding.UTF8.GetString(buffer, 0, bytesRead)}";
            Console.WriteLine(message);

            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            foreach (var cl in clients)
            {
                if (cl != client)
                {
                    try
                    {
                        cl.Send(messageBytes);
                    }
                    catch (SocketException)
                    {
                        clients.Remove(cl);
                    }
                }
            }
        }
        catch (SocketException)
        {
            break;
        }
    }
    string exitMessage = $"{name} покинув(ла) чат";
    Console.WriteLine(exitMessage);
    byte[] exitBytes = Encoding.UTF8.GetBytes(exitMessage);
    foreach (var cl in clients)
    {
        if (cl != client)
        {
            try
            {
                cl.Send(exitBytes);
            }
            catch (SocketException)
            {
                clients.Remove(cl);
            }
        }
    }
    clients.Remove(client);
    client.Shutdown(SocketShutdown.Both);
    client.Close();
}