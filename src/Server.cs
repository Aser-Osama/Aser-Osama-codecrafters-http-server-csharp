using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
//Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
Console.WriteLine($"TCP Server has started");
while (true)
{
    Console.WriteLine("Awaiting requests");
    // wait for client, this is blocking
    Socket socket = server.AcceptSocket();

    //Log the connection URL

    //Receive request from server
    Byte[] recBytes = new Byte[512];
    int r = socket.Receive(recBytes);
    string r_string = new string(Encoding.ASCII.GetChars(recBytes));
    var endpoint = r_string.Split('\n')[0].Split(' ')[1].Split('/');
    foreach (var item in endpoint)
    {
        Console.WriteLine($"|{item}|");
    }


    //Send response back to servers
    string responseStatus = "";
    string responseContent = "\r\n\r\n";
    if (endpoint[1] == "")
    {
        responseStatus = "200 OK";
    }
    else if (endpoint[1].ToLower() == "echo")
    {
        responseStatus = "200 OK";
        responseContent = $"\r\nContent-Type: text/plain\r\nContent-Length: {endpoint[2].Length}\r\n\r\n{endpoint[2]}";
    }
    else
    {
        responseStatus = "404 Not Found";
    }

    string responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
    Byte[] sendBytes = Encoding.ASCII.GetBytes(responseString);
    int i = socket.Send(sendBytes);
    Console.WriteLine("Sent: \n------------\n" + responseString + "\n------------\n");
}
