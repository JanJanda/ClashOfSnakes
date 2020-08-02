using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClashOfSnakes
{
    public partial class GameWindow : Form
    {
        const int delay = 180; //constants for game configuration
        const int read = 20;
        const int resetTime = 5000;
        const int mapWidth = 1000;
        const int mapHeight = 800;
        const int blockEdge = 20;
        const int port = 52217;
        Button connect;
        Button create;
        Button single;
        Label L1;
        Label L2;
        Label info;
        TextBox addr;
        readonly Bitmap ground = Properties.Resources.ground;
        SinglePGame game;
        Direction directionA;
        Direction directionB;
        bool validMessage;
        Timer t1;
        Timer t2;
        whoAmI me;
        StreamWriter dataOut;
        StreamReader dataIn;
        Timer waiting;
        TcpListener listener;
        bool connectionFail;
        bool multi;
        Timer connectWait;
        int receivedSeed;
        bool seedReceived;
        bool seedRecFailed;
        Timer waitForSeed;
        int connectAttempts;
        bool dontChangeDirection;
        TcpClient client;
        Timer waitForReset;

        public GameWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Paints ground
        /// </summary>
        /// <param name="gr"></param>
        private void makeGround(Graphics gr)
        {
            ground.SetResolution(gr.DpiX, gr.DpiY);
            for (int i = 0; i < mapWidth / blockEdge; i++)
            {
                for (int j = 0; j < mapHeight / blockEdge; j++)
                {
                    gr.DrawImage(ground, i * blockEdge, j * blockEdge);
                }
            }
        }

        /// <summary>
        /// Makes buttons, labels, textbox, timers, ... Used for inicialization.
        /// </summary>
        private void makeUI()
        {
            connect = new Button();
            create = new Button();
            single = new Button();
            connect.Click += Connect_Click;
            create.Click += Create_Click;
            single.Click += Single_Click;
            configButton(connect);
            configButton(create);
            configButton(single);
            connect.Text = "Connect";
            connect.Location = new Point(680, 830);
            Controls.Add(connect);
            create.Text = "Create Multiplayer";
            create.Location = new Point(780, 830);
            Controls.Add(create);
            single.Text = "Singleplayer";
            single.Location = new Point(880, 830);
            Controls.Add(single);

            L1 = new Label();
            configLabel(L1);
            L1.Location = new Point(20, 825);
            L1.ForeColor = Color.Green;
            Controls.Add(L1);
            L2 = new Label();
            configLabel(L2);
            L2.Location = new Point(170, 825);
            L2.ForeColor = Color.Red;
            Controls.Add(L2);

            addr = new TextBox();
            addr.Width = 100;
            addr.Height = 20;
            addr.BackColor = Color.Beige;
            addr.Location = new Point(560, 840);
            addr.Visible = false;
            Controls.Add(addr);

            info = new Label();
            info.Font = new Font("Arial", 20);
            info.Height = 40;
            info.Width = 550;
            info.Location = new Point(0, 830);
            info.ForeColor = Color.Beige;
            info.Visible = false;
            Controls.Add(info);

            t1 = new Timer();
            t1.Tick += T1_Tick;

            t2 = new Timer();
            t2.Tick += T2_Tick;

            waiting = new Timer();
            waiting.Interval = 1000;
            waiting.Tick += Waiting_Tick;

            connectWait = new Timer();
            connectWait.Interval = 1000;
            connectWait.Tick += ConnectWait_Tick;

            waitForSeed = new Timer();
            waitForSeed.Interval = 500;
            waitForSeed.Tick += WaitForSeed_Tick;

            waitForReset = new Timer();
            waitForReset.Interval = resetTime;
            waitForReset.Tick += WaitForReset_Tick;
        }

        /// <summary>
        /// Configures common properties of a button
        /// </summary>
        /// <param name="b">The button to be configured</param>
        private void configButton(Button b)
        {
            b.Height = 40;
            b.Width = 80;
            b.BackColor = Color.Beige;
            b.ForeColor = Color.Black;
        }

        /// <summary>
        /// Configures common properties of a label
        /// </summary>
        /// <param name="l">The label to be configured</param>
        private void configLabel(Label l)
        {
            l.Font = new Font("Arial", 40);
            l.Height = 60;
            l.Width = 130;
            l.Visible = false;
        }

        /// <summary>
        /// Resets the current game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaitForReset_Tick(object sender, EventArgs e)
        {
            waitForReset.Stop();
            directionA = Direction.right;
            directionB = Direction.left;
            if (game != null)
            {
                game.Reset();
                t2.Start();
            }
            Invalidate();
        }

        /// <summary>
        /// Main game timer. Makes game move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void T1_Tick(object sender, EventArgs e)
        {
            if (connectionFail)
            {
                L1.Visible = false;
                L2.Visible = false;
                info.Text = "Connection lost!";
                info.Visible = true;
                t1.Stop();
                return;
            }
            if (!validMessage)
            {
                return;
            }

            Scores sc = game?.MakeMove(directionA, directionB) ?? new Scores();
            dontChangeDirection = false;
            L1.Text = sc.A.ToString();
            L2.Text = sc.B.ToString();
            Invalidate();

            if (multi)
            {
                validMessage = false;
                t1.Stop();
                if (sc.gameOver) waitForReset.Start();
                else t2.Start();
            }
        }

        /// <summary>
        /// Multiplayer game timer. Takes care of network communication
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void T2_Tick(object sender, EventArgs e)
        {
            t2.Stop();
            dontChangeDirection = true;
            writeNet();
            Task.Run(readNet);
            t1.Start();
        }

        /// <summary>
        /// Sends out to netword data about current local move.
        /// </summary>
        private void writeNet()
        {
            Direction tmp;
            if (me == whoAmI.playerA) tmp = directionA;
            else tmp = directionB;
            try
            {
                switch (tmp)
                {
                    case Direction.up:
                        dataOut.WriteLine("w");
                        break;
                    case Direction.left:
                        dataOut.WriteLine("a");
                        break;
                    case Direction.down:
                        dataOut.WriteLine("s");
                        break;
                    case Direction.right:
                        dataOut.WriteLine("d");
                        break;
                }
            }
            catch (IOException)
            {
                connectionFail = true;
            }
        }

        /// <summary>
        /// Reads incoming data about move of the opponent.
        /// </summary>
        private void readNet()
        {
            string s;
            try
            {
                s = dataIn.ReadLine();
            }
            catch (IOException)
            {
                connectionFail = true;
                return;
            }

            Direction tmp;
            if (me == whoAmI.playerA) tmp = directionB;
            else tmp = directionA;
            switch (s)
            {
                case "w":
                    tmp = Direction.up;
                    break;
                case "a":
                    tmp = Direction.left;
                    break;
                case "s":
                    tmp = Direction.down;
                    break;
                case "d":
                    tmp = Direction.right;
                    break;
            }
            if (me == whoAmI.playerA) directionB = tmp;
            else directionA = tmp;
            validMessage = true;
        }

        /// <summary>
        /// Sets the program for multiplayer as a server.
        /// </summary>
        private void beginMultiplayer()
        {
            L1.Text = "0"; //config UI
            L2.Text = "0";
            L1.Visible = true;
            L2.Visible = true;
            info.Visible = false;
            addr.Visible = false;
            directionA = Direction.right;
            directionB = Direction.left;
            validMessage = false;

            Random r = new Random();
            int seed = r.Next();
            game = new MultiPGame(mapWidth, mapHeight, blockEdge, seed);
            dataOut.WriteLine(seed);
            multi = true;
            t1.Interval = read;
            t1.Stop();
            t2.Interval = delay;
            t2.Start();
            Invalidate();
        }

        /// <summary>
        /// Checks network for connecting opponent.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Waiting_Tick(object sender, EventArgs e)
        {
            if (listener != null && listener.Pending())
            {
                client = listener.AcceptTcpClient();
                client.NoDelay = true;
                Stream s = client.GetStream();
                dataIn = new StreamReader(s);
                dataOut = new StreamWriter(s);
                dataOut.AutoFlush = true;
                listener.Stop();
                waiting.Stop();
                me = whoAmI.playerA;
                beginMultiplayer();
            }
        }

        /// <summary>
        /// Connect button click event handeler. Initiates client connection to a game server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connect_Click(object sender, EventArgs e)
        {
            if (!addr.Visible)
            {
                t1.Stop(); //config UI
                t2.Stop();
                waiting.Stop();
                waitForReset.Stop();
                connectWait.Stop();
                waitForSeed.Stop();
                L1.Visible = false;
                L2.Visible = false;
                info.Text = "Enter opponent's IP address and Connect:";
                info.Visible = true;
                addr.Text = "";
                addr.Visible = true;
                game = null;
                directionA = Direction.right;
                directionB = Direction.left;
                validMessage = false;
                me = whoAmI.playerB;
                dataIn = null;
                dataOut = null;
                listener?.Stop();
                listener = null;
                connectionFail = false;
                multi = true;
                seedRecFailed = false;
                seedReceived = false;

                dontChangeDirection = false;
                client?.Close();
                client = null;
                Invalidate();
            }
            else
            {
                addr.Visible = false;
                if (IPAddress.TryParse(addr.Text, out IPAddress ad))
                {
                    info.Text = "Connecting to " + ad.ToString() + "...";
                    dataIn = null;
                    dataOut = null;
                    connectAttempts = 20;
                    connectWait.Start();
                    Task.Run(() => makeConnection(ad));
                }
                else info.Text = "Can not understand the address!";
            }
        }


        /// <summary>
        /// Tries to make connection to a game server on the given IP address.
        /// </summary>
        /// <param name="a">The IP address to connect to</param>
        private void makeConnection(IPAddress a)
        {
            client = new TcpClient();
            client.NoDelay = true;
            try
            {
                client.Connect(a, port);
            }
            catch (SocketException)
            {
                return;
            }
            Stream s = client.GetStream();
            dataIn = new StreamReader(s);
            dataOut = new StreamWriter(s);
            dataOut.AutoFlush = true;
        }

        /// <summary>
        /// Checks if the connection as a client was successful. Takes only set number of attempts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectWait_Tick(object sender, EventArgs e)
        {
            if (connectAttempts < 0)
            {
                connectWait.Stop();
                info.Text = "Connection failed!";
                return;
            }
            if (dataIn != null)
            {
                connectWait.Stop();
                seedReceived = false;
                seedRecFailed = false;
                waitForSeed.Start();
                Task.Run(receiveSeed);
            }
            connectAttempts--;
        }

        /// <summary>
        /// Receives the game seed from the connected server.
        /// </summary>
        private void receiveSeed()
        {
            try
            {
                receivedSeed = int.Parse(dataIn.ReadLine());
                seedReceived = true;
            }
            catch (IOException)
            {
                seedRecFailed = true;
            }
        }

        /// <summary>
        /// Handles the received game seed from the game server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaitForSeed_Tick(object sender, EventArgs e)
        {
            if (seedRecFailed)
            {
                waitForSeed.Stop();
                info.Text = "Connection lost!";
                return;
            }
            if (seedReceived)
            {
                t1.Stop();
                t2.Stop();
                waitForSeed.Stop();
                game = new MultiPGame(mapWidth, mapHeight, blockEdge, receivedSeed);
                me = whoAmI.playerB;
                multi = true;
                info.Visible = false;
                L1.Visible = true;
                L2.Visible = true;
                t1.Interval = read;
                t2.Interval = delay;
                t2.Start();
            }
        }

        /// <summary>
        /// Handles the Create multiplayer button click event ant initiates process of creating a game as a server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Create_Click(object sender, EventArgs e)
        {
            t1.Stop(); //config UI
            t2.Stop();
            waitForReset.Stop();
            waiting.Stop();
            connectWait.Stop();
            waitForSeed.Stop();
            L1.Visible = false;
            L2.Visible = false;
            info.Text = "";
            info.Visible = true;
            addr.Visible = false;
            game = null;
            directionA = Direction.right;
            directionB = Direction.left;
            validMessage = false;
            me = whoAmI.playerA;
            dataOut = null;
            dataIn = null;
            listener?.Stop();
            listener = null;
            connectionFail = false;
            multi = true;
            seedReceived = false;
            seedRecFailed = false;
            dontChangeDirection = false;
            client?.Close();
            client = null;
            Invalidate();

            if (!NetworkInterface.GetIsNetworkAvailable()) //check for network
            {
                info.Text = "No network detected!";
                return;
            }

            IPAddress[] options = Dns.GetHostAddresses(Dns.GetHostName()); //show available addresses of this computer
            IEnumerable<string> ipv4addrs = from a in options where a.AddressFamily == AddressFamily.InterNetwork select a.ToString() + "\n";
            string preamble = "This computer has these addresses:\n\n";
            string opt = "no address available";
            if (ipv4addrs.Count() > 0)
            {
                opt = "";
                foreach (string s in ipv4addrs) opt += s;
            }
            Task.Run(() => MessageBox.Show(preamble + opt, "Clash of Snakes", MessageBoxButtons.OK, MessageBoxIcon.Information));

            info.Text = "Waiting for opponent..."; //start polling network
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            waiting.Start();
        }


        /// <summary>
        /// Handles the Singleplayer button click event and creates new singleplayer game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Single_Click(object sender, EventArgs e)
        {
            t1.Stop(); //config UI
            t2.Stop();
            waitForReset.Stop();
            waiting.Stop();
            connectWait.Stop();
            waitForSeed.Stop();
            L1.Text = "0";
            L1.Visible = true;
            L2.Visible = false;
            info.Visible = false;
            addr.Visible = false;
            directionA = Direction.right;
            validMessage = true;
            me = whoAmI.playerA;
            dataOut = null;
            dataIn = null;
            listener?.Stop();
            listener = null;
            connectionFail = false;
            multi = false;
            seedReceived = false;
            seedRecFailed = false;
            dontChangeDirection = false;
            t1.Interval = delay + read;
            client?.Close();
            client = null;

            Random r = new Random();
            game = new SinglePGame(mapWidth, mapHeight, blockEdge, r.Next());
            t1.Start();
            Invalidate();
        }

        /// <summary>
        /// Initializes the entire program.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameWindow_Load(object sender, EventArgs e)
        {
            ClientSize = new Size(mapWidth, mapHeight + 100);
            BackColor = Color.Black;
            Icon = Properties.Resources.icon;
            makeUI();
        }

        /// <summary>
        /// Repaints the game window and the game map if necessary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameWindow_Paint(object sender, PaintEventArgs e)
        {
            makeGround(e.Graphics);
            game?.Paint(e.Graphics);
        }

        /// <summary>
        /// Handles KeyDown as well as KeyUp events. Saves the instruction for the snake.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (dontChangeDirection) return;

            Direction tmp;
            if (me == whoAmI.playerA) tmp = directionA;
            else tmp = directionB;

            switch (e.KeyCode)
            {
                case Keys.W:
                    tmp = Direction.up;
                    break;
                case Keys.A:
                    tmp = Direction.left;
                    break;
                case Keys.S:
                    tmp = Direction.down;
                    break;
                case Keys.D:
                    tmp = Direction.right;
                    break;
            }

            if (me == whoAmI.playerA) directionA = tmp;
            else directionB = tmp;
        }

        /// <summary>
        /// Tells who is the local player. Singleplayer is playerA, server side multiplayer is playerA and client side multiplayer is playerB
        /// </summary>
        enum whoAmI
        {
            playerA,
            playerB
        }
    }
}
