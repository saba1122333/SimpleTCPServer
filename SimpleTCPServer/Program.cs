
using System.Net;
using System.Net.Sockets;
internal class WebServer
{

  
    private const int PORT = 5000;
    private const string IP_ADDRESS = "0.0.0.0";
    private const int BUFFER_SIZE = 1024 * 4;
    private const string Root = "Webroot";


    private static readonly string[] ALLOWED_EXTENSIONS = { "html", "css", "js", "ico" };
    private static readonly string[] IGNORE_ENDPOINTS = { "favicon.ico" };
    private static readonly string ALLOWED_METHOD = "GET";



    private static void StartServer()
    {
        TcpListener listener = new TcpListener(IPAddress.Parse(IP_ADDRESS), PORT);
        listener.Start();
        Console.WriteLine($"Server started on {IP_ADDRESS}:{PORT}");
        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected");
            HandleClient(client);
        }
    }
    private static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[BUFFER_SIZE];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        if (bytesRead == 0)
        {
            Console.WriteLine("Empty request - closing connection");
            client.Close();
            return;
        }

        string request = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"Received request: {request}");
      

        if (HandleRequest(request))
        {
            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello World!";
            byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        client.Close();
    }
    private static bool HandleRequest(string request)
    {
        if (string.IsNullOrEmpty(request.Trim()))
        {
            Console.WriteLine("Empty request - ignoring");
            return false;
        }

        string[] lines = request.Split('\n');
        if (lines.Length == 0) return false;

        string[] requestParts = lines[0].Trim().Split(' ');
        if (requestParts.Length < 3)
        {
            Console.WriteLine("Invalid request format");
            return false;
        }






        string method = requestParts[0] == ALLOWED_METHOD ? ALLOWED_METHOD : string.Empty;


        string url = requestParts[1];

        Console.WriteLine($"Method: {method}, URL: {url}");



        if (string.IsNullOrEmpty(method))
        {
            Console.WriteLine($"Method Not Allowed (405): {method}");
            return false;
        }
        string[] urlParts = url.Split('.');
        string extension = urlParts.Length > 1 ? urlParts.Last() : string.Empty;
        Console.WriteLine("extension: " + extension);

        if (string.IsNullOrEmpty(extension) || !ALLOWED_EXTENSIONS.Contains(extension))
        {
            Console.WriteLine($"Forbidden (403) – Unsupported File Type: {extension}");
            return false;
        }
        {

        }


        if (url == "/")
        {
            url = "index.html";
        }


        if (url.StartsWith("/"))
        {

            url = url.Substring(1);
        }


        if (IGNORE_ENDPOINTS.Contains(url))
        {
            Console.WriteLine("Favicon request - ignoring");
            // this is sent by the browser automaticaly I dont know why ? 
            return false;
        }



        string filePath = Path.Combine("..", "..", "..", Root, url);
        string fullPath = Path.GetFullPath(filePath);

        Console.WriteLine($"Looking for: {filePath}");

        if (File.Exists(filePath))
        {
            Console.WriteLine("File found!");
            return true;
        }
        else
        {
            Console.WriteLine("File Not Found (404)");
            return false;
        }


    }
    private static void HandleErorr(int code)
    {


    }



    static void Main(string[] args)
    {
        StartServer();
        Console.WriteLine("Start Web Server ");
    }
}