using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

public class Endpoints
{
    public string EndpointURL { get; }
    public string HTTP_Verb { get; }
    string[] RequestArray;
    string[] FullEndpointURL;
    string FilePath;
    string r_string;
    public Endpoints(Byte[] recBytes, string filepath)
    {
        r_string = new string(Encoding.ASCII.GetChars(recBytes));
        RequestArray = r_string.Split('\n');
        var endpoint_line = RequestArray[0].Split(' ');
        FullEndpointURL = endpoint_line[1].Split('/');
        EndpointURL = FullEndpointURL[1];
        HTTP_Verb = endpoint_line[0];
        FilePath = filepath;
    }
    public string getIndex()
    {
        string responseContent = "\r\n\r\n";
        string responseStatus = "200 OK";
        string responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
        return responseString;
    }
    public string getEcho()
    {
        string encodingType = "";
        foreach (var ReqLine in RequestArray)
        {

            if (ReqLine.ToLower().Contains("encoding"))
            {
                if (ReqLine.Contains("gzip"))
                {
                    encodingType = "\r\nContent-Encoding: gzip";
                    break;
                }
            }
        }
        string responseStatus = "200 OK";
        string responseContent = $"\r\nContent-Type: text/plain\r\nContent-Length: {FullEndpointURL[2].Length}\r\n\r\n{FullEndpointURL[2]}";
        string responseString = $"HTTP/1.1 {responseStatus}{encodingType}{responseContent}";
        return responseString;
    }
    public string getUserAgent()
    {

        string responseStatus = "200 OK";
        string responseContent;
        if (RequestArray.Length <= 3)
        {
            responseStatus = "404 Not Found";
            responseContent = "\r\nContent-Type: text/plain\r\nContent-Length: 0\r\n\r\n";
            string responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
            return responseString;
        }
        string userAgentLine = "";
        foreach (string line in RequestArray)
        {
            if (line.StartsWith("User-Agent:"))
            {
                userAgentLine = line;
                break;
            }
        }
        if (string.IsNullOrEmpty(userAgentLine))
        {
            responseContent = "\r\nContent-Type: text/plain\r\nContent-Length: 0\r\n\r\n";
            string responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
            return responseString;
        }
        else
        {
            string[] parts = userAgentLine.Split(':');
            string agent = parts[1].Trim();
            int contentLength1 = Encoding.UTF8.GetByteCount(agent);
            responseContent = $"\r\nContent-Type: text/plain\r\nContent-Length: {contentLength1}\r\n\r\n{agent}";
            string responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
            return responseString;
        }
    }
    public string getFiles()
    {

        string responseContent = "\r\n\r\n";
        string responseStatus;
        string responseString;
        Console.WriteLine(FilePath + FullEndpointURL[2]);
        if (string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(FullEndpointURL[2]) || !File.Exists(FilePath + FullEndpointURL[2]))
        {
            responseStatus = "404 Not Found";
            responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
            return responseString;
        }
        string fileContent = File.ReadAllText(FilePath + FullEndpointURL[2]);
        int contentLength2 = Encoding.UTF8.GetByteCount(fileContent);
        responseStatus = "200 OK";
        responseContent = $"\r\nContent-Type: application/octet-stream\r\nContent-Length: {contentLength2}\r\n\r\n{fileContent}";
        responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
        return responseString;
    }
    public string postFiles()
    {

        string responseContent = "\r\n\r\n";
        string responseStatus;
        string responseString;

        Console.WriteLine(FilePath + FullEndpointURL[2]);
        if (string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(FullEndpointURL[2]))
        {
            responseStatus = "404 Not Found";
            responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
            return responseString;
        }
        string filePath = FilePath + FullEndpointURL[2];
        var data = RequestArray[RequestArray.Length - 1].Trim('\0').Trim();
        Console.WriteLine(data.Length);
        File.WriteAllText(filePath, data);
        responseStatus = "201 Created";
        responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
        return responseString;
    }
}