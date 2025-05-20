using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
var ipEndPoint = new IPEndPoint(IPAddress.Any, 2009);
var listenear = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
listenear.Bind(ipEndPoint);
listenear.Listen(20);

Console.WriteLine($"Сервер запущено {ipEndPoint}");

while (true)
{
    Socket client = await listenear.AcceptAsync();
    _ = Task.Run(() => HandleClientAsync(client));

}
async Task HandleClientAsync(Socket client)
{
    Console.WriteLine($"Клієнт {client.RemoteEndPoint} підключився");
    byte[] buffer = new byte[10024];
    int bytesRead = await client.ReceiveAsync(buffer);
    string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
    Console.WriteLine("Клієнт прислав повідомлення: " + message);
    string response = $"Сервер отримав ваше повідомлення: {DateTime.UtcNow.ToLocalTime().ToShortTimeString()} - {client.RemoteEndPoint}";
    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
    await client.SendAsync(responseBytes);
    client.Shutdown(SocketShutdown.Both);
    client.Close();
}