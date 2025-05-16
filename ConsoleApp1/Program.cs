using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static Dictionary<string, TcpClient> clients = new();

    static void Main()
    {
        var listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Server started. Waiting for clients...");

        while (true)
        {
            var client = listener.AcceptTcpClient();
            var stream = client.GetStream();
            var buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string name = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

            lock (clients)
            {
                clients[name] = client;
            }

            Console.WriteLine($"{name} joined. Total clients: {clients.Count}");

            new Thread(() => HandleClient(name, client)).Start();
        }
    }

    static void HandleClient(string name, TcpClient client)
    {
        var stream = client.GetStream();
        var buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                Console.WriteLine($"{name}: {message}");
                Broadcast($"{name}: {message}");
            }
        }
        catch { }

        lock (clients)
        {
            clients.Remove(name);
        }
        Console.WriteLine($"{name} disconnected. Total clients: {clients.Count}");
    }

    static void Broadcast(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        lock (clients)
        {
            foreach (var client in clients.Values)
            {
                try
                {
                    var stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
                catch { }
            }
        }
    }
}
