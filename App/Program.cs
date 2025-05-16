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
        var listener = new TcpListener(IPAddress.Any, 36929);
        listener.Start();
        Console.WriteLine("Server started. Waiting for clients...");

        while (true)
        {
            var client = listener.AcceptTcpClient();
            var stream = client.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            string name = reader.ReadLine();

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
        var reader = new StreamReader(stream, Encoding.UTF8);

        try
        {
            while (true)
            {
                string message = reader.ReadLine();
                if (message == null) break;

                if (message.StartsWith("@"))
                {
                    // Private message format: @recipient message
                    int spaceIndex = message.IndexOf(' ');
                    if (spaceIndex > 1)
                    {
                        string recipient = message.Substring(1, spaceIndex - 1);
                        string actualMessage = message.Substring(spaceIndex + 1);
                        SendPrivateMessage(recipient, $"{name} (private): {actualMessage}");
                    }
                }
                else
                {
                    Broadcast($"{name}: {message}");
                }
            }
        }
        catch { }

        lock (clients)
        {
            clients.Remove(name);
        }
        Console.WriteLine($"{name} disconnected. Total clients: {clients.Count}");
    }

    static void SendPrivateMessage(string recipient, string message)
    {
        lock (clients)
        {
            if (clients.TryGetValue(recipient, out var client))
            {
                try
                {
                    var writer = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };
                    writer.WriteLine(message);
                }
                catch { }
            }
        }
    }



    static void Broadcast(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + "\n"); // Append newline

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
