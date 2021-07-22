using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResBase_Server
{
    class Program
    {
        public static OleDbConnection DB;
        static int port = 90;
        static TcpListener Listner = new TcpListener(IPAddress.Any, port);
        static async Task Main(string[] args)
        {
            string connecting = "provider=Microsoft.ACE.OLEDB.12.0;Data Source=access.mdb";
            DB = new OleDbConnection(connecting);
            DB.Open();
            
            Console.Title = "ResBase Server";
            Console.WriteLine("[*] Starting...");

            Listner.Start();

            while (true)
            {
                var client = await Listner.AcceptTcpClientAsync();
                Thread thread = new Thread(() =>
                {
                    Read(client);

                });
                thread.Start();
            }
        }

        static void Read(TcpClient client)
        {
            try
            {
                byte[] data = new byte[256];
                int count = 0;
                string request = "";
                while ((count = client.GetStream().Read(data, 0, data.Length)) > 0)
                {
                    request += Encoding.ASCII.GetString(data, 0, count);
                    var reqs = request.Split("\r\n");
                    foreach (var req in reqs)
                    {
                        if (req != "")
                            DO(client, req);
                    }
                }
                Console.WriteLine("[!] Stream end");
            }
            catch (IOException)
            {
                Console.WriteLine("Client disconnected");
            }
            
        }
        static void SendMenu()
        {
            var com = "SELECT * from Menu";
            OleDbCommand command = new OleDbCommand(com, DB);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception)
            {

                throw;
            }
            OleDbDataAdapter adapter = new OleDbDataAdapter(command);
            adapter.SelectCommand = command;
            DataTable table = new DataTable();

            adapter.Fill(table);

            
        }
        static void DO(TcpClient client, string command)
        {
            Console.WriteLine("------------");
            Console.WriteLine(command);
            var buffer = Encoding.ASCII.GetBytes($"HI {command}\r\n");
            client.GetStream().Write(buffer, 0, buffer.Length);
            Console.WriteLine("------------");
        }
    }
}
