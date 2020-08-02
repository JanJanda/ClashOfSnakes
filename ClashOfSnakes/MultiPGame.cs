using System.Drawing;

namespace ClashOfSnakes
{
    class MultiPGame : SinglePGame
    {
        Player playerB;
        bool stretchB;

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
                if (playerA.Occupies(playerB.headX, playerB.headY) || playerB.Occupies(playerA.headX, playerA.headY)) gameOver = true;
                if (mapf[playerA.headX, playerA.headY] == Thing.wall || mapf[playerB.headX, playerB.headY] == Thing.wall) gameOver = true;
                stretchA = mapf[playerA.headX, playerA.headY] == Thing.food;
                stretchB = mapf[playerB.headX, playerB.headY] == Thing.food;
                if (mapf[playerA.headX, playerA.headY] == Thing.food)
                {
                    addFood();
                    mapf[playerA.headX, playerA.headY] = Thing.nothing;
                }
                if (mapf[playerB.headX, playerB.headY] == Thing.food)
                {
                    addFood();
                    mapf[playerB.headX, playerB.headY] = Thing.nothing;
                }
            }

            return new Scores(playerA.length - 3, playerB.length - 3, gameOver);
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
                if (!playerA.Occupies(x, y) && !playerB.Occupies(x, y) && mapf[x, y] == Thing.nothing)
                {
                    mapf[x, y] = Thing.food;
                    go = false;
                }
            }
        }
    }
}
