using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

public class MainClass
{
    public static void Main(string[] args)
    {

        TcpListener server = new TcpListener(IPAddress.Any, 4221);
        server.Start();
        Console.WriteLine($"TCP Server has started");
        // wait for client, this is blocking
        string filePath = "";
        if (args.Length > 1)
        {
            filePath = args[1];
        }

        while (true)
        {
            Console.WriteLine("Awaiting requests");
            Socket socket = server.AcceptSocket();
            Thread clientThread = new Thread(() => HandleClient(socket, filePath));
            clientThread.Start();
        }
    }

    static void HandleClient(Socket socket, string fp)
    {
        //Receive request from server
        Byte[] recBytes = new Byte[4096];
        int r = socket.Receive(recBytes);
        Endpoints ep = new Endpoints(recBytes, fp);
        string endpoint = ep.EndpointURL;
        string HTTP_Verb = ep.HTTP_Verb;
        //Send response back to servers
        string responseString = "HTTP/1.1 404 Not Found\r\n\r\n"; ;

        switch (endpoint)
        {
            case "":
                responseString = ep.getIndex();
                break;
            case "echo":
                responseString = ep.getEcho();

                break;
            case "user-agent":
                responseString = ep.getUserAgent();
                break;
            case "files":
                if (HTTP_Verb == "GET")
                    responseString = ep.getFiles();
                else if (HTTP_Verb == "POST")
                    responseString = ep.postFiles();
                break;
            default:
                break;
        }

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


}
