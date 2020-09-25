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



namespace location
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>      
        /// 
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)] static extern bool FreeConsole();

        public static bool window = false;


        [STAThread]
        static void Main(string[] args)
        {
            string host = "whois.net.dcs.hull.ac.uk";
            int port = 43;
            string name = "";
            string location = "";
            int count = 0;
            string protocol = "";
            int timeoutPeriod = 1000;
            //This for loop runs through the arguements supplied in the command line and adds them to the relevant variables
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-h":
                        host = args[++i];
                        break;
                    case "-p":
                        port = int.Parse(args[++i]);
                        break;
                    case "-t":
                        timeoutPeriod = int.Parse(args[++i]);
                        break;
                    case "-w":
                        window = true;
                        break;
                    case "-h9":
                        protocol = args[i];
                        break;
                    case "-h0":
                        protocol = args[i];
                        break;
                    case "-h1":
                        protocol = args[i];
                        break;

                    default:
                        if (count == 0 && !args[i].Contains("-h"))
                        {
                            name = args[i];
                            count++;
                        }
                        else if (count == 1 && !args[i].Contains("-h"))
                        {
                            location = args[i];
                            count++;
                        }
                        break;
                }
            }
            //This checks whether the user has selected to use the UI mode or console in the command line
            if (window == true)
            {
                //Disables console
                FreeConsole();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //Values from the command line are passed to the form
                Application.Run(new Form1(host, port, timeoutPeriod, protocol, name, location));
            }
            else
            {
                //Values from the command line are passed to the main program
                logic.startClient(host, port, timeoutPeriod, protocol, name, location);
            }


        }
    }
    static class logic
    {
        public static void startClient(string host, int port, int timeoutPeriod, string protocol, string name, string location)
        {
            string command = "";
            string outputMessage = "";
            try
            {
                string response = "";
                TcpClient client = new TcpClient();
                //Time out length is set
                client.ReceiveTimeout = timeoutPeriod;
                client.SendTimeout = timeoutPeriod;
                client.Connect(host, port);
                StreamWriter sw = new StreamWriter(client.GetStream());
                StreamReader sr = new StreamReader(client.GetStream());
                //Here is where the type of protocol being used is checked
                //Then the location value is checked, if it is empty then we know it is a search request

                if (protocol == "-h9")
                {
                    if (location == "")
                    {
                        //The command is formatted correctly here and added to a string
                        //The command is then sent using a streamwriter to the server
                        //The flush command sends the packet to the server 
                        //The readResponse method is called and the returned value is added to a string
                        //The response is then split up and gets added to a string array.
                        command = "GET /" + name + "\r\n";
                        sw.Write(command);
                        sw.Flush();
                        response = readResponse(sr);
                        string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        //This checks the response has the correct amount of lines
                        //The first line is checked to ensure the right response has been recieved
                        //The name and location is then added to the output message.
                        //If the response is not correct the output message is set to an error.
                        if (lines.Length > 2)
                        {
                            if (port == 80)
                            {
                                Console.WriteLine(response);
                            }
                            else
                            {
                                if (lines[0] == "HTTP/0.9 200 OK")
                                {
                                    outputMessage = name + " is " + lines[3];
                                }
                                else
                                {
                                    outputMessage = "ERROR: No entries found";
                                }
                            }

                        }
                    }
                    else
                    {
                        //The command is formatted, added to a string and then sent to the server
                        //The response is checked and the correct output message set.
                        command = "PUT /" + name + "\r\n\r\n" + location + "\r\n";
                        sw.Write(command);
                        sw.Flush();
                        response = readResponse(sr);

                        if (response == "HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n")
                        {
                            outputMessage = name + " location changed to be " + location;
                        }
                        else
                        {
                            outputMessage = "ERROR: Location couldn't be changed";
                        }

                    }
                }
                else if (protocol == "-h0")
                {
                    if (location == "")
                    {
                        //The command is formatted, added to a string and then sent to the server
                        //The response is checked and the correct output message set.
                        command = "GET /?" + name + " HTTP/1.0\r\n\r\n";
                        sw.Write(command);
                        sw.Flush();
                        response = readResponse(sr);
                        string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        if (lines[0] == "HTTP/1.0 200 OK")
                        {
                            outputMessage = name + " is " + lines[3];
                        }
                        else
                        {
                            outputMessage = "ERROR: No entries found";
                        }
                    }
                    else
                    {
                        //The command is formatted, added to a string and then sent to the server
                        //The response is checked and the correct output message set.
                        command = "POST /" + name + " HTTP/1.0\r\nContent-Length: " + location.Length + "\r\n\r\n" + location;
                        sw.Write(command);
                        sw.Flush();
                        response = readResponse(sr);
                        if (response == "HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n")
                        {
                            outputMessage = name + " location changed to be " + location;
                        }
                        else
                        {
                            outputMessage = "ERROR: Location couldn't be changed";
                        }
                    }
                }

                else if (protocol == "-h1")
                {
                    if (location == "")
                    {
                        //The command is formatted, added to a string and then sent to the server
                        //The response is checked and the correct output message set.
                        command = "GET /?name=" + name + " HTTP/1.1\r\nHost: " + host + "\r\n\r\n";
                        sw.Write(command);
                        sw.Flush();
                        response = readResponse(sr);
                        string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        if (lines[0] == "HTTP/1.1 200 OK")
                        {
                            outputMessage = name + " is " + lines[3];
                        }
                        else
                        {
                            outputMessage = "ERROR: No entries found";
                        }
                    }
                    else
                    {
                        //The nameLocation string formats the name and location in the correct way so that its legnth can be added to the command
                        //The command is formatted, added to a string and then sent to the server
                        //The response is checked and the correct output message set.
                        string namelocation = "name=" + name + "&location=" + location;
                        command = "POST / HTTP/1.1\r\nHost: " + host + "\r\nContent-Length: " + namelocation.Length + "\r\n\r\nname=" + name + "&location=" + location;
                        sw.Write(command);
                        sw.Flush();
                        response = readResponse(sr);
                        if (response == "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n")
                        {
                            outputMessage = name + " location changed to be " + location;
                        }
                        else
                        {
                            outputMessage = "ERROR: Location couldn't be changed";
                        }
                    }
                }

                else
                {
                    if (location == "")
                    {
                        //The name is sent to the server and the response is added to the output message if it is positve
                        //If not and error message is given
                        sw.WriteLine(name);
                        sw.Flush();
                        response = sr.ReadToEnd();
                        if (response == "ERROR: no entries found")
                        {
                            outputMessage = "ERROR: No entries found";
                        }
                        else
                        {
                            outputMessage = name + " is " + response;
                        }
                    }
                    else
                    {
                        //The name and location is sent to the server and the response is added to the output message.
                        sw.WriteLine(name + " " + location);
                        sw.Flush();
                        response = readResponse(sr);
                        string trimmed = response.Trim();
                        if (trimmed == "OK")
                        {

                            outputMessage = (name + " location changed to be " + location);
                        }
                        else
                        {
                            outputMessage = "ERROR: Location couldn't be changed";
                        }
                    }
                }
                //If the program is in UI mode the output message is written in a message box 
                // if it is not in UI mode then it is written to the console.
                if (Program.window == true)
                {
                    MessageBox.Show(outputMessage);
                }
                else
                {
                    Console.WriteLine(outputMessage);
                }
            }
            catch (Exception e)
            {
                outputMessage = "Error: Timed out";                
            }
        }
        //This method reads the reponse from the server using a stream reader.
        //A while loop goes through the the response until the characetr is null.

        static string readResponse(StreamReader sr)
        {
            try
            {
                int character;
                string strResponse = "";
                Thread.Sleep(10);
                character = sr.Read();
                int con = 0;
                while (character != -1)
                {
                    strResponse += (char)character;
                    character = sr.Read();

                    con++;
                    if (con == 10)
                    {
                        Thread.Sleep(10);
                        con = 0;
                    }
                }
                return strResponse;
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }
}
