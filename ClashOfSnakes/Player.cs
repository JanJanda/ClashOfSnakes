using System;
using System.Drawing;

namespace ClashOfSnakes
{
    class Player
    {
        readonly Bitmap[] heads = new Bitmap[4];
        readonly Bitmap[] bodies = new Bitmap[6];
        readonly Bitmap[] tails = new Bitmap[4];
        readonly Bitmap[][] parts;
        readonly int mapWidth; //in blocks
        readonly int mapHeight; //in blocks
        readonly int blockEdge; //in pixels
        int hx;
        public int HeadX { get { return hx; } private set { hx = value.PosMod(mapWidth); } }
        int hy;
        public int HeadY { get { return hy; } private set { hy = value.PosMod(mapHeight); } }
        int tx;
        public int TailX { get { return tx; } private set { tx = value.PosMod(mapWidth); } }
        int ty;
        public int TailY { get { return ty; } private set { ty = value.PosMod(mapHeight); } }
        public int Length { get; private set; }

        readonly PartID[,] map;

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
                    HeadX = 2;
                    HeadY = 0;
                    TailX = 0;
                    TailY = 0;
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
                    HeadX = mapWidth - 3;
                    HeadY = mapHeight - 1;
                    TailX = mapWidth - 1;
                    TailY = mapHeight - 1;
                    map[mapWidth - 3, mapHeight - 1] = new PartID(0, 2);
                    map[mapWidth - 2, mapHeight - 1] = new PartID(1, 0);
                    map[mapWidth - 1, mapHeight - 1] = new PartID(2, 2);
                    break;
                default:
                    throw new ArgumentException();
            }
            Length = 3;

            parts = new Bitmap[3][];
            parts[0] = heads;
            parts[1] = bodies;
            parts[2] = tails;

            for (int i = 0; i < parts.Length; i++)
            {
                for (int j = 0; j < parts[i].Length; j++)
                {
                    parts[i][j].MakeTransparent(Color.White);
                }
            }
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
                        Bitmap pic = parts[map[i, j].x][map[i, j].y];
                        pic.SetResolution(gr.DpiX, gr.DpiY);
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
            if (stretch)
            {
                Length++;
                return MoveHead(direc);
            }
            else
            {
                MoveTail();
                return MoveHead(direc);
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
        private bool MoveHead(Direction direc)
        {
            bool crash = false;
            switch (map[HeadX, HeadY].y)
            {
                case 0: //head is directed to the right
                    switch (direc)
                    {
                        case Direction.up:
                            map[HeadX, HeadY] = new PartID(1, 2);
                            HeadY--;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 3);
                            break;
                        case Direction.down:
                            map[HeadX, HeadY] = new PartID(1, 5);
                            HeadY++;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 1);
                            break;
                        default:
                            map[HeadX, HeadY] = new PartID(1, 0);
                            HeadX++;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 0);
                            break;
                    }
                    break;
                case 1: //head is directed down
                    switch (direc)
                    {
                        case Direction.left:
                            map[HeadX, HeadY] = new PartID(1, 2);
                            HeadX--;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 2);
                            break;
                        case Direction.right:
                            map[HeadX, HeadY] = new PartID(1, 3);
                            HeadX++;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 0);
                            break;
                        default:
                            map[HeadX, HeadY] = new PartID(1, 1);
                            HeadY++;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 1);
                            break;
                    }
                    break;
                case 2: //head is directed to the left
                    switch (direc)
                    {
                        case Direction.up:
                            map[HeadX, HeadY] = new PartID(1, 3);
                            HeadY--;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 3);
                            break;
                        case Direction.down:
                            map[HeadX, HeadY] = new PartID(1, 4);
                            HeadY++;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 1);
                            break;
                        default:
                            map[HeadX, HeadY] = new PartID(1, 0);
                            HeadX--;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 2);
                            break;
                    }
                    break;
                case 3: //head is directed up
                    switch (direc)
                    {
                        case Direction.left:
                            map[HeadX, HeadY] = new PartID(1, 5);
                            HeadX--;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 2);
                            break;
                        case Direction.right:
                            map[HeadX, HeadY] = new PartID(1, 4);
                            HeadX++;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 0);
                            break;
                        default:
                            map[HeadX, HeadY] = new PartID(1, 1);
                            HeadY--;
                            crash = map[HeadX, HeadY].valid;
                            map[HeadX, HeadY] = new PartID(0, 3);
                            break;
                    }
                    break;
            }
            return crash;
        }

        /// <summary>
        /// Moves tail
        /// </summary>
        private void MoveTail()
        {
            int oldtailX = TailX;
            int oldtailY = TailY;
            switch (map[TailX, TailY].y)
            {
                case 0: //body is to the right from the tail
                    TailX++;
                    switch (map[TailX, TailY].y)
                    {
                        case 0:
                            map[TailX, TailY] = new PartID(2, 0);
                            break;
                        case 2:
                            map[TailX, TailY] = new PartID(2, 3);
                            break;
                        case 5:
                            map[TailX, TailY] = new PartID(2, 1);
                            break;
                    }
                    break;
                case 1: //body is below the tail
                    TailY++;
                    switch (map[TailX, TailY].y)
                    {
                        case 1:
                            map[TailX, TailY] = new PartID(2, 1);
                            break;
                        case 2:
                            map[TailX, TailY] = new PartID(2, 2);
                            break;
                        case 3:
                            map[TailX, TailY] = new PartID(2, 0);
                            break;
                    }
                    break;
                case 2: //body is to the left from the tail
                    TailX--;
                    switch (map[TailX, TailY].y)
                    {
                        case 0:
                            map[TailX, TailY] = new PartID(2, 2);
                            break;
                        case 3:
                            map[TailX, TailY] = new PartID(2, 3);
                            break;
                        case 4:
                            map[TailX, TailY] = new PartID(2, 1);
                            break;
                    }
                    break;
                case 3: //body is above the tail
                    TailY--;
                    switch (map[TailX, TailY].y)
                    {
                        case 1:
                            map[TailX, TailY] = new PartID(2, 3);
                            break;
                        case 4:
                            map[TailX, TailY] = new PartID(2, 0);
                            break;
                        case 5:
                            map[TailX, TailY] = new PartID(2, 2);
                            break;
                    }
                    break;

            }
            map[oldtailX, oldtailY] = new PartID();
        }

        /// <summary>
        /// Data type used for indicating what part of a snake is on the particular place of the map.
        /// </summary>
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

    /// <summary>
    /// There are currently two possibilities of snake color.
    /// </summary>
    enum SnakeColor
    {
        green,
        red
    }

    /// <summary>
    /// The direction of the desired motion of the snake on the map.
    /// </summary>
    enum Direction
    {
        right,
        down,
        left,
        up
    }
}
