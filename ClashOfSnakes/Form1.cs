using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClashOfSnakes
{
    public partial class GameWindow : Form
    {
        const int mapWidth = 1000;
        const int mapHeight = 800;
        const int blockEdge = 20;
        Button connect;
        Button create;
        Button single;
        Label L1;
        Label L2;
        Label info;
        TextBox addr;
        Image ground = Properties.Resources.ground;

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
            ((Bitmap)ground).SetResolution(gr.DpiX, gr.DpiY);
            for (int i = 0; i < mapWidth / blockEdge; i++)
            {
                for (int j = 0; j < mapHeight / blockEdge; j++)
                {
                    gr.DrawImage(ground, i * blockEdge, j * blockEdge);
                }
            }
        }

        /// <summary>
        /// Makes buttons, labels and textbox
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
            addr.Width = 130;
            addr.Height = 20;
            addr.BackColor = Color.Beige;
            addr.Location = new Point(530, 840);
            addr.Visible = false;
            Controls.Add(addr);

            info = new Label();            
            info.Font = new Font("Arial", 20);
            info.Height = 40;
            info.Width = 650;
            info.Location = new Point(0, 830);
            info.ForeColor = Color.Beige;
            info.Visible = false;
            Controls.Add(info);
        }

        private void configButton(Button b)
        {
            b.Height = 40;
            b.Width = 80;
            b.BackColor = Color.Beige;
            b.ForeColor = Color.Black;
        }

        private void configLabel(Label l)
        {
            l.Font = new Font("Arial", 40);
            l.Height = 60;
            l.Width = 130;
            l.Visible = false;
        }

        private void Connect_Click(object sender, EventArgs e)
        {
            
        }

        private void Create_Click(object sender, EventArgs e)
        {
            
        }

        private void Single_Click(object sender, EventArgs e)
        {

        }

        private void GameWindow_Load(object sender, EventArgs e)
        {
            this.ClientSize = new Size(mapWidth, mapHeight + 100);
            this.BackColor = Color.Black;
            this.Icon = Properties.Resources.icon;
            makeUI();
        }

        private void GameWindow_Paint(object sender, PaintEventArgs e)
        {
            makeGround(e.Graphics);
        }
    }
}
