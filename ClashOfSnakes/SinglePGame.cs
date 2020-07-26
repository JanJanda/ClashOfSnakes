using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashOfSnakes
{
    class SinglePGame
    {
        readonly int mapWidth; //in blocks
        readonly int mapHeight; //in blocks
        readonly int blockEdge; //in pixels
        const int foodCount = 20;
        readonly Food[,] mapf;
        Image food = Properties.Resources.food;
        Player playerA;

        /// <summary>
        /// Creates new single player game
        /// </summary>
        /// <param name="width">Width of the map, PIXELS</param>
        /// <param name="height">Height of the map, PIXELS</param>
        /// <param name="edge">Length of the edge of a block, PIXELS, must divide width and height</param>
        public SinglePGame(int width, int height, int edge)
        {
            if (width % edge != 0 || height % edge != 0) throw new ArgumentException();
            playerA = new Player(SnakeColor.green, width, height, edge);
            mapWidth = width / edge;
            mapHeight = height / edge;
            blockEdge = edge;
            mapf = new Food[mapWidth, mapHeight];
            placeAllFood();
        }

        /// <summary>
        /// Places all pieces of foon on the map. Also suitable for multiplayer.
        /// </summary>
        private void placeAllFood()
        {
            int done = 0;
            Random rnd = new Random();
            while(done < foodCount)
            {
                int x = rnd.Next(mapWidth);
                int y = rnd.Next(mapHeight);
                if (!(x == 0 && y == 0) && !(x == 1 && y == 0) && !(x == mapWidth - 1 && y == mapHeight - 1) && !(x == mapWidth - 2 && y == mapHeight - 1) && mapf[x, y] == Food.nothing) //make sure that the food is not placed on initial positions of snakes or another food
                {
                    mapf[x, y] = Food.food;
                    done++;
                }
            }
        }

        /// <summary>
        /// Paints food on the ground
        /// </summary>
        /// <param name="gr">Drawing surface</param>
        private void paintFood(Graphics gr)
        {
            Bitmap pic = (Bitmap)food;
            pic.SetResolution(gr.DpiX, gr.DpiY);
            pic.MakeTransparent(Color.White);
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    if (mapf[i, j] == Food.food) gr.DrawImage(pic, i * blockEdge, j * blockEdge);
                }
            }
        }

        /// <summary>
        /// Paints food and snake on the ground
        /// </summary>
        /// <param name="gr">Drawing surface</param>
        public void Paint(Graphics gr)
        {
            paintFood(gr);
            playerA.Paint(gr);
        }

        enum Food
        {
            nothing,
            food
        }
    }
}
