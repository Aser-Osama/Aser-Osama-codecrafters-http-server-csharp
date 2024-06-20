using System.Collections.Immutable;
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
    // wait for client, this is blocking
    Console.WriteLine("Awaiting requests");
    Socket socket = server.AcceptSocket();


    //Receive request from server
    Byte[] recBytes = new Byte[512];
    int r = socket.Receive(recBytes);
    string r_string = new string(Encoding.ASCII.GetChars(recBytes));
    var r_arr = r_string.Split('\n');
    var endpoint = r_arr[0].Split(' ')[1].Split('/');
    // Console.WriteLine(r_string + "\n ------------");
    // foreach (var item in endpoint)
    // {
    //     Console.Write($"|{item}|");
    // }
    // Console.WriteLine("");

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
    else if (endpoint[1].ToLower() == "user-agent")
    {
        responseStatus = "200 OK";
        if (r_arr.Length >= 4)
        {
            //Console.WriteLine(r_arr[3]);
            string agent = r_arr[3].Split(':')[1][1..];
            //Console.WriteLine(agent);
            responseContent = $"\r\nContent-Type: text/plain\r\nContent-Length: {agent.Length}\r\n\r\n{agent}";
        }
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
