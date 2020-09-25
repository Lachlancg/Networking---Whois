using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace locationserver
{
    static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        public static Dictionary<string, string> savedResults = new Dictionary<string, string>();
        public static Logging log;
        public static saving save;
        public static string filename = null;
        public static string savefile = null;
        public static bool window = false;

        [STAThread]
        static void Main(string[] args)
        {
            //When the server is started the command line arguements are checked and if anything is present theyre added to variables
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-l":
                        filename = args[++i];
                        break;
                    case "-f":
                        savefile = args[++i];
                        break;
                    case "-w":
                        window = true;
                        break;
                    default:
                        Console.WriteLine("Unknown Option " + args[i]);
                        break;
                }
            }
            //The save and load objects are created here and the addreses passed.
            log = new Logging(filename);
            save = new saving(savefile);
            //The save file address is used to load the previous server save.
            logic.readFromFile(savefile);
            //If the server was started in window mode then a form is created, if not the console is used.
            if (window == true)
            {
                // Run windows forms app               
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(ref filename, ref savefile));
            }
            else
            {
                logic.runServer();                
            }
        }
    }
    static class logic
    {
        public static void runServer()
        {
            TcpListener listener;
            Socket connection;
            Handler RequestHandler;
            //The TcpListener is created using the servers ipaddress and port 43 is used 
            //A thread is created and the main server method is called.
            try
            {
                listener = new TcpListener(IPAddress.Any, 43);
                listener.Start();
                Console.WriteLine(@"Server started listening");
                while (true)
                {
                    connection = listener.AcceptSocket();
                    RequestHandler = new Handler();
                    Thread t = new Thread(() => RequestHandler.doRequest(connection, Program.log, Program.save));
                    t.Start();
                }
            }
            catch (Exception e)
            {
                Handler.status = "EXCEPTION";
            }
        }
        //This method is used to load the saved data into the dictionary that stores the users and locations
        public static void readFromFile(string saveFile)
        {
            if (saveFile == null)
            {
                return;
            }
            try
            {
                StreamReader reader = new StreamReader(saveFile);
                string line = null;
                string[] lines = null;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Replace("[", "");
                    line = line.Replace("]", "");
                    lines = line.Split(',');
                    Program.savedResults.Add(lines[0], lines[1]);
                }
                reader.Close();
            }
            catch (Exception)
            {

            }
        }
    }


    class Handler
    {
        public static string status = "OK";
        //This is the main server method 
        public void doRequest(Socket connection, Logging log, saving save)
        {
            //This gets the IP addrsess so it can be added to the log
            string host = ((IPEndPoint)connection.RemoteEndPoint).Address.ToString();
            string line = null;
            //This is added to the log to show the server is functioning correctly
            status = "OK";
            NetworkStream socketStream;
            socketStream = new NetworkStream(connection);
            try
            {
                //The timeout amounts are set here for the read and writes.
                socketStream.ReadTimeout = 1000;
                socketStream.WriteTimeout = 1000;

                StreamWriter sw = new StreamWriter(socketStream);
                StreamReader sr = new StreamReader(socketStream);
                //The client response is retrieved here, we loop through the streamreader until the value is equal to null.
                //The response is added to a string, the string is then split it up using the line breaks and spaces'.
                try
                {                 
                    char[] buffer = new char[1];
                    while (sr.Peek() > -1)
                    {
                        sr.Read(buffer, 0, buffer.Length);
                        line += buffer[0];
                    }

                }
                catch (Exception e)
                {
                    status = "TIMEOUT";
                }
                
                string[] lines = line.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                string[] firstLine = lines[0].Split(' ');
                //We then check which protocol is being used.
                //Once we know the protocol we can check for the type of request. 
                //If it is a search request we can return the users location.
                //If it is a post request add the user and their location to the dictionary
                if (lines.Length > 2)
                {
                    if (firstLine.Length > 2)
                    {
                        if (firstLine[2] == "HTTP/1.0")
                        {
                            if (firstLine[0] == "GET")
                            {
                                string search = firstLine[1].Replace("/?", "");
                                if (Program.savedResults.ContainsKey(search))
                                {
                                    string result = Program.savedResults[search];
                                    sw.Write("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + result + "\r\n");
                                    sw.Flush();
                                }
                                else
                                {
                                    sw.Write("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                                    sw.Flush();
                                    status = "NO ENTRY";
                                }
                            }
                            else if (firstLine[0] == "POST")
                            {
                                string firstWord = firstLine[1];
                                firstWord = firstWord.Replace("/", "");
                                string str = lines[3];
                                if (!Program.savedResults.ContainsKey(firstWord))
                                {
                                    Program.savedResults.Add(firstWord, str);
                                    sw.Write("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                                    sw.Flush();
                                }
                                else
                                {
                                    Program.savedResults[firstWord] = str;
                                    sw.Write("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                                    sw.Flush();
                                }
                            }
                        }
                        else if (firstLine[2] == "HTTP/1.1")
                        {

                            if (firstLine[0] == "GET")
                            {
                                string search = firstLine[1].Replace("/?name=", "");
                                if (Program.savedResults.ContainsKey(search))
                                {
                                    string result = Program.savedResults[search];
                                    sw.Write("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + result + "\r\n");
                                    sw.Flush();
                                }
                                else
                                {
                                    sw.Write("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                                    sw.Flush();
                                    status = "NO ENTRY";
                                }
                            }
                            if (firstLine[0] == "POST")
                            {
                                string name;
                                string location;
                                string[] temp = lines[4].Split('&');
                                name = temp[0];
                                location = temp[1];
                                name = name.Replace("name=", "");
                                location = location.Replace("location=", "");
                                if (!Program.savedResults.ContainsKey(name))
                                {
                                    Program.savedResults.Add(name, location);
                                    sw.Write("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                                    sw.Flush();
                                }
                                else
                                {
                                    Program.savedResults[name] = location;
                                    sw.Write("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                                    sw.Flush();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (firstLine[0] == "PUT")
                        {
                            string firstWord = firstLine[1];
                            firstWord = firstWord.Replace("/", "");
                            string str = lines[2];
                            if (!Program.savedResults.ContainsKey(firstWord))
                            {
                                Program.savedResults.Add(firstWord, str);
                                sw.Write("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                                sw.Flush();
                            }
                            else
                            {
                                Program.savedResults[firstWord] = str;
                                sw.Write("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                                sw.Flush();
                            }
                        }
                    }
                }
                else
                {
                    if (firstLine[0] == "GET")
                    {
                        string search = firstLine[1].Replace("/", "");
                        if (Program.savedResults.ContainsKey(search))
                        {
                            string result = Program.savedResults[search];
                            sw.Write("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + result + "\r\n");
                            sw.Flush();
                        }
                        else
                        {
                            sw.Write("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                            status = "NO ENTRY";
                        }
                    }
                    else
                    {
                        if (line.Contains(" "))
                        {
                            string firstWord = line.Split(' ')[0];
                            int i = line.IndexOf(" ") + 1;
                            string str = line.Substring(i);
                            if (!Program.savedResults.ContainsKey(firstWord))
                            {
                                Program.savedResults.Add(firstWord, str);
                                sw.Write("OK\r\n");
                                sw.Flush();
                            }
                            else
                            {
                                Program.savedResults[firstWord] = str;
                                sw.Write("OK\r\n");
                                sw.Flush();
                            }
                        }
                        else
                        {
                            string search = line.Replace("\r\n", "");
                            if (Program.savedResults.ContainsKey(search))
                            {
                                string result = Program.savedResults[search];
                                sw.Write(result);
                                sw.Flush();
                            }
                            else
                            {
                                sw.Write("ERROR: no entries found");
                                sw.Flush();
                                status = "NO ENTRY";
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                status = "EXCEPTION";
            }
            finally
            {
                //The connections are closed
                //The write to log method is called and so is the write to save file method
                socketStream.Close();
                connection.Close();
                log.WriteToLog(host, line, status);
                save.writeToFile();
            }
        }
    }
    public class saving
    {
        //This class allows the users and locations to be saved to a file and loaded when the server is restarted.
        public static string saveFile = null;
        public saving(string savefile)
        {
            saveFile = savefile;
        }
        private static readonly object locker = new object();
        //A thread is created so multiple users can use the server at the same time.
        //If there is a save file address then the dictionary is saved
        public void writeToFile()
        {
            lock (locker)
            {
                if (saveFile == null) return;
                try
                {
                    StreamWriter SW;
                    File.WriteAllText(saveFile, string.Empty);
                    SW = File.AppendText(saveFile);
                    foreach (var entry in Program.savedResults)
                    {
                        SW.WriteLine("[{0}, {1}]", entry.Key, entry.Value.Trim());
                    }
                    SW.Close();
                }
                catch (Exception)
                {
                    Console.WriteLine("Unable to write save file " + saveFile);
                }

            }
        }

    }
    public class Logging
    {
        //This class prints the log to the console and saves the log to a text file.
        public static string logFile = null;
        public Logging(string filename)
        {
            logFile = filename;
        }

        private static readonly object locker = new object();
        //Line breaks are replaced with spaces'
        //The log entry is formatted correctly and gets printed to the console
        //The entry is then saved to the log file as long as the address is present.
        public void WriteToLog(string hostname, string message, string status)
        {
            if (message != null)
            {
                message = message.Replace("\r\n", " ");
            }

            string line = hostname + " - - " + DateTime.Now.ToString("'['dd'/'MM'/'yyyy':'HH':'mm':'ss zz00']'") + " \"" + message + "\" " + status;

            lock (locker)
            {

                Console.WriteLine(line);

                if (logFile == null) return;
                try
                {
                    StreamWriter SW;
                    SW = File.AppendText(logFile);
                    SW.WriteLine(line);
                    SW.Close();
                }
                catch (Exception)
                {
                    Console.WriteLine("Unable to write log file " + logFile);
                }

            }
        }
    }
}
