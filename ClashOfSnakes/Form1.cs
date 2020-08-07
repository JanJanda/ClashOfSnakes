using System;
using System.Collections.Generic;
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
        const int delay = 200; //constants for game configuration
        const int mapWidth = 1000;
        const int mapHeight = 800;
        const int blockEdge = 20;
        Button connect; //control buttons
        Button create;
        Button single;
        Label L1; //labels with scores
        Label L2;
        Label info; //label with generec informations about the program
        TextBox addr; //textbox for the challengers IP address
        readonly Bitmap ground = Properties.Resources.ground;
        SinglePGame game;
        Direction directionA; //desired direction of the playerAs motion
        Direction directionB; //desired direction of the playerBs motion
        Timer t1; //main game timer
        WhoAmI me; //tells who is the local player
        bool connectionFail; //tells if the connection failed
        bool multi; //tells if the game is in multiplayer mode
        bool dontChangeDirection; //blocks the local player from changing his keyboard input
        Networking net;

        public GameWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Paints ground
        /// </summary>
        /// <param name="gr"></param>
        private void MakeGround(Graphics gr)
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
        private void MakeUI()
        {
            connect = new Button();
            create = new Button();
            single = new Button();
            connect.Click += Connect_Click;
            create.Click += Create_Click;
            single.Click += Single_Click;
            ConfigButton(connect);
            ConfigButton(create);
            ConfigButton(single);
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
            ConfigLabel(L1);
            L1.Location = new Point(20, 825);
            L1.ForeColor = Color.Green;
            Controls.Add(L1);
            L2 = new Label();
            ConfigLabel(L2);
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
            t1.Interval = delay;
            t1.Tick += T1_Tick;
        }

        /// <summary>
        /// Configures common properties of a button
        /// </summary>
        /// <param name="b">The button to be configured</param>
        private void ConfigButton(Button b)
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
        private void ConfigLabel(Label l)
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
        private void Reset()
        {
            MessageBox.Show("Game Over!", "Clash Of Snakes", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            directionA = Direction.right;
            directionB = Direction.left;
            L1.Text = "0";
            L2.Text = "0";
            if (game != null)
            {
                game.Reset();
                t1.Start();
            }
            Invalidate();
        }

        /// <summary>
        /// Makes one move of the game. Usable for multiplayer as well as singleplayer. Displays concurrent score of a player or both players.
        /// </summary>
        private void Game_Move()
        {
            if (connectionFail)
            {
                L1.Visible = false;
                L2.Visible = false;
                info.Text = "Connection lost!";
                info.Visible = true;
                return;
            }
            if (game == null) return;

            Scores sc = game.MakeMove(directionA, directionB);
            dontChangeDirection = false;
            L1.Text = sc.A.ToString();
            L2.Text = sc.B.ToString();
            Invalidate();

            if (sc.gameOver)
            {
                Reset();
                return;
            }

            t1.Start();
        }

        /// <summary>
        /// Game timer. Takes care of network communication. Asynchronously reads network.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void T1_Tick(object sender, EventArgs e)
        {
            t1.Stop();
            dontChangeDirection = true;
            if (multi)
            {
                WriteNet();
                try
                {
                    await ReadNetAsync();
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
            Game_Move();
        }

        /// <summary>
        /// Sends out to netword data about current local move.
        /// </summary>
        private void WriteNet()
        {
            Direction tmp;
            if (me == WhoAmI.playerA) tmp = directionA;
            else tmp = directionB;
            try
            {
                switch (tmp)
                {
                    case Direction.up:
                        net.DataOut.WriteLine("w");
                        break;
                    case Direction.left:
                        net.DataOut.WriteLine("a");
                        break;
                    case Direction.down:
                        net.DataOut.WriteLine("s");
                        break;
                    case Direction.right:
                        net.DataOut.WriteLine("d");
                        break;
                }
            }
            catch (IOException)
            {
                connectionFail = true;
            }
        }

        /// <summary>
        /// Reads incoming data about move of the opponent asynchronously.
        /// </summary>
        private async Task ReadNetAsync()
        {
            string s;
            try
            {
                s = await net.DataIn.ReadLineAsync();
            }
            catch (IOException)
            {
                connectionFail = true;
                return;
            }

            Direction tmp;
            if (me == WhoAmI.playerA) tmp = directionB;
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
            if (me == WhoAmI.playerA) directionB = tmp;
            else directionA = tmp;
        }

        /// <summary>
        /// Connect button click event handeler. Initiates client connection to a game server asynchronously.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Connect_Click(object sender, EventArgs e)
        {
            if (!addr.Visible)
            {
                t1.Stop(); //config UI
                net?.RenewAll();
                net = new Networking();
                L1.Visible = false;
                L2.Visible = false;
                info.Text = "Enter opponent's IP address and Connect:";
                info.Visible = true;
                addr.Text = "";
                addr.Visible = true;
                game = null;
                directionA = Direction.right;
                directionB = Direction.left;
                me = WhoAmI.playerB;
                connectionFail = false;
                multi = true;
                dontChangeDirection = false;
                Invalidate();
            }
            else
            {
                addr.Visible = false;
                if (IPAddress.TryParse(addr.Text, out IPAddress ad))
                {
                    info.Text = "Connecting to " + ad.ToString() + "...";
                    int receivedSeed;
                    try
                    {
                        await net.ConnectToChallengerAsync(ad);
                        receivedSeed = int.Parse(await net.DataIn.ReadLineAsync());
                    }
                    catch (SocketException)
                    {
                        info.Text = "Connection failed!";
                        return;
                    }
                    catch (IOException)
                    {
                        info.Text = "Connection lost!";
                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    BeginClientMultiplayer(receivedSeed);
                }
                else info.Text = "Can not understand the address!";
            }
        }

        /// <summary>
        /// Begins multiplayer as a client. Sets the program for multiplayer game as playerB.
        /// </summary>
        private void BeginClientMultiplayer(int receivedSeed)
        {
            t1.Stop();
            game = new MultiPGame(mapWidth, mapHeight, blockEdge, receivedSeed);
            Invalidate();
            me = WhoAmI.playerB;
            multi = true;
            info.Visible = false;
            L1.Text = "0";
            L2.Text = "0";
            L1.Visible = true;
            L2.Visible = true;
            t1.Start();
        }

        /// <summary>
        /// Handles the Create multiplayer button click event ant initiates process of creating a game as a server asynchronously.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Create_Click(object sender, EventArgs e)
        {
            t1.Stop(); //config UI
            net?.RenewAll();
            net = new Networking();
            L1.Visible = false;
            L2.Visible = false;
            info.Text = "";
            info.Visible = true;
            addr.Visible = false;
            game = null;
            directionA = Direction.right;
            directionB = Direction.left;
            me = WhoAmI.playerA;
            connectionFail = false;
            multi = true;
            dontChangeDirection = false;
            Invalidate();

            if (!NetworkInterface.GetIsNetworkAvailable()) //check for network
            {
                info.Text = "No network detected!";
                return;
            }

            IEnumerable<string> ipv4addrs = net.MyAddresses();
            string preamble = "This computer has these addresses:\n\n";
            string opt = "no address available";
            if (ipv4addrs.Count() > 0)
            {
                opt = "";
                foreach (string s in ipv4addrs) opt += s;
            }
            Task.Run(() => MessageBox.Show(preamble + opt, "Clash of Snakes", MessageBoxButtons.OK, MessageBoxIcon.Information)); //No need to wait for the completion of this task. Ignore possible warnings.

            info.Text = "Waiting for opponent...";
            try
            {
                await net.AcceptOpponentAsync();
            }
            catch (SocketException)
            {
                info.Text = "Socket is busy!";
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            BeginMultiplayer();
        }

        /// <summary>
        /// Sets the program for multiplayer as a server.
        /// </summary>
        private void BeginMultiplayer()
        {
            L1.Text = "0"; //config UI
            L2.Text = "0";
            L1.Visible = true;
            L2.Visible = true;
            info.Visible = false;
            addr.Visible = false;
            directionA = Direction.right;
            directionB = Direction.left;

            Random r = new Random();
            int seed = r.Next();
            game = new MultiPGame(mapWidth, mapHeight, blockEdge, seed);
            net.DataOut.WriteLine(seed);
            multi = true;
            t1.Start();
            Invalidate();
        }

        /// <summary>
        /// Handles the Singleplayer button click event and creates new singleplayer game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Single_Click(object sender, EventArgs e)
        {
            t1.Stop(); //config UI
            net?.RenewAll();
            L1.Text = "0";
            L1.Visible = true;
            L2.Visible = false;
            info.Visible = false;
            addr.Visible = false;
            directionA = Direction.right;
            me = WhoAmI.playerA;
            connectionFail = false;
            multi = false;
            dontChangeDirection = false;

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
            MakeUI();
        }

        /// <summary>
        /// Repaints the game window and the game map if necessary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameWindow_Paint(object sender, PaintEventArgs e)
        {
            MakeGround(e.Graphics);
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
            if (me == WhoAmI.playerA) tmp = directionA;
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

            if (me == WhoAmI.playerA) directionA = tmp;
            else directionB = tmp;
        }

        /// <summary>
        /// Tells who is the local player. Singleplayer is playerA, server side multiplayer is playerA and client side multiplayer is playerB
        /// </summary>
        enum WhoAmI
        {
            playerA,
            playerB
        }
    }
}
