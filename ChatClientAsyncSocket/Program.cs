using System.Net.Sockets;
using System.Net;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2004);

await clientSocket.ConnectAsync(endPoint);

Console.WriteLine("Введіть ваше ім’я: ");
string name = Console.ReadLine();
Console.WriteLine("ps: Введіть 'exit' або 'вийти' для виходу з чату, або введіть повідомлення");
await clientSocket.SendAsync(Encoding.UTF8.GetBytes(name));

_ = Task.Run(async () =>
{
    byte[] buffer = new byte[1024];
    while (true)
    {
        int bytes = await clientSocket.ReceiveAsync(buffer);
        if (bytes == 0) break;
        Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, bytes));
    }
});
while (true)
{
    string text = Console.ReadLine();
    if (text.ToLower() == "exit" || text.ToLower() == "вийти")
        break;
    byte[] data = Encoding.UTF8.GetBytes(text);
    await clientSocket.SendAsync(data);
}
clientSocket.Close();