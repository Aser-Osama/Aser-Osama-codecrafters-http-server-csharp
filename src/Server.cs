using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
Socket socket = server.AcceptSocket(); // wait for client, this is blocking

Console.WriteLine("Connection Accepted");

string responseString = "HTTP/1.1 220 OK\r\n\r\n";

//Forms and sends a response string to the connected client.
Byte[] sendBytes = Encoding.ASCII.GetBytes(responseString);
int i = socket.Send(sendBytes);
Console.WriteLine("Message Sent /> : " + responseString + "\n with response " + i);
