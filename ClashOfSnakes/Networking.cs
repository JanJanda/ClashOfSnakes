using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ClashOfSnakes
{
    class Networking
    {
        const int port = 52217;
        TcpListener listener;
        TcpClient client;
        CancellationTokenSource cts;
        public StreamWriter DataOut { get; private set; } //network stream for sending data
        public StreamReader DataIn { get; private set; } //network stream for receiving data
        bool connectFinished; //tells if the method ConnectToChallenger is awaiting connecion

        /// <summary>
        /// Releases all resources used by this instance of networking.
        /// </summary>
        public void RenewAll()
        {
            cts?.Cancel();
            DataIn?.Dispose();
            DataOut?.Dispose();
            if (connectFinished) client?.Close(); //needs to be closed depending on assynchronous attempt for a connection
            listener?.Stop();
        }


        /// <summary>
        /// Creates a server and accepts the first opponent, that connects to it asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task AcceptOpponentAsync()
        {
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            client = await listener.AcceptTcpClientAsync();
            listener.Stop();
            token.ThrowIfCancellationRequested();
            client.NoDelay = true;
            Stream s = client.GetStream();
            DataIn = new StreamReader(s);
            DataOut = new StreamWriter(s);
            DataOut.AutoFlush = true;
        }

        /// <summary>
        /// Connects to a listening game server asynchronously.
        /// </summary>
        /// <param name="adr">The IP address of the listening game server</param>
        /// <returns></returns>
        public async Task ConnectToChallengerAsync(IPAddress adr)
        {
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            client = new TcpClient();
            connectFinished = false;
            await client.ConnectAsync(adr, port);
            connectFinished = true;
            if (token.IsCancellationRequested)
            {
                client.Close();
                throw new OperationCanceledException();
            }
            client.NoDelay = true;
            Stream s = client.GetStream();
            DataIn = new StreamReader(s);
            DataOut = new StreamWriter(s);
            DataOut.AutoFlush = true;
        }

        /// <summary>
        /// Creates a list of usable IP addresses of this computer.
        /// </summary>
        /// <returns>The list of this computers usable IP addresses</returns>
        public IEnumerable<string> MyAddresses()
        {
            IPAddress[] options = Dns.GetHostAddresses(Dns.GetHostName()); //show available addresses of this computer
            return from a in options where a.AddressFamily == AddressFamily.InterNetwork select a.ToString() + "\n";
        }
    }
}
