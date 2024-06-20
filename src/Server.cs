using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
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
// wait for client, this is blocking
while (true)
{
    Console.WriteLine("Awaiting requests");
    Socket socket = server.AcceptSocket();
    Thread clientThread = new Thread(() => HandleClient(socket));
    clientThread.Start();
}



static void HandleClient(Socket socket)
{
    //Receive request from server
    Byte[] recBytes = new Byte[4096];
    int r = socket.Receive(recBytes);
    string r_string = new string(Encoding.ASCII.GetChars(recBytes));
    var r_arr = r_string.Split('\n');
    var endpoint = r_arr[0].Split(' ')[1].Split('/');

    //Send response back to servers
    string responseStatus = "";
    string responseContent = "\r\n\r\n";
    switch (endpoint[1].ToLower())
    {
        case "":
            responseStatus = "200 OK";
            break;
        case "echo":
            responseStatus = "200 OK";
            responseContent = $"\r\nContent-Type: text/plain\r\nContent-Length: {endpoint[2].Length}\r\n\r\n{endpoint[2]}";
            break;
        case "user-agent":
            responseStatus = "200 OK";
            if (r_arr.Length > 3)
            {
                string userAgentLine = "";
                foreach (string line in r_arr)
                {
                    if (line.StartsWith("User-Agent:"))
                    {
                        userAgentLine = line;
                    }
                }
                if (string.IsNullOrEmpty(userAgentLine))
                {
                    responseContent = "\r\nContent-Type: text/plain\r\nContent-Length: 0\r\n\r\n";
                }
                else
                {
                    string[] parts = userAgentLine.Split(':');
                    string agent = parts[1].Trim();
                    int contentLength = Encoding.UTF8.GetByteCount(agent);
                    responseContent = $"\r\nContent-Type: text/plain\r\nContent-Length: {contentLength}\r\n\r\n{agent}";
                }
            }
            else
            {
                responseContent = "\r\nContent-Type: text/plain\r\nContent-Length: 0\r\n\r\n";
            }
            break;

        default:
            responseStatus = "404 Not Found";
            break;
    }

    string responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
    Byte[] sendBytes = Encoding.ASCII.GetBytes(responseString);
    int totalBytesSent = 0;
    while (totalBytesSent < sendBytes.Length)
    {
        int sent = socket.Send(sendBytes, totalBytesSent, sendBytes.Length - totalBytesSent, SocketFlags.None);
        if (sent == 0)
        {
            break; // Socket has been closed or an error occurred.
        }
        totalBytesSent += sent;
    }
    Console.WriteLine("Sent: \n------------\n" + responseString + "\n------------\n");
}
