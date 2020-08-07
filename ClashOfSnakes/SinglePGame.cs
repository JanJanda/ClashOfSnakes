using System;
using System.Drawing;

namespace ClashOfSnakes
{
    class SinglePGame
    {
        protected readonly int mapWidth; //in blocks
        protected readonly int mapHeight; //in blocks
        protected readonly int blockEdge; //in pixels
        protected const int wallCount = 4;
        protected const int foodCount = 20;
        protected ThingOnMap[,] map;
        protected Bitmap food = Properties.Resources.food;
        protected Bitmap wall = Properties.Resources.wall;
        protected Player playerA;
        protected bool gameOver; //tells if the game is over
        protected bool stretchA; //tells if playerAs snake shall be stretched in this move
        protected readonly Random rnd;

        /// <summary>
        /// Creates new single player game
        /// </summary>
        /// <param name="width">Width of the map, PIXELS</param>
        /// <param name="height">Height of the map, PIXELS</param>
        /// <param name="edge">Length of the edge of a block, PIXELS, must divide width and height</param>
        public SinglePGame(int width, int height, int edge, int rndseed)
        {
            if (width % edge != 0 || height % edge != 0) throw new ArgumentException();
            mapWidth = width / edge;
            mapHeight = height / edge;
            blockEdge = edge;
            playerA = new Player(SnakeColor.green, mapWidth, mapHeight, blockEdge);
            map = new ThingOnMap[mapWidth, mapHeight];
            rnd = new Random(rndseed);
            food.MakeTransparent(Color.White);
            PlaceWalls();
            PlaceAllFood();
        }

        /// <summary>
        /// Places walls on the map
        /// </summary>
        private void PlaceWalls()
        {
            int done = 0;
            while (done < wallCount)
            {
                int length = rnd.Next(5, mapHeight / 2);
                int orientation = rnd.Next(2);
                int startX = rnd.Next(mapWidth);
                int startY = rnd.Next(mapHeight);
                if (startX >= 4 && startX <= mapWidth - 5 && startY >= 2 && startY <= mapHeight - 3) //makes sure that wall is not on initial position of any snake
                {
                    if (orientation == 0) for (int i = 1; i <= length; i++) map[(startX + i).PosMod(mapWidth), startY] = ThingOnMap.wall;
                    else for (int i = 1; i <= length; i++) map[startX, (startY + i).PosMod(mapHeight)] = ThingOnMap.wall;
                    done++;
                }
            }
        }

        /// <summary>
        /// Resets the game.
        /// </summary>
        public virtual void Reset()
        {
            gameOver = false;
            stretchA = false;
            playerA = new Player(SnakeColor.green, mapWidth, mapHeight, blockEdge);
            map = new ThingOnMap[mapWidth, mapHeight];
            PlaceWalls();
            PlaceAllFood();
        }

        /// <summary>
        /// Places all pieces of food on the map. Also suitable for multiplayer.
        /// </summary>
        private void PlaceAllFood()
        {
            int done = 0;
            while (done < foodCount)
            {
                int x = rnd.Next(mapWidth);
                int y = rnd.Next(mapHeight);
                if (!(x == 0 && y == 0) && !(x == 1 && y == 0) && !(x == 2 && y == 0) && !(x == mapWidth - 1 && y == mapHeight - 1) && !(x == mapWidth - 2 && y == mapHeight - 1) && !(x == mapWidth - 3 && y == mapHeight - 1) && map[x, y] == ThingOnMap.nothing) //make sure that the food is not placed on initial positions of snakes or another food or wall
                {
                    map[x, y] = ThingOnMap.food;
                    done++;
                }
            }
        }

        /// <summary>
        /// Adds one piece of food
        /// </summary>
        private void AddFood()
        {
            bool go = true;
            while (go)
            {
                int x = rnd.Next(mapWidth);
                int y = rnd.Next(mapHeight);
                if (!playerA.Occupies(x, y) && map[x, y] == ThingOnMap.nothing)
                {
                    map[x, y] = ThingOnMap.food;
                    go = false;
                }
            }
        }

        /// <summary>
        /// Makes one game move in the specified direction
        /// </summary>
        /// <param name="direc">The specified direction</param>
        /// <returns></returns>
        public virtual Scores MakeMove(Direction direc, Direction none)
        {
            if (!gameOver)
            {
                gameOver = playerA.Move(direc, stretchA);
                if (map[playerA.HeadX, playerA.HeadY] == ThingOnMap.wall) gameOver = true;
                stretchA = map[playerA.HeadX, playerA.HeadY] == ThingOnMap.food;
                if (map[playerA.HeadX, playerA.HeadY] == ThingOnMap.food)
                {
                    AddFood();
                    map[playerA.HeadX, playerA.HeadY] = ThingOnMap.nothing;
                }
            }
            return new Scores(playerA.Length - 3, 0, gameOver);
        }

        /// <summary>
        /// Paints food and walls on the ground
        /// </summary>
        /// <param name="gr">Drawing surface</param>
        private void PaintMap(Graphics gr)
        {
            food.SetResolution(gr.DpiX, gr.DpiY);
            wall.SetResolution(gr.DpiX, gr.DpiY);
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    if (map[i, j] == ThingOnMap.food) gr.DrawImage(food, i * blockEdge, j * blockEdge);
                    if (map[i, j] == ThingOnMap.wall) gr.DrawImage(wall, i * blockEdge, j * blockEdge);
                }
            }
        }

        /// <summary>
        /// Paints food, walls and snake on the ground
        /// </summary>
        /// <param name="gr">Drawing surface</param>
        public virtual void Paint(Graphics gr)
        {
            PaintMap(gr);
            playerA.Paint(gr);
        }

        /// <summary>
        /// Tells what kind of an object or entity is on a particular place of the map.
        /// </summary>
        protected enum ThingOnMap
        {
            nothing,
            food,
            wall
        }
    }

    /// <summary>
    /// Data type used for returning values about the game back to the GameWindow
    /// </summary>
    struct Scores
    {
        public readonly int A;
        public readonly int B;
        public readonly bool gameOver;
        public Scores(int a, int b, bool gameo)
        {
            A = a;
            B = b;
            gameOver = gameo;
        }
    }
}
