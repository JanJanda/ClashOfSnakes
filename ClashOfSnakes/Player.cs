using System;
using System.Drawing;

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
        int hx;
        public int headX { get { return hx; } private set { hx = value.posMod(mapWidth); } }
        int hy;
        public int headY { get { return hy; } private set { hy = value.posMod(mapHeight); } }
        int tx;
        public int tailX { get { return tx; } private set { tx = value.posMod(mapWidth); } }
        int ty;
        public int tailY { get { return ty; } private set { ty = value.posMod(mapHeight); } }
        public int length { get; private set; }
        PartID[,] map;

        /// <summary>
        /// Creates new player
        /// </summary>
        /// <param name="clr">Color of the players snake</param>
        /// <param name="width">Width of the map, BLOCKS</param>
        /// <param name="height">Height of the map, BLOCKS</param>
        /// <param name="edge">Length of the edge of a block, PIXELS</param>
        public Player(SnakeColor clr, int width, int height, int edge)
        {
            mapWidth = width;
            mapHeight = height;
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
                    headX = 2;
                    headY = 0;
                    tailX = 0;
                    tailY = 0;
                    map[2, 0] = new PartID(0, 0);
                    map[1, 0] = new PartID(1, 0);
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
                    headX = mapWidth - 3;
                    headY = mapHeight - 1;
                    tailX = mapWidth - 1;
                    tailY = mapHeight - 1;
                    map[mapWidth - 3, mapHeight - 1] = new PartID(0, 2);
                    map[mapWidth - 2, mapHeight - 1] = new PartID(1, 0);
                    map[mapWidth - 1, mapHeight - 1] = new PartID(2, 2);
                    break;
                default:
                    throw new ArgumentException();
            }
            length = 3;

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

        /// <summary>
        /// Moves the snake in the given direction and possibly stretches the snake
        /// </summary>
        /// <param name="direc">The direction of required motion</param>
        /// <param name="stretch">Will the snake be stretched</param>
        public bool Move(Direction direc, bool stretch = false)
        {
            PartID head = map[headX, headY];
            if (stretch)
            {
                length++;
                return moveHead(direc);
            }
            else
            {
                moveTail();
                return moveHead(direc);
            }
        }

        /// <summary>
        /// Tels if the snake is on the given position of the map
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns></returns>
        public bool Occupies(int x, int y)
        {
            return map[x, y].valid;
        }

        /// <summary>
        /// Moves the snakes head in the given direction
        /// </summary>
        /// <param name="direc">The direction of required motion</param>
        private bool moveHead(Direction direc)
        {
            bool crash = false;
            switch (map[headX, headY].y)
            {
                case 0: //head is directed to the right
                    switch (direc)
                    {
                        case Direction.up:
                            map[headX, headY] = new PartID(1, 2);
                            headY = headY - 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 3);
                            break;
                        case Direction.down:
                            map[headX, headY] = new PartID(1, 5);
                            headY = headY + 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 1);
                            break;
                        default:
                            map[headX, headY] = new PartID(1, 0);
                            headX = headX + 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 0);
                            break;
                    }
                    break;
                case 1: //head is directed down
                    switch (direc)
                    {
                        case Direction.left:
                            map[headX, headY] = new PartID(1, 2);
                            headX = headX - 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 2);
                            break;
                        case Direction.right:
                            map[headX, headY] = new PartID(1, 3);
                            headX = headX + 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 0);
                            break;
                        default:
                            map[headX, headY] = new PartID(1, 1);
                            headY = headY + 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 1);
                            break;
                    }
                    break;
                case 2: //head is directed to the left
                    switch (direc)
                    {
                        case Direction.up:
                            map[headX, headY] = new PartID(1, 3);
                            headY = headY - 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 3);
                            break;
                        case Direction.down:
                            map[headX, headY] = new PartID(1, 4);
                            headY = headY + 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 1);
                            break;
                        default:
                            map[headX, headY] = new PartID(1, 0);
                            headX = headX - 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 2);
                            break;
                    }
                    break;
                case 3: //head is directed up
                    switch (direc)
                    {
                        case Direction.left:
                            map[headX, headY] = new PartID(1, 5);
                            headX = headX - 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 2);
                            break;
                        case Direction.right:
                            map[headX, headY] = new PartID(1, 4);
                            headX = headX + 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 0);
                            break;
                        default:
                            map[headX, headY] = new PartID(1, 1);
                            headY = headY - 1;
                            crash = map[headX, headY].valid;
                            map[headX, headY] = new PartID(0, 3);
                            break;
                    }
                    break;
            }
            return crash;
        }

        /// <summary>
        /// Moves tail
        /// </summary>
        private void moveTail()
        {
            int oldtailX = tailX;
            int oldtailY = tailY;
            switch (map[tailX, tailY].y)
            {
                case 0: //body is to the right from the tail
                    tailX = tailX + 1;
                    switch (map[tailX, tailY].y)
                    {
                        case 0:
                            map[tailX, tailY] = new PartID(2, 0);
                            break;
                        case 2:
                            map[tailX, tailY] = new PartID(2, 3);
                            break;
                        case 5:
                            map[tailX, tailY] = new PartID(2, 1);
                            break;
                    }
                    break;
                case 1: //body is below the tail
                    tailY = tailY + 1;
                    switch (map[tailX, tailY].y)
                    {
                        case 1:
                            map[tailX, tailY] = new PartID(2, 1);
                            break;
                        case 2:
                            map[tailX, tailY] = new PartID(2, 2);
                            break;
                        case 3:
                            map[tailX, tailY] = new PartID(2, 0);
                            break;
                    }
                    break;
                case 2: //body is to the left from the tail
                    tailX = tailX - 1;
                    switch (map[tailX, tailY].y)
                    {
                        case 0:
                            map[tailX, tailY] = new PartID(2, 2);
                            break;
                        case 3:
                            map[tailX, tailY] = new PartID(2, 3);
                            break;
                        case 4:
                            map[tailX, tailY] = new PartID(2, 1);
                            break;
                    }
                    break;
                case 3: //body is above the tail
                    tailY = tailY - 1;
                    switch (map[tailX, tailY].y)
                    {
                        case 1:
                            map[tailX, tailY] = new PartID(2, 3);
                            break;
                        case 4:
                            map[tailX, tailY] = new PartID(2, 0);
                            break;
                        case 5:
                            map[tailX, tailY] = new PartID(2, 2);
                            break;
                    }
                    break;

            }
            map[oldtailX, oldtailY] = new PartID();
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
        right,
        down,
        left,
        up
    }
}
