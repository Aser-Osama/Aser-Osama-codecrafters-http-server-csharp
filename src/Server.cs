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
        string filePath = "";
        if (args.Length > 1)
        {
            filePath = args[1];
        }

        while (true)
        {
            // wait for client requests, this is blocking
            Console.WriteLine("Awaiting requests");
            Socket socket = server.AcceptSocket();

            // start a new thread to handle the request then die, supporting basic concurrency
            Thread clientThread = new Thread(() => HandleClient(socket, filePath));
            clientThread.Start();
        }
    }

    static void HandleClient(Socket socket, string fp)
    {
        //Receive request from server
        byte[] recBytes = new Byte[4096];
        int r = socket.Receive(recBytes);

        // class that internally handles the requests
        Endpoints ep = new Endpoints(recBytes, fp);

        // retrive the endpoint the client was targeting as well as the http verb (get or post only)
        string endpoint = ep.EndpointURL;
        string HTTP_Verb = ep.HTTP_Verb;

        //Send response back to servers
        byte[] responseString = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");

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

        int totalBytesSent = 0;
        while (totalBytesSent < responseString.Length)
        { // Send the respose back to the user
            int sent = socket.Send(responseString, totalBytesSent, responseString.Length - totalBytesSent, SocketFlags.None);
            if (sent == 0)
            {
                break; // Socket has been closed or an error occurred.
            }
            totalBytesSent += sent;
        }
        Console.WriteLine("Sent: \n------------\n" + Encoding.UTF8.GetString(responseString) + "\n------------\n");
    }


}
