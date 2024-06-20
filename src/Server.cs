using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
//Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
while (true)
{
    // wait for client, this is blocking
    Socket socket = server.AcceptSocket();

    //Log the connection URL
    //Console.WriteLine($"Connection Accepted on url {socket.LocalEndPoint} ");

    //Receive request from server
    Byte[] recBytes = new Byte[512];
    int r = socket.Receive(recBytes);
    string r_string = new string(Encoding.ASCII.GetChars(recBytes));
    string endpoint = r_string.Split('\n')[0].Split(' ')[1];
    Console.WriteLine($"Recived: \n ------------ \n|{endpoint}|\n------------\n");


    //Send response back to server
    string response_verb = (endpoint == "/") ? "200 OK" : "404 Not Found";
    string responseString = $"HTTP/1.1 {response_verb}\r\n\r\n";
    Byte[] sendBytes = Encoding.ASCII.GetBytes(responseString);
    int i = socket.Send(sendBytes);
    Console.WriteLine("Sent: \n------------\n" + responseString + "\n------------\n");
}
