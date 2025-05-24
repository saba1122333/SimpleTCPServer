using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
namespace MyApp
{

    internal class WebServer
    {

        private const int PORT = 5000;
        private const string IP_ADDRESS = "0.0.0.0";
        private const int BUFFER_SIZE = 1024 * 4;
        private const string Root = "Webroot";

        private static readonly string[] ALLOWED_EXTENSIONS = { "html", "css", "js", "ico" };
        private static readonly string[] IGNORE_ENDPOINTS = { "favicon.ico" };
        private static readonly string ALLOWED_METHOD = "GET";

        private static readonly object _logLock = new object();
        private static readonly string _logFile = Path.Combine("..", "..", "..", "logs", "requests.log");

        private static void LogRequest(string clientIP, string method, string url, string massage, int statusCode)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {clientIP} | {method} {url} | {massage} | {statusCode}\n";

            lock (_logLock)
            {
                string? logDirectory = Path.GetDirectoryName(_logFile);
                if (!string.IsNullOrEmpty(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                File.AppendAllText(_logFile, logEntry);
            }
        }



        private static void StartServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(IP_ADDRESS), PORT);
            listener.Start();
            Console.WriteLine($"Server started on {IP_ADDRESS}:{PORT}");
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected");

                if (client.Client.RemoteEndPoint is IPEndPoint remoteEndPoint)
                {
                    string clientIP = remoteEndPoint.Address.ToString();
                    ThreadPool.QueueUserWorkItem(_ => HandleClient(client, clientIP));
                }
                else
                {
                    string massage = "Client RemoteEndPoint is null or not an IPEndPoint. Closing connection.";
                    LogRequest(client.Client.RemoteEndPoint?.ToString() ?? "UNKNOWN", "UNKNOWN", "UNKNOWN", massage, 400);
                    client.Close();
                }
            }
        }
        private static void HandleClient(TcpClient client, string clientIP)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[BUFFER_SIZE];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);



            string request = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine(request);
            Console.WriteLine(HandleRequest(request, stream, clientIP));


            client.Close();
        }

        private static bool HandleRequest(string request, NetworkStream stream, string clientIP)
        {
            if (string.IsNullOrEmpty(request.Trim()))
            {
                // we are just going to ingnore this request as it is sent be browser when it is idle     
                string massange = "Empty request - ignoring";
                LogRequest(clientIP, "UNKNOWN", "UNKNOWN", massange, 400);

                return false;
            }

            string[] lines = request.Split('\n');
            if (lines.Length == 0) return false;

            string[] requestParts = lines[0].Trim().Split(' ');
            if (requestParts.Length < 3)
            {

                //  fetch('http://localhost:5000/index.html', { method: 'POST' })
                //.then(response => response.text())
                //.then(html => {
                //    document.open();
                //    document.write(html);
                //    document.close();
                //}); this is for testing purpose for 400 error 


                string massage = "Invalid request format - expected at least 3 parts (method, URL, HTTP version)";

                LogRequest(clientIP, "MALFORMED", "MALFORMED", massage, 400);

                HandleErorr(400, "Bad Request", stream);
                return false;
            }

            string method = requestParts[0] == ALLOWED_METHOD ? ALLOWED_METHOD : string.Empty;


            string url = requestParts[1];




            if (string.IsNullOrEmpty(method))
            {
                string massage = "Method Not Allowed - Only GET method is allowed";
                LogRequest(clientIP, method, url, massage, 405);

                HandleErorr(405, "Method Not Allowed", stream);


                return false;
            }


            if (url == "/")
            {
                url = "index.html";
            }


            if (url.StartsWith("/"))
            {

                url = url.TrimStart('/');

            }
            if (url.EndsWith("/"))
            {
                url = url.TrimEnd('/');
            }



            string[] urlParts = url.Split('.');
            string extension = urlParts.Length > 1 ? urlParts.Last() : string.Empty;

            if (string.IsNullOrEmpty(extension) || !ALLOWED_EXTENSIONS.Contains(extension.ToLower()))
            {
                string massage = $"Forbidden – Unsupported File Type: {extension}";

                LogRequest(clientIP, method, url, massage, 403);

                HandleErorr(403, "Forbidden", stream);

                return false;
            }
            {

            }



            if (IGNORE_ENDPOINTS.Contains(url))
            {

                // we can log this but it is unnecessary as it is a browser request when idling 

                //string massage = "Favicon request - ignoring";
                //LogRequest(clientIP, method, url, massage, 400);

                return false;
            }



            string filePath = Path.Combine("..", "..", "..", Root, url);
            string fullPath = Path.GetFullPath(filePath);


            if (File.Exists(filePath))
            {
                string massage = $"File found: {filePath}";

                LogRequest(clientIP, method, url, massage, 200);

                HandleSucess(200, fullPath, "File found!", stream);
                return true;
            }
            else
            {
                string massage = $"File Not Found: {filePath}";

                LogRequest(clientIP, method, url, massage, 404);

                HandleErorr(404, "File Not Found", stream);
                return false;
            }

        }

        private static void HandleSucess(int code, string fullPath, string massage, NetworkStream stream)
        {

            byte[] Content;
            string headers;
            byte[] headerBytes;
            string contentType = Path.GetExtension(fullPath).ToLower() switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                _ => "text/plain"
            };


            Content = File.ReadAllBytes(fullPath);

            headers = $"HTTP/1.1 {code} {massage}\r\nContent-Type: {contentType}\r\nContent-Length: " + Content.Length + "\r\n\r\n";
            headerBytes = System.Text.Encoding.UTF8.GetBytes(headers);
            // Send headers then file content
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(Content, 0, Content.Length);

        }

        private static void HandleErorr(int code, string massage, NetworkStream stream)
        {
            byte[] htmlContent;
            string headers;
            byte[] headerBytes;
            string fileName;
            string filePath;
            string fullPath;

            switch (code)
            {

                case 400:
                    fileName = "error400.html";
                    filePath = Path.Combine("..", "..", "..", Root, fileName);
                    fullPath = Path.GetFullPath(filePath);

                    htmlContent = File.ReadAllBytes(fullPath);

                    headers = $"HTTP/1.1 {code} {massage}\r\nContent-Type: text/html\r\nContent-Length: " + htmlContent.Length + "\r\n\r\n";
                    headerBytes = System.Text.Encoding.UTF8.GetBytes(headers);

                    break;


                case 403:
                    fileName = "error403.html";
                    filePath = Path.Combine("..", "..", "..", Root, fileName);
                    fullPath = Path.GetFullPath(filePath);

                    htmlContent = File.ReadAllBytes(fullPath);

                    headers = $"HTTP/1.1 {code} {massage}\r\nContent-Type: text/html\r\nContent-Length: " + htmlContent.Length + "\r\n\r\n";
                    headerBytes = System.Text.Encoding.UTF8.GetBytes(headers);

                    break;

                case 404:
                    fileName = "error404.html";
                    filePath = Path.Combine("..", "..", "..", Root, fileName);
                    fullPath = Path.GetFullPath(filePath);

                    htmlContent = File.ReadAllBytes(fullPath);

                    headers = $"HTTP/1.1 {code} {massage}\r\nContent-Type: text/html\r\nContent-Length: " + htmlContent.Length + "\r\n\r\n";
                    headerBytes = System.Text.Encoding.UTF8.GetBytes(headers);

                    break;
                case 405:
                    fileName = "error405.html";
                    filePath = Path.Combine("..", "..", "..", Root, fileName);
                    fullPath = Path.GetFullPath(filePath);

                    htmlContent = File.ReadAllBytes(fullPath);

                    headers = $"HTTP/1.1 {code} {massage}\r\nContent-Type: text/html\r\nContent-Length: " + htmlContent.Length + "\r\n\r\n";
                    headerBytes = System.Text.Encoding.UTF8.GetBytes(headers);

                    break;
                default:

                    fileName = "error.html";
                    filePath = Path.Combine("..", "..", "..", Root, fileName);
                    fullPath = Path.GetFullPath(filePath);

                    htmlContent = File.ReadAllBytes(fullPath);

                    headers = $"HTTP/1.1 {code} {massage}\r\nContent-Type: text/html\r\nContent-Length: " + htmlContent.Length + "\r\n\r\n";
                    headerBytes = System.Text.Encoding.UTF8.GetBytes(headers);

                    break;

            }

            // Send headers then file content
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(htmlContent, 0, htmlContent.Length);



        }

        static void Main(string[] args)
        {
            StartServer();
            Console.WriteLine("Start Web Server ");
        }
    }

}