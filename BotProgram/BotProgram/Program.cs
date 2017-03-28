using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BotProgram.Models;
using System.Diagnostics;
namespace BotProgram
{
    public class Program
    {
        static int boardXLength { get; set; }
        static int boardYLength { get; set; }
        static List<Key> BotInvent = new List<Key>();
        static int bombCount { get; set; }
        static int playerLives { get; set; }
        static int goalY { get; set; }
        static int goalX { get; set; }
        static string elapsedTime { get; set; }
        static int wallPosY { get; set; }
        static int wallGapPosX { get; set; }
        
        static int currentLevel { get; set; }

        static void DrawBoard(char[,] board, Level l)
        {
            Console.Clear();
            for (int i = 0; i < l.BoardHeight; i++)
            {
                Console.Write("\n");
                for (int j = 0; j < l.BoardWidth; j++)
                {
                    switch(board[i, j])
                    {
                        case 'E':
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;
                        case 'G':
                            if(CheckPlayerInvent(i, j,l))
                            {
                                board[i, j] = '-';
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                            }
                            else
                                Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                        case 'K':
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case 'W':
                        case 'w':
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            break;
                        case '#':
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            break;
                        case 'B':
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case 'X':
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            break;


                    }
                    Console.CursorVisible = false;
                    Console.Write(board[i, j] + " ");
                }
                Console.ForegroundColor = ConsoleColor.White;
                if( i == 1 )
                    Console.Write("\t\tYou currently have " + BotInvent.Count.ToString() + " key(s).");
                else if (i == 2)
                {
                    if (l.Keys.Count - BotInvent.Count > 1)
                        Console.Write("\t\tThere are still " + ((l.Keys.Count - BotInvent.Count).ToString()) + " keys to find.");
                    else if (l.Keys.Count - BotInvent.Count == 1)
                        Console.Write("\t\tThere is still " + ((l.Keys.Count - BotInvent.Count).ToString()) + " key to find.");
                    else
                        Console.Write("\t\tGreat! There are no more keys to find.");
                }

                else if (i == 3)
                {
                        Console.Write("\t\tBot lives left: "+ playerLives);
                }
                else if (i == 4)
                {
                    Console.Write("\t\tBomb's left: " + bombCount);
                }
                else if (i == 5)
                {
                    Console.Write("\t\tTime Elapsed: " + elapsedTime);
                }
            }
            
        }

        private static string GetBotPosition(char[,] board, Level l)
        {
            string pos = "";
            for (int i = 0; i < l.BoardHeight; i++)
            {
                for (int j = 0; j < l.BoardWidth; j++)
                {
                    if (board[i, j] == 'B')
                    {
                        pos += i.ToString() + "," + j.ToString();
                        board[i, j] = '-';
                    }

                }
            }

            return pos;
        }
        private static string GetBotPositionNoChange(char[,] board, Level l)
        {
            string pos = "";
            for (int i = 0; i < l.BoardHeight; i++)
            {
                for (int j = 0; j < l.BoardWidth; j++)
                {
                    if (board[i, j] == 'B')
                    {
                        pos += i.ToString() + "," + j.ToString();
                    }

                }
            }

            return pos;
        }

        private static int GetMaxMoves(int bPosY, int bPosX, string command, char[,] board)
        {
            int maxMoves = 0;
            switch (command)
            {
                case "up":
                    for (int i = bPosY - 1; i > 0; i--)
                    {
                        if (board[i, bPosX] == '-' || board[i, bPosX] == 'E')
                            maxMoves += 1;
                        else
                            break;
                    }
                    break;
                case "down":
                    for (int i = bPosY + 1; i < boardYLength; i++)
                    {
                        if (board[i, bPosX] == '-' || board[i, bPosX] == 'E')
                            maxMoves += 1;
                        else
                            break;
                    }
                    break;
                case "left":
                    for (int i = bPosX - 1; i > 0; i--)
                    {
                        if (board[bPosY, i] == '-' || board[bPosY, i] == 'E')
                            maxMoves += 1;
                        else
                            break;
                    }
                    break;
                case "right":
                    for (int i = bPosX + 1; i < boardXLength; i++)
                    {
                        if (board[bPosY, i] == '-' || board[bPosY, i] == 'E')
                            maxMoves += 1;
                        else
                            break;
                    }
                    break;


            }
            return maxMoves;
        }

        private static bool CheckPlayerInvent(int posY, int posX, Level curreLevel)
        {
            int gId = 0;
            bool hasKey = false;
            foreach (var g in curreLevel.Gates)
            {
                if(posX == g.XPos)
                {
                    if(posY == g.YPos)
                    {
                        gId = g.GateID;
                    }
                }
            }
            foreach (var k in BotInvent)
            {
                if (k.GateID == gId)
                {
                    hasKey = true;
                    break;
                }
            }
            return hasKey;
        }

        private static void AddKeyToInvent(int posY, int posX, Level curreLevel)
        {
            foreach (var k in curreLevel.Keys)
            {
                if (posX == k.XPos)
                {
                    if (posY == k.Ypos)
                    {
                        BotInvent.Add(k);
                    }
                }
            }
        }

        private static char[,] CheckMoveTo(int bPosY, int bPosX, string command, char[,] board, Level curreLevel)
        {
            switch(command)
            {
                case "up":
                    if (board[bPosY - 1, bPosX] == '#' || board[bPosY - 1, bPosX] == 'W' )
                    {
                        board[bPosY, bPosX] = 'B';
                        break;
                    }
                    else if (board[bPosY - 1, bPosX] == 'w')
                    {
                        board[bPosY, bPosX] = 'B';
                        break;
                    }
                    else if (board[bPosY - 1, bPosX] == 'G')
                    {
                        if (CheckPlayerInvent(bPosY - 1, bPosX, curreLevel))
                        {
                            board[bPosY, bPosX] = '-';
                            board[bPosY - 1, bPosX] = 'B';
                        }
                        else
                        {
                            board[bPosY, bPosX] = 'B';
                            break;
                        }
                    }
                    else if (board[bPosY - 1, bPosX] == 'K')
                    {
                        AddKeyToInvent(bPosY - 1, bPosX, curreLevel);
                        board[bPosY, bPosX] = '-';
                        board[bPosY - 1, bPosX] = 'B';
                    }
                    else if (board[bPosY - 1, bPosX] == 'E')
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY - 1, bPosX] = 'B';
                    }
                    else if (board[bPosY - 1, bPosX] == 'X')
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY - 1, bPosX] = 'B';
                        bombCount += 1;
                    }
                    else
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY - 1, bPosX] = 'B';
                    }
                    break;
                case "down":
                    if (board[bPosY + 1, bPosX] == '#' || board[bPosY + 1, bPosX] == 'W')
                    {
                        board[bPosY, bPosX] = 'B';
                        break;
                    }
                    else if (board[bPosY + 1, bPosX] == 'w')
                    {
                        board[bPosY, bPosX] = 'B';
                        break;
                    }
                    else if (board[bPosY + 1, bPosX] == 'G')
                    {
                        if (CheckPlayerInvent(bPosY + 1, bPosX, curreLevel))
                        {
                            board[bPosY, bPosX] = '-';
                            board[bPosY + 1, bPosX] = 'B';
                        }
                        else
                        {
                            board[bPosY, bPosX] = 'B';
                            break;
                        }
                    }
                    else if (board[bPosY + 1, bPosX] == 'K')
                    {
                        AddKeyToInvent(bPosY + 1, bPosX, curreLevel);
                        board[bPosY, bPosX] = '-';
                        board[bPosY + 1, bPosX] = 'B';
                    }
                    else if (board[bPosY + 1, bPosX] == 'E')
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY + 1, bPosX] = 'B';
                    }
                    else if (board[bPosY + 1, bPosX] == 'X')
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY + 1, bPosX] = 'B';
                        bombCount += 1;
                    }
                    else
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY + 1, bPosX] = 'B';
                    }
                    break;
                case "left":
                    if (board[bPosY, bPosX - 1] == '#' || board[bPosY, bPosX - 1] == 'W')
                    {
                        board[bPosY, bPosX] = 'B';
                        break;
                    }
                    else if (board[bPosY, bPosX - 1] == 'w')
                    {
                        board[bPosY, bPosX] = 'B';
                        break;
                    }
                    else if (board[bPosY, bPosX - 1] == 'G')
                    {
                        if (CheckPlayerInvent(bPosY, bPosX - 1, curreLevel))
                        {
                            board[bPosY, bPosX] = '-';
                            board[bPosY, bPosX - 1] = 'B';
                        }
                        else
                        {
                            board[bPosY, bPosX] = 'B';
                            break;
                        }
                    }
                    else if (board[bPosY, bPosX - 1] == 'K')
                    {
                        AddKeyToInvent(bPosY, bPosX - 1, curreLevel);
                        board[bPosY, bPosX] = '-';
                        board[bPosY, bPosX - 1] = 'B';
                    }
                    else if (board[bPosY, bPosX - 1] == 'E')
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY, bPosX - 1] = 'B';
                    }
                    else if (board[bPosY, bPosX - 1] == 'X')
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY, bPosX - 1] = 'B';
                        bombCount += 1;
                    }
                    else
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY, bPosX - 1] = 'B';
                    }
                    break;
                case "right":
                    if (board[bPosY, bPosX + 1] == '#' || board[bPosY, bPosX + 1] == 'W')
                    {
                        board[bPosY, bPosX] = 'B';
                        break;
                    }
                    else if (board[bPosY, bPosX + 1] == 'w')
                    {
                        board[bPosY, bPosX] = 'B';
                        break;
                    }
                    else if (board[bPosY, bPosX + 1] == 'G')
                    {
                        if (CheckPlayerInvent(bPosY, bPosX + 1, curreLevel))
                        {
                            board[bPosY, bPosX] = '-';
                            board[bPosY, bPosX + 1] = 'B';
                        }
                        else
                        {
                            board[bPosY, bPosX] = 'B';
                            break;
                        }
                    }
                    else if (board[bPosY, bPosX + 1] == 'K')
                    {
                        AddKeyToInvent(bPosY, bPosX + 1, curreLevel);
                        board[bPosY, bPosX] = '-';
                        board[bPosY, bPosX + 1] = 'B';
                    }
                    else if (board[bPosY, bPosX + 1] == 'E')
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY, bPosX + 1] = 'B';
                    }
                    else if (board[bPosY, bPosX + 1] == 'X')
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY, bPosX + 1] = 'B';
                        bombCount += 1;
                    }
                    else
                    {
                        board[bPosY, bPosX] = '-';
                        board[bPosY, bPosX + 1] = 'B';
                    }
                    break;
            }
            return board;
        }

        private static char[,] MoveBot(string command, int moveCount, char[,] board, Level l)
        {
            string botPos = GetBotPosition(board,l);
            var pos = botPos.Split(',');
            switch (command)
            {
                case "up":
                    //if (Convert.ToInt32(pos[0]) != 1)
                    //{
                    //    int maxMoves = GetMaxMoves(Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]), command, board);
                    //    if (maxMoves > moveCount)
                    //        pos[0] = (Convert.ToInt32(pos[0]) - moveCount).ToString();
                    //    else
                    //        pos[0] = (Convert.ToInt32(pos[0]) - maxMoves).ToString();
                    //}
                  board =  CheckMoveTo(Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]), "up", board, l);
                    break;
                case "down":
                    //if (Convert.ToInt32(pos[0]) != boardYLength -2)
                    //{
                    //    int maxMoves = GetMaxMoves(Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]), command, board);
                    //    if (maxMoves > moveCount)
                    //        pos[0] = (Convert.ToInt32(pos[0]) + moveCount).ToString();
                    //    else
                    //        pos[0] = (Convert.ToInt32(pos[0]) + maxMoves).ToString();
                    //}
                    board = CheckMoveTo(Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]), "down", board, l);
                    break;
                case "left":
                    //if (Convert.ToInt32(pos[1]) != 1)
                    //{
                    //    int maxMoves = GetMaxMoves(Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]), command, board);
                    //    if (maxMoves > moveCount)
                    //        pos[1] = (Convert.ToInt32(pos[1]) - moveCount).ToString();
                    //    else
                    //        pos[1] = (Convert.ToInt32(pos[1]) - maxMoves).ToString();
                    //}
                    board = CheckMoveTo(Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]), "left", board, l);
                    break;
                case "right":
                    //if (Convert.ToInt32(pos[1]) != boardXLength - 2)
                    //{
                    //    int maxMoves = GetMaxMoves(Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]), command, board);
                    //    if (maxMoves > moveCount)
                    //        pos[1] = (Convert.ToInt32(pos[1]) + moveCount).ToString();
                    //    else
                    //        pos[1] = (Convert.ToInt32(pos[1]) + maxMoves).ToString();
                    //}
                    board = CheckMoveTo(Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]), "right", board, l);
                    break;


            }

           
            return board;
        }

        private static void MovePlayerOneSpace(string dir, char[,] board, Level l)
        {
            switch(dir)
            {
                case "left":
                    MoveBot("left", 1, board, l);
                    break;
                case "right":
                    MoveBot("right", 1, board, l);
                    break;
                case "up":
                    MoveBot("up", 1, board, l);
                    break;
                case "down":
                    MoveBot("down", 1, board, l);
                    break;
                default:
                    break;
            }
        }

        private static void DrawBomb(string lastDir, char[,] board, Level level)
        {
            string botPos = GetBotPositionNoChange(board, level);
            string[] pos = botPos.Split(',');
            switch(lastDir)
            {
                case "up":
                    if(board[Convert.ToInt32(pos[0]) + 1 ,Convert.ToInt32(pos[1])] == '-')
                    {
                        board[Convert.ToInt32(pos[0]) + 1, Convert.ToInt32(pos[1])] = 'B';
                        board[Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1])] = 'X';
                    }
                    break;
                case "down":
                    if (board[Convert.ToInt32(pos[0]) - 1, Convert.ToInt32(pos[1])] == '-')
                    {
                        board[Convert.ToInt32(pos[0]) - 1, Convert.ToInt32(pos[1])] = 'B';
                        board[Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1])] = 'X';
                    }
                    break;
                case "right":
                    if (board[Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]) - 1] == '-')
                    {
                        board[Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]) - 1] = 'B';
                        board[Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1])] = 'X';
                    }
                    break;
                case "left":
                    if (board[Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]) + 1] == '-')
                    {
                        board[Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]) + 1] = 'B';
                        board[Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1])] = 'X';
                    }
                    break;

            }
        }

        private static void DetBomb(char[,] board, Level l)
        {
             for (int i = 0; i < l.BoardHeight; i++)
             {
                 for (int j = 0; j < l.BoardWidth; j++)
                 {
                     if(board[i,j] == 'X')
                     {
                         //top left
                         if (board[i - 1, j - 1] == 'w')
                             board[i - 1, j - 1] = '-';
                         else if (board[i - 1, j - 1] == 'B')
                             {
                                 board[i - 1, j - 1] = '-';
                                 board[1, 1] = 'B';
                                 playerLives -= 1;
                             }
                         //top 
                         if (board[i - 1, j] == 'w')
                             board[i - 1, j] = '-';
                         else if (board[i - 1, j] == 'B')
                         {
                             board[i - 1, j] = '-';
                             board[1, 1] = 'B';
                             playerLives -= 1;
                         }
                         //top right
                         if (board[i - 1, j + 1] == 'w')
                             board[i - 1, j + 1] = '-';
                         else if (board[i - 1, j + 1] == 'B')
                         {
                             board[i - 1, j + 1] = '-';
                             board[1, 1] = 'B';
                             playerLives -= 1;
                         }
                         // left
                         if (board[i, j - 1] == 'w')
                             board[i, j - 1] = '-';
                         else if (board[i, j - 1] == 'B')
                         {
                             board[i, j - 1] = '-';
                             board[1, 1] = 'B';
                             playerLives -= 1;
                         }
                         //center
                         board[i, j] = '-';
                         bombCount += 1;
                         //right
                         if (board[i, j + 1] == 'w')
                             board[i, j + 1] = '-';
                         else if (board[i, j + 1] == 'B')
                         {
                             board[i, j + 1] = '-';
                             board[1, 1] = 'B';
                             playerLives -= 1;
                         }
                         //bottom left
                         if (board[i + 1, j - 1] == 'w')
                             board[i + 1, j - 1] = '-';
                         else if (board[i + 1, j - 1] == 'B')
                         {
                             board[i + 1, j - 1] = '-';
                             board[1, 1] = 'B';
                             playerLives -= 1;
                         }
                         //bottom 
                         if (board[i + 1, j] == 'w')
                             board[i + 1, j] = '-';
                         else if (board[i + 1, j] == 'B')
                         {
                             board[i + 1, j] = '-';
                             board[1, 1] = 'B';
                             playerLives -= 1;
                         }
                         //bottom right
                         if (board[i + 1, j + 1] == 'w')
                             board[i + 1, j + 1] = '-';
                         else if (board[i + 1, j] == 'B')
                         {
                             board[i + 1, j + 1] = '-';
                             board[1, 1] = 'B';
                             playerLives -= 1;
                         }
                     }
                 }
             }
        }

        private static bool Playing(bool playing, char[,] board, Level level)
        {
            string command = "";
            bool waiting = true;
            string moveCountString = "0";
            int moveCount = 0;
            bool waitingToMove = true;
            string lastDirection = "right";
            Console.Write("\nStart by typing 'move' again: ");
            do
            {
                command = Console.ReadLine();
                command = command.ToLower();
                if (command != "move")
                    Console.Write("\n\nPlease, type in 'move' to begin with...");
                else
                {
                    Console.Write("\nGreat, now we need a direction.");
                }
            } while (command != "move");

            Console.Write("\n Please give the bot a direction to move in.\n Type 'up', 'right', 'down', 'left' for the direction you want the bot to move in.");

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while (waiting)
            {
                TimeSpan ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                ConsoleKeyInfo keypress;
                keypress = Console.ReadKey(); // read keystrokes 
               
                if (keypress.Key == ConsoleKey.LeftArrow)
                {
                    MovePlayerOneSpace("left",board,level);
                    lastDirection = "left";
                }
                else if (keypress.Key == ConsoleKey.RightArrow)
                {
                    MovePlayerOneSpace("right", board, level);
                    lastDirection = "right";
                }
                else if (keypress.Key == ConsoleKey.UpArrow)
                {
                    MovePlayerOneSpace("up", board, level);
                    lastDirection = "up";
                }
                else if (keypress.Key == ConsoleKey.DownArrow)
                {
                    MovePlayerOneSpace("down", board, level);
                    lastDirection = "down";
                }
                else if (keypress.Key == ConsoleKey.B)
                {
                    if (bombCount >= 1)
                    {
                        DrawBomb(lastDirection, board, level);
                        bombCount -= 1;
                    }
                    else
                    {
                        Console.WriteLine("NO BOMBS LEFT!");
                    }
                }
                else if (keypress.Key == ConsoleKey.D)
                {
                    DetBomb(board, level);
                }
                else if (keypress.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
                Console.Write("\n\n\n\n Here's the updated board...");
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                DrawBoard(board, level);
                string botPos = GetBotPositionNoChange(board, level);
                string[] pos = botPos.Split(',');
                Console.SetCursorPosition(Convert.ToInt32(pos[0]), Convert.ToInt32(pos[1]));

                if (Convert.ToInt32(pos[0]) == level.Exit.YPos)
                {
                    if (Convert.ToInt32(pos[1]) == level.Exit.XPos)
                    {
                        Console.Write("\n\n\n\t GOOD JOB YOU REACHED THE END");
                        waiting = false;
                        playing = false;
                        currentLevel += 1;
                        BotInvent = new List<Key>();
                    }
                }

            }
            stopWatch.Stop();

            
            //while (waitingToMove)
            //{
            //    int moveCountMax = 10;
            //    if (command == "right" || command == "left")
            //    {
            //        moveCountMax = boardXLength - 2;
            //        Console.Write("\n Please give the bot a distance to move between 1 and " + moveCountMax.ToString() + " blocks: ");
                    
            //    }
            //    else
            //    {
            //        moveCountMax = boardYLength - 2;
            //        Console.Write("\n Please give the bot a distance to move between 1 and " + moveCountMax.ToString() + " blocks: ");
            //    }

                
            //    moveCountString = Console.ReadLine();
            //    moveCount = Convert.ToInt32(moveCountString);
            //    if (moveCount > 0)
            //    {
            //        if (moveCount < moveCountMax)
            //        {
            //            Console.Write("\n Great, we will move the bot " + moveCount + " paces '" + command + "'.");
            //            waitingToMove = false;
            //        }
            //    }
            //}

            //board = MoveBot(command, moveCount, board);
            //Console.Write("\n\n\n\n Here's the updated board...");
            //DrawBoard(board);

            //botPos = GetBotPositionNoChange(board);
            //pos = botPos.Split(',');

            //if (Convert.ToInt32(pos[0]) == goalY)
            //{
            //    if (Convert.ToInt32(pos[1]) == goalX)
            //    {
            //        Console.Write("\n\n\n\t GOOD JOB YOU REACHED THE END");
            //        playing = false;
            //    }
            //}
            return playing;
        }


        
        private static void GenBoardSizes()
        {
             Random r = new Random();
            boardXLength = r.Next(10, 100);
            boardYLength = r.Next(10, 100);
            wallPosY = r.Next(2, boardYLength - 4);
            wallGapPosX = r.Next(0, boardXLength - 2);
            goalX = r.Next(2, boardXLength - 2);
            goalY = r.Next(wallPosY + 2, boardYLength - 2);


        }
        private static char[,] LoadLevelToBoard(Level l)
        {
            char[,] board = new char[l.BoardHeight, l.BoardWidth];

            //ADD IN THE OUTER AREA
            for (int i = 0; i < l.BoardHeight; i++)
            {
                for (int j = 0; j < l.BoardWidth; j++)
                {

                    if (i == 0 || j == 0 || i == l.BoardHeight - 1 || j == l.BoardWidth - 1)
                    {
                        board[i, j] = '#';
                    }
                    else
                    {
                        board[i, j] = '-';
                    }
                }

            }
            for (int i = 0; i < l.BoardHeight; i++)
            {
                for (int j = 0; j < l.BoardWidth; j++)
                {
                    if (i == l.Exit.YPos)
                    {
                        if (j == l.Exit.XPos)
                        {
                            board[i, j] = 'E'; // add in the exit
                        }
                    }
                }
            }
            //ADD IN THE WALLS
            foreach (var W in l.Walls)
            {
                for (int i = 0; i < l.BoardHeight; i++)
                {
                    for (int j = 0; j < l.BoardWidth; j++)
                    {
                        if(W.Horizontal)
                        {
                            if(i == W.YPos)
                            {
                                int length = 0;
                                if (W.XLength == 0)
                                    length = l.BoardWidth - 2;
                                else
                                    length = W.XPos + W.XLength;

                                    if (j >= W.XPos && j <= length)
                                    {
                                        if (W.Breakable == true)
                                            board[i, j] = 'w';
                                        else 
                                            board[i, j] = 'W';
                                    }
                            }
                        }
                        else
                        {
                            if (i >= 1)
                            {
                                int length = 0;
                                if (W.YLength == 0)
                                    length = l.BoardHeight - 2;
                                else
                                    length = W.YPos + W.YLength;

                                if (i >= W.YPos && i <=length)
                                {
                                    if (i <= l.BoardHeight - 2)
                                    {
                                        if (j == W.XPos)
                                        {
                                            if (W.Breakable == true)
                                                board[i, j] = 'w';
                                            else
                                                board[i, j] = 'W';
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            //ADD IN THE KEYS
            foreach (var k in l.Keys)
            {
                for (int i = 0; i < l.BoardHeight; i++)
                {
                    for (int j = 0; j < l.BoardWidth; j++)
                    {
                        if(i == k.Ypos)
                        {
                            if(j==k.XPos)
                            {
                                board[i, j] = 'K';
                            }
                        }
                    }
                }
            }

            //ADD IN THE GATES
            foreach (var g in l.Gates)
            {
                for (int i = 0; i < l.BoardHeight; i++)
                {
                    for (int j = 0; j < l.BoardWidth; j++)
                    {
                        if (i == g.YPos)
                        {
                            if (j == g.XPos)
                            {
                                board[i, j] = 'G';
                            }
                        }
                    }
                }
            }
            return board;
        }

        private static char[,] GenBoard(char[,] board)
        {
            for (int i = 0; i < boardYLength; i++)
            {
                for (int j = 0; j < boardXLength; j++)
                {

                    if (i == 0 || j == 0 || i == boardYLength - 1 || j == boardXLength - 1)
                    {
                        board[i, j] = '+';
                    }
                    else if (i == wallPosY && j != wallGapPosX)
                    {
                        board[i, j] = 'x';
                    }
                    else if (i == goalY && j == goalX)
                    {
                        board[i, j] = 'O';
                    }
                    else
                    {
                        board[i, j] = '-';
                    }
                }

            }
            return board;
        }

        private static char[,] Intro(char[,] board, Level level)
        {


            Console.Write("\n\n The bot is ready, let's try a calling a simple function.");
            Console.Write("\n Functions a pre defined and allow us to move and interact with the board.\n We will start with the move function.");
            Console.Write("\n Start by typing in 'move': ");

            string command = "";
            do
            {
                command = Console.ReadLine();
                command = command.ToLower();
                if (command != "move")
                    Console.Write("\n\nPlease, type in 'move' to begin with...");
                else
                {
                    Console.Write("\nGreat, now we need a direction.");
                }
            } while (command != "move");

            Console.Write("\n Please give the bot a direction to move in.\n Type 'up', 'right', 'down', 'left' for the direction you want the bot to move in.");
            bool waiting = true;

            while (waiting)
            {
                command = Console.ReadLine();
                command = command.ToLower();
                if (command != "up")
                {
                    if (command != "right")
                    {
                        if (command != "down")
                        {
                            if (command != "left")
                            {
                                Console.Write("\n\nPlease, type in 'up', 'right', 'down', 'left' for the direction.\n");
                            }
                            else
                            {
                                Console.Write("\nGreat, now we need a distance...");
                                waiting = false;
                            }
                        }
                        else
                        {
                            Console.Write("\nGreat, now we need a distance...");
                            waiting = false;
                        }
                    }
                    else
                    {
                        Console.Write("\nGreat, now we need a distance...");
                        waiting = false;
                    }
                }
                else
                {
                    Console.Write("\nGreat, now we need a distance...");
                    waiting = false;
                }
            }


            string moveCountString = "0";
            int moveCount = 0;
            bool waitingToMove = true;
            while (waitingToMove)
            {
                int moveCountMax = 10;
                if (command == "right" || command == "left")
                {
                    moveCountMax = boardXLength - 2;
                    Console.Write("\n Please give the bot a distance to move between 1 and " + moveCountMax.ToString() + " blocks: ");

                }
                else
                {
                    moveCountMax = boardYLength - 2;
                    Console.Write("\n Please give the bot a distance to move between 1 and " + moveCountMax.ToString() + " blocks: ");
                }

                moveCountString = Console.ReadLine();
                moveCount = Convert.ToInt32(moveCountString);
                if (moveCount > 0)
                {
                    if (moveCount < moveCountMax)
                    {
                        Console.Write("\n Great, we will move the bot " + moveCount + " paces '" + command + "'.");
                        waitingToMove = false;
                    }
                }
            }

            board = MoveBot(command, moveCount, board, level);
            Console.Write("\n\n\n\n Here's the updated board...");
            DrawBoard(board, level);

            return board;
        }


        private static Level LoadLevel(int levelNo)
        {
            if (levelNo == 1)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(BotProgram.Properties.Resources.level_1);
                XmlSerializer x = new XmlSerializer(typeof(Level));
                return (Level)x.Deserialize(new StringReader(doc.InnerXml));
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(BotProgram.Properties.Resources.level_2);
                XmlSerializer x = new XmlSerializer(typeof(Level));
                return (Level)x.Deserialize(new StringReader(doc.InnerXml));
            }
        }

        static void Main(string[] args)
        {

            //Level level = new Level();

            //level.BoardHeight = 30;
            //level.BoardWidth = 70;
            //Exit exit = new Exit { XPos = 68, YPos = 3 };
            //level.Exit = exit;
            //Wall wall_One = new Wall {XPos = 15, YPos=0,Horizontal = false };
            //Wall wall_Two = new Wall { XPos = 30, YPos = 0, Horizontal = false };
            //Wall wall_Three = new Wall { XPos = 17, YPos = 15, Horizontal = true };
            //Wall wall_Four = new Wall { XPos = 40, YPos = 16, Horizontal = false };
            //Wall wall_Five = new Wall { XPos = 50, YPos = 18, Horizontal = false };
            //level.Walls = new List<Wall>();
            //level.Walls.Add(wall_One);
            //level.Walls.Add(wall_Two);
            //level.Walls.Add(wall_Three);
            //level.Walls.Add(wall_Four);
            //level.Walls.Add(wall_Five);
            //level.Gates = new List<Gate>();
            //level.Gates.Add(new Gate { GateID = 1, XPos = 16, YPos = 15 });
            //level.Gates.Add(new Gate { GateID = 2, XPos = 30, YPos = 39 });
            //level.Gates.Add(new Gate { GateID = 3, XPos = 60, YPos = 15 });
            //level.Keys = new List<Key>();
            //level.Keys.Add(new Key { GateID = 3, XPos = 2, Ypos = 38 });
            //level.Keys.Add(new Key { GateID = 1, XPos = 28, Ypos = 17 });
            //level.Keys.Add(new Key { GateID = 2, XPos = 16, Ypos = 2 });

            //XmlWriter x = new XmlWriter();
            //x.WriteLevelToFile(level, 2);

           
            currentLevel = 1;
            while (currentLevel <= 2)
            {
                Level level = LoadLevel(currentLevel);
                //string[] text = BotProgram.Properties.Resources.level1.Split('\n');
                Console.Write("Welcome to Bot Program!");

                Console.Write("\nWithin this program we can do several things.");
                Console.Write("\nFirst things first, let's get a board drawn for you.");


                GenBoardSizes();
                char[,] board = LoadLevelToBoard(level);


                //board = GenBoard(board);

                DrawBoard(board, level);


                Console.Write("\n\n\nWith the board drawn, let's get the bot in place.");
                board[1, 1] = 'B';

                DrawBoard(board, level);


                board = Intro(board, level);

                Console.Write("\n\n\n\n Do you know what you're doing now?\n Let's try get to the goal 'O'.");



                bool playing = true;
                bool regen = true;
                bombCount = 3;
                playerLives = 3;

                
                while (playing)
                {
                    playing = Playing(playing, board, level);
                    
                }
                
            }
        }



        
    }
}
