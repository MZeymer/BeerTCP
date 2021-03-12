using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Beer_Testing;
using Newtonsoft.Json;

namespace BeerTCP
{
    class Server
    {
        private static List<Beer> _beers = new List<Beer>();
        public void Start()
        {
            TcpListener server = null;
            try
            {
                Int32 port = 4646;
                IPAddress localAddr = IPAddress.Loopback;
                server = new TcpListener(localAddr, port);
                server.Start();
                Console.WriteLine("Server started");
                while (true)
                {
                    TcpClient connectionSocket = server.AcceptTcpClient();
                    Task.Run(() =>
                    {
                        TcpClient tempSocket = connectionSocket;
                        DoClient(tempSocket);
                    });
                }
                server.Stop();

            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

        }

        private void DoClient(TcpClient connectionSocket)
        {

            Console.WriteLine("server activated");
            Stream ns = connectionSocket.GetStream();

            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);
            sw.AutoFlush = true;
            bool keepGoing = true;

            while (keepGoing)
            {
                string message = sr.ReadLine();
                switch (message)
                {
                    case "Stop":
                        keepGoing = false;
                        break;
                    case "HentAlle":
                        foreach (var beer in _beers)
                        {
                            sw.WriteLine(beer.ToString());
                        }
                        break;
                    case "Hent":
                        try
                        {
                            int id = Convert.ToInt32(sr.ReadLine());
                            sw.WriteLine(_beers.Find(beer => beer.Id == id));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        
                        break;
                    case "Gem":
                        string json = sr.ReadLine();
                        if (json != null)
                        {
                            try
                            {
                                Beer newBeer = JsonConvert.DeserializeObject<Beer>(json);
                                _beers.Add(newBeer);
                            }
                            catch (Exception ex)
                            {
                                sw.WriteLine(ex);
                            }
                            
                        }
                        else
                        {
                            sw.WriteLine("ingen øl tilføjet (Forkert/intet input)");
                        }
                        break;
                    default:
                        sw.WriteLine("Ikke en mulighed");
                        break;

                }
            }



            ns.Close();
            connectionSocket.Close();
        }
    }
}
