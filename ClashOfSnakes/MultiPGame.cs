﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public MultiPGame(int width, int height, int edge) : base(width, height, edge)
        {
            playerB = new Player(SnakeColor.red, width, height, edge);
        }

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
                stretchA = mapf[playerA.headX, playerA.headY] == Food.food;
                stretchB = mapf[playerB.headX, playerB.headY] == Food.food;
                if (mapf[playerA.headX, playerA.headY] == Food.food) addFood();
                if (mapf[playerB.headX, playerB.headY] == Food.food) addFood();
                mapf[playerA.headX, playerA.headY] = Food.nothing;
                mapf[playerB.headX, playerB.headY] = Food.nothing;
            }

            return new Scores(playerA.length - 3, playerB.length - 3);
        }

        /// <summary>
        /// Adds one piece of food
        /// </summary>
        private void addFood()
        {
            bool go = true;
            Random rnd = new Random();
            while (go)
            {
                int x = rnd.Next(mapWidth);
                int y = rnd.Next(mapHeight);
                if (!playerA.Occupies(x, y) && !playerB.Occupies(x, y) && mapf[x, y] == Food.nothing)
                {
                    mapf[x, y] = Food.food;
                    go = false;
                }
            }
        }
    }
}