using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClashOfSnakes
{
    class Player
    {
        Image[] heads = new Image[4];
        Image[] bodies = new Image[6];
        Image[] tails = new Image[4];
        Image[][] parts;
        readonly int mapWidth; //in blocks
        readonly int mapHeight; //in blocks
        readonly int blockEdge; //in pixels
        int headX;
        int headY;
        PartID[,] map;
        
        /// <summary>
        /// Creates new player
        /// </summary>
        /// <param name="clr">Color of the players snake</param>
        /// <param name="width">Width of the map, PIXELS</param>
        /// <param name="height">Height of the map, PIXELS</param>
        /// <param name="edge">Length of the edge of a block, PIXELS, must divide width and height</param>
        public Player(SnakeColor clr, int width, int height, int edge)
        {
            if (width % edge != 0 || height % edge != 0) throw new ArgumentException();
            mapWidth = width / edge;
            mapHeight = height / edge;
            blockEdge = edge;
            map = new PartID[mapWidth, mapHeight];

            switch (clr)
            {
                case SnakeColor.green:
                    heads[0] = Properties.Resources.headA1;
                    heads[1] = Properties.Resources.headA2;
                    heads[2] = Properties.Resources.headA3;
                    heads[3] = Properties.Resources.headA4;
                    bodies[0] = Properties.Resources.snakeA1;
                    bodies[1] = Properties.Resources.snakeA2;
                    bodies[2] = Properties.Resources.snakeA3;
                    bodies[3] = Properties.Resources.snakeA4;
                    bodies[4] = Properties.Resources.snakeA5;
                    bodies[5] = Properties.Resources.snakeA6;
                    tails[0] = Properties.Resources.tailA1;
                    tails[1] = Properties.Resources.tailA2;
                    tails[2] = Properties.Resources.tailA3;
                    tails[3] = Properties.Resources.tailA4;
                    headX = 1;
                    headY = 0;
                    map[1, 0] = new PartID(0, 0);
                    map[0, 0] = new PartID(2, 0);
                    break;
                case SnakeColor.red:
                    heads[0] = Properties.Resources.headB1;
                    heads[1] = Properties.Resources.headB2;
                    heads[2] = Properties.Resources.headB3;
                    heads[3] = Properties.Resources.headB4;
                    bodies[0] = Properties.Resources.snakeB1;
                    bodies[1] = Properties.Resources.snakeB2;
                    bodies[2] = Properties.Resources.snakeB3;
                    bodies[3] = Properties.Resources.snakeB4;
                    bodies[4] = Properties.Resources.snakeB5;
                    bodies[5] = Properties.Resources.snakeB6;
                    tails[0] = Properties.Resources.tailB1;
                    tails[1] = Properties.Resources.tailB2;
                    tails[2] = Properties.Resources.tailB3;
                    tails[3] = Properties.Resources.tailB4;
                    headX = mapWidth - 2;
                    headY = mapHeight - 1;
                    map[mapWidth - 2, mapHeight - 1] = new PartID(0, 2);
                    map[mapWidth - 1, mapHeight - 1] = new PartID(2, 2);
                    break;
                default:
                    throw new ArgumentException();
            }

            parts = new Image[3][];
            parts[0] = heads;
            parts[1] = bodies;
            parts[2] = tails;
        }

        /// <summary>
        /// Paints the snake on the ground
        /// </summary>
        /// <param name="gr">Drawing surface</param>
        public void Paint(Graphics gr)
        {
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    if (map[i, j].valid)
                    {
                        Bitmap pic = (Bitmap)parts[map[i, j].x][map[i, j].y];
                        pic.SetResolution(gr.DpiX, gr.DpiY);
                        pic.MakeTransparent(Color.White);
                        gr.DrawImage(pic, i * blockEdge, j * blockEdge);
                    }
                }
            }
        }


        struct PartID
        {
            public PartID(byte first, byte second)
            {
                x = first;
                y = second;
                valid = true;
            }
            public byte x;
            public byte y;
            public bool valid;
        }       
    }
    enum SnakeColor
    {
        green,
        red
    }
    enum Direction
    {
        straight,
        left,
        right
    }
}
