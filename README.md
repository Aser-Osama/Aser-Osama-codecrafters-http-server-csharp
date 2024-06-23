# HTTP/1.1 Server

This repository is an implementation of an HTTP/1.1 web server written in C#. Using TCP protocol, this basic server implementation supports POST and GET methods, can return data compressed using the gzip algorithm, and manages multiple concurrent connections through C#'s multithreading capabilities.

## Project Setup

### Prerequisites
Ensure you have [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed on your system. Verify this by running `dotnet --version` in your command line.

### Building and Running the Project
Clone the repository and set up your project environment:

```sh
git clone https://github.com/Aser-Osama/codecrafters-http-server-csharp.git
cd codecrafters-http-server-csharp
```

Build and execute the project:
```sh
./your_server.sh
```

To use the `/files` endpoint, specify a directory for storing or retrieving files using the following command:
```sh
./your_server.sh  --directory /your/directory/
```
