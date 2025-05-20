using System.Net;
using System.Net.Sockets;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

var endPoint = new IPEndPoint(IPAddress.Parse("91.238.103.107"), 8888);
Console.WriteLine($"Search server {endPoint}");
await clientSocket.ConnectAsync(endPoint);

string text = Console.ReadLine();
byte[] data = Encoding.UTF8.GetBytes(text);
await clientSocket.SendAsync(data);
byte[] buffer = new byte[10024];
int bytes = await clientSocket.ReceiveAsync(buffer);
string textServerResp = Encoding.UTF8.GetString(buffer, 0, bytes);
Console.WriteLine($"Server response: {textServerResp}");
clientSocket.Shutdown(SocketShutdown.Both);
clientSocket.Close();