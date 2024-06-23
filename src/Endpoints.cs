using System.Text;
using System.IO.Compression;

// class (poorly named) responsible for all the processing 
public class Endpoints
{
    public string EndpointURL { get; }
    public string HTTP_Verb { get; }
    string[] RequestArray;
    string[] FullEndpointURL;
    string FilePath;
    string RequestString;
    public Endpoints(Byte[] recBytes, string filepath)
    {
        RequestString = new string(Encoding.ASCII.GetChars(recBytes));
        RequestArray = RequestString.Split('\n');

        var endpoint_line = RequestArray[0].Split(' ');
        FullEndpointURL = endpoint_line[1].Split('/');

        // The second part is used for the echo and files endpoints
        EndpointURL = FullEndpointURL[1];
        HTTP_Verb = endpoint_line[0];

        FilePath = filepath;
    }
    public byte[] getIndex()
    {
        string responseString = $"HTTP/1.1 200 OK\r\n\r\n";
        return Encoding.UTF8.GetBytes(responseString);
    }
    public byte[] getEcho()
    {
        string returnString = FullEndpointURL[2].Trim('\0', '\r', '\n');
        byte[] responseData = Encoding.UTF8.GetBytes(returnString);
        bool isGzip = false;

        foreach (var reqLine in RequestArray)
        {
            if (reqLine.ToLower().Contains("encoding") && reqLine.Contains("gzip"))
            {
                responseData = Compress(returnString);
                isGzip = true;
                break;
            }
        }

        string responseStatus = "HTTP/1.1 200 OK";
        string headers = $"\r\nContent-Type: text/plain";
        if (isGzip)
        {
            headers += "\r\nContent-Encoding: gzip";
        }
        headers += $"\r\nContent-Length: {responseData.Length}\r\n\r\n";

        byte[] headerBytes = Encoding.ASCII.GetBytes(responseStatus + headers);
        byte[] response = new byte[headerBytes.Length + responseData.Length];
        Buffer.BlockCopy(headerBytes, 0, response, 0, headerBytes.Length);
        Buffer.BlockCopy(responseData, 0, response, headerBytes.Length, responseData.Length);

        return response;
    }
    public byte[] getUserAgent()
    {

        string responseStatus = "200 OK";
        string responseContent;
        if (RequestArray.Length <= 3)
        {
            responseStatus = "404 Not Found";
            responseContent = "\r\nContent-Type: text/plain\r\nContent-Length: 0\r\n\r\n";
            string responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
            return Encoding.UTF8.GetBytes(responseString);
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
            return Encoding.UTF8.GetBytes(responseString);
        }
        else
        {
            string[] parts = userAgentLine.Split(':');
            string agent = parts[1].Trim();
            int contentLength1 = Encoding.UTF8.GetByteCount(agent);
            responseContent = $"\r\nContent-Type: text/plain\r\nContent-Length: {contentLength1}\r\n\r\n{agent}";
            string responseString = $"HTTP/1.1 {responseStatus}{responseContent}";
            return Encoding.UTF8.GetBytes(responseString);
        }
    }
    public byte[] getFiles()
    {
        string responseString;
        Console.WriteLine(FilePath + FullEndpointURL[2]);
        if (string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(FullEndpointURL[2]) || !File.Exists(FilePath + FullEndpointURL[2]))
        {
            responseString = $"HTTP/1.1 404 Not Found\r\n\r\n";
            return Encoding.UTF8.GetBytes(responseString);
        }

        string fileContent = File.ReadAllText(FilePath + FullEndpointURL[2]);
        int contentLength = Encoding.UTF8.GetByteCount(fileContent);

        var responseContent = $"\r\nContent-Type: application/octet-stream\r\nContent-Length: {contentLength}\r\n\r\n{fileContent}";
        return Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK{responseContent}");
    }
    public byte[] postFiles()
    {
        // Second endpoint is the file name
        if (string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(FullEndpointURL[2]))
        {
            return Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\n\r\n");
        }
        string fileNameAndPath = FilePath + FullEndpointURL[2];
        var data = RequestArray[RequestArray.Length - 1].Trim('\0').Trim();
        File.WriteAllText(fileNameAndPath, data);
        return Encoding.UTF8.GetBytes($"HTTP/1.1 201 Created\r\n\r\n");
    }
    private static byte[] Compress(string value)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var gZipStream = new GZipStream(memoryStream, CompressionLevel.Fastest))
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(value);
                gZipStream.Write(inputBytes, 0, inputBytes.Length);
            }
            // Ensure the GZipStream is properly flushed and closed before getting the byte array
            var outputArray = memoryStream.ToArray();
            return outputArray;
        }
    }
}