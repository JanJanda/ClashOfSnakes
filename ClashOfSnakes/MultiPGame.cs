using System.Drawing;

namespace ClashOfSnakes
{
    class MultiPGame : SinglePGame
    {
        Player playerB;
        bool stretchB; //Tells if the playerBs snake shall be stretched in the current move.

        /// <summary>
        /// Creates new multi player game
        /// </summary>
        /// <param name="width">Width of the map, PIXELS</param>
        /// <param name="height">Height of the map, PIXELS</param>
        /// <param name="edge">Length of the edge of a block, PIXELS, must divide width and height</param>
        public MultiPGame(int width, int height, int edge, int rndseed) : base(width, height, edge, rndseed)
        {
            playerB = new Player(SnakeColor.red, mapWidth, mapHeight, blockEdge);
        }

        /// <summary>
        /// Resets the game.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            stretchB = false;
            playerB = new Player(SnakeColor.red, mapWidth, mapHeight, blockEdge);
        }

        /// <summary>
        /// Paints food, walls and snakes on the ground
        /// </summary>
        /// <param name="gr">Drawing surface</param>
        public override void Paint(Graphics gr)
        {
            base.Paint(gr);
            playerB.Paint(gr);
        }

        /// <summary>
        /// Makes one game move with two snakes in specified directions
        /// </summary>
        /// <param name="direcA">Direction for snake A</param>
        /// <param name="direcB">Direction for snake B</param>
        /// <returns></returns>
        public override Scores MakeMove(Direction direcA, Direction direcB)
        {
            if (!gameOver)
            {
                gameOver = playerA.Move(direcA, stretchA) || playerB.Move(direcB, stretchB);
                if (playerA.Occupies(playerB.HeadX, playerB.HeadY) || playerB.Occupies(playerA.HeadX, playerA.HeadY)) gameOver = true;
                if (map[playerA.HeadX, playerA.HeadY] == ThingOnMap.wall || map[playerB.HeadX, playerB.HeadY] == ThingOnMap.wall) gameOver = true;
                stretchA = map[playerA.HeadX, playerA.HeadY] == ThingOnMap.food;
                stretchB = map[playerB.HeadX, playerB.HeadY] == ThingOnMap.food;
                if (map[playerA.HeadX, playerA.HeadY] == ThingOnMap.food)
                {
                    AddFood();
                    map[playerA.HeadX, playerA.HeadY] = ThingOnMap.nothing;
                }
                if (map[playerB.HeadX, playerB.HeadY] == ThingOnMap.food)
                {
                    AddFood();
                    map[playerB.HeadX, playerB.HeadY] = ThingOnMap.nothing;
                }
            }

            return new Scores(playerA.Length - 3, playerB.Length - 3, gameOver);
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
                if (!playerA.Occupies(x, y) && !playerB.Occupies(x, y) && map[x, y] == ThingOnMap.nothing)
                {
                    map[x, y] = ThingOnMap.food;
                    go = false;
                }
            }
        }
    }
}
