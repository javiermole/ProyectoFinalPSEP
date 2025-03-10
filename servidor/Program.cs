// Servidor TCP modificado para interactuar con la API Rest
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;

namespace servidor
{
    public class StateObject
    {
        public Socket? workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    public class Server
    {
        private static int PORT = 11000;
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private static readonly HttpClient httpClient = new HttpClient();

        public static void StartListening()
        {
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    allDone.Reset();
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                allDone.Set();
                Socket listener = (Socket)ar.AsyncState!;
                Socket handler = listener.EndAccept(ar);

                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Accept error: {e.Message}");
            }
        }

        public static async void ReadCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState!;
                Socket handler = state.workSocket!;
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    string encryptedRequest = state.sb.ToString().Trim();
                    Console.WriteLine($"Received encrypted request: {encryptedRequest}");
                    
                    // Desencriptar la solicitud
                    string decryptedRequest = EncryptionHelper.Decrypt(encryptedRequest);
                    Console.WriteLine($"Decrypted request: {decryptedRequest}");
                    
                    string response = await FetchFromApi(decryptedRequest);
                    
                    // Encriptar la respuesta
                    string encryptedResponse = EncryptionHelper.Encrypt(response);
                    Send(handler, encryptedResponse);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Read error: {e.Message}");
            }
        }

        private static async Task<string> FetchFromApi(string endpoint)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync($"http://localhost:5000{endpoint}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error fetching data: {ex.Message}";
            }
        }

        private static void Send(Socket handler, string data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState!;
                handler.EndSend(ar);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Send error: {e.Message}");
            }
        }
    }

    public class Program
    {
        public static void Main()
        {
            Server.StartListening();
        }
    }
}
