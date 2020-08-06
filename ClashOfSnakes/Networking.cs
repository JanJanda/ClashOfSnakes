using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClashOfSnakes
{
    class Networking
    {
        const int port = 52217;
        TcpListener listener;
        TcpClient client;
        public StreamWriter dataOut { get; private set; }
        public StreamReader dataIn { get; private set; }

        public void RenewAll()
        {
            dataIn?.Dispose();
            dataOut?.Dispose();
            client?.Close();
            listener?.Stop();            
        }

        public async Task AcceptOpponentAsync()
        {
            RenewAll();
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            client = await listener.AcceptTcpClientAsync();
            listener.Stop();
            client.NoDelay = true;
            Stream s = client.GetStream();
            dataIn = new StreamReader(s);
            dataOut = new StreamWriter(s);
            dataOut.AutoFlush = true;                 
        }

        public async Task ConnectToChallengerAsync(IPAddress adr)
        {
            RenewAll();
            client = new TcpClient();
            await client.ConnectAsync(adr, port);
            client.NoDelay = true;
            Stream s = client.GetStream();
            dataIn = new StreamReader(s);
            dataOut = new StreamWriter(s);
            dataOut.AutoFlush = true;
        }

        public IEnumerable<string> MyAddresses()
        {
            IPAddress[] options = Dns.GetHostAddresses(Dns.GetHostName()); //show available addresses of this computer
            return from a in options where a.AddressFamily == AddressFamily.InterNetwork select a.ToString() + "\n";
        }
    }
}
