
using System.Net;
using System.Net.Sockets;
internal class WebServer
{

  
    private const int PORT = 5000;
    private const string IP_ADDRESS = "0.0.0.0";
    private const int BUFFER_SIZE = 1024 * 4;
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
        string request = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"Received request: {request}");
        string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello World!";
        byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(response);
        stream.Write(responseBytes, 0, responseBytes.Length);
        client.Close();
    }



    static void Main(string[] args)
    {
        StartServer();
        Console.WriteLine("Start Web Server ");
    }
}