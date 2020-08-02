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
        protected Thing[,] mapf;
        protected Bitmap food = Properties.Resources.food;
        protected Bitmap wall = Properties.Resources.wall;
        protected Player playerA;
        protected bool gameOver;
        protected bool stretchA;
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
            mapf = new Thing[mapWidth, mapHeight];
            rnd = new Random(rndseed);
            food.MakeTransparent(Color.White);
            placeWalls();
            placeAllFood();
        }

        /// <summary>
        /// Places walls on the map
        /// </summary>
        private void placeWalls()
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
                    if (orientation == 0) for (int i = 1; i <= length; i++) mapf[(startX + i).posMod(mapWidth), startY] = Thing.wall;
                    else for (int i = 1; i <= length; i++) mapf[startX, (startY + i).posMod(mapHeight)] = Thing.wall;
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
            mapf = new Thing[mapWidth, mapHeight];
            placeWalls();
            placeAllFood();
        }

        /// <summary>
        /// Places all pieces of food on the map. Also suitable for multiplayer.
        /// </summary>
        private void placeAllFood()
        {
            int done = 0;
            while (done < foodCount)
            {
                int x = rnd.Next(mapWidth);
                int y = rnd.Next(mapHeight);
                if (!(x == 0 && y == 0) && !(x == 1 && y == 0) && !(x == 2 && y == 0) && !(x == mapWidth - 1 && y == mapHeight - 1) && !(x == mapWidth - 2 && y == mapHeight - 1) && !(x == mapWidth - 3 && y == mapHeight - 1) && mapf[x, y] == Thing.nothing) //make sure that the food is not placed on initial positions of snakes or another food or wall
                {
                    mapf[x, y] = Thing.food;
                    done++;
                }
            }
        }

        /// <summary>
        /// Adds one piece of food
        /// </summary>
        private void addFood()
        {
            bool go = true;
            while (go)
            {
                int x = rnd.Next(mapWidth);
                int y = rnd.Next(mapHeight);
                if (!playerA.Occupies(x, y) && mapf[x, y] == Thing.nothing)
                {
                    mapf[x, y] = Thing.food;
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
                if (mapf[playerA.headX, playerA.headY] == Thing.wall) gameOver = true;
                stretchA = mapf[playerA.headX, playerA.headY] == Thing.food;
                if (mapf[playerA.headX, playerA.headY] == Thing.food)
                {
                    addFood();
                    mapf[playerA.headX, playerA.headY] = Thing.nothing;
                }
            }
            return new Scores(playerA.length - 3, 0, gameOver);
        }

        /// <summary>
        /// Paints food and walls on the ground
        /// </summary>
        /// <param name="gr">Drawing surface</param>
        private void paintMap(Graphics gr)
        {
            food.SetResolution(gr.DpiX, gr.DpiY);
            wall.SetResolution(gr.DpiX, gr.DpiY);
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    if (mapf[i, j] == Thing.food) gr.DrawImage(food, i * blockEdge, j * blockEdge);
                    if (mapf[i, j] == Thing.wall) gr.DrawImage(wall, i * blockEdge, j * blockEdge);
                }
            }
        }

        /// <summary>
        /// Paints food, walls and snake on the ground
        /// </summary>
        /// <param name="gr">Drawing surface</param>
        public virtual void Paint(Graphics gr)
        {
            paintMap(gr);
            playerA.Paint(gr);
        }

        protected enum Thing
        {
            nothing,
            food,
            wall
        }
    }

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
