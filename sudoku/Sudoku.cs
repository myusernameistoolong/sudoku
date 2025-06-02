using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sudoku
{
    public enum SudokuType
    {
        Default = 0,
        Killer = 1
    }

    class Sudoku
    {
        public char[,] sudoku;
        public char[,] emptySudoku;
        public List<Relation> killerSudokuRelations = new List<Relation>();
        public SudokuType type = SudokuType.Default;
        public int size = 3;

        public Sudoku(SudokuType type = SudokuType.Default, int size = 3)
        {
            this.type   = type;
            this.size   = size;

            Color.colors.Add(ConsoleColor.DarkYellow);
            Color.colors.Add(ConsoleColor.DarkGreen);
            Color.colors.Add(ConsoleColor.DarkBlue);
            Color.colors.Add(ConsoleColor.DarkRed);
            Color.colors.Add(ConsoleColor.DarkMagenta);
            Color.colors.Add(ConsoleColor.DarkCyan);
            Color.colors.Add(ConsoleColor.DarkGray);
            Color.colors.Add(ConsoleColor.Black);
            GenerateSudoku(type, size);
        }

        public void DisplayCurrentSudoku()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            string line = "=";

            //Output relation schema
            if (type == SudokuType.Killer && killerSudokuRelations.Count > 0)
            {
                for (int k = 0; k < size; k++)
                    for (int l = 0; l <= size; l++)
                        line += "=";

                Console.WriteLine("Relation schema:");
                Console.WriteLine(line);

                foreach (Relation relation in killerSudokuRelations)
                {
                    Console.WriteLine(relation.color.ToString() + ": " + relation.value);
                }
                Console.WriteLine();
            }

            for (int i = 0; i <= sudoku.GetLength(0) - 1; i++)
            {
                //Generate lines
                if (i % 3 == 0)
                {
                    line = "=";

                    for (int k = 0; k < size; k++)
                        for (int l = 0; l <= size; l++)
                            line += "=";

                    Console.WriteLine(line);
                }

                //Generate numbers
                for (int j = 0; j <= sudoku.GetLength(1) - 1; j++)
                {
                    if (j % 3 == 0)
                        Console.Write("|");

                    if (sudoku[i, j] != char.Parse(".") && emptySudoku[i, j] == char.Parse("."))
                        Console.ForegroundColor = ConsoleColor.Green;

                    //Set relation color
                    if (type == SudokuType.Killer && killerSudokuRelations.Count > 0)
                    {
                        Relation connectedRelation = null;

                        foreach (Relation relation in killerSudokuRelations)
                        {
                            foreach (int[] cords in relation.relationCords)
                            {
                                if (cords[0] == i && cords[1] == j)
                                {
                                    connectedRelation = relation;
                                    break;
                                }

                                if (connectedRelation != null)
                                    break;
                            }
                        }

                        if (connectedRelation == null)
                        {
                            //Console.WriteLine("Relationship missing!");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.BackgroundColor = connectedRelation.color;
                        }
                    }

                    Console.Write(sudoku[i, j]);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                Console.Write("|");
                Console.WriteLine();
            }

            //Generate final line
            line = "=";

            for (int k = 0; k < size; k++)
                for (int l = 0; l <= size; l++)
                    line += "=";

            Console.WriteLine(line);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void UserInputNumber()
        {
            string input;
            int number  = 0;
            int row     = 0;
            int column  = 0;
            bool validUserInput = false;

            do
            {
                Console.WriteLine("Enter a valid number between 1 and 9 and its positions or type 'ai' to let the AI solve the sudoku for you.");
                Console.WriteLine("Format: <number> <row> <column>   e.g. 4 2 3");
                input = Console.ReadLine();

                //Enable AI
                if(input == "ai")
                {
                    SolveSudoku();
                    break;
                }

                //Exit
                if (input == "exit")
                {
                    System.Environment.Exit(1);
                }

                //Try entering an answer
                string[] splitInputs = input.Split(' ');

                if (splitInputs.Length == 3 && int.TryParse(splitInputs[0], out number) && int.TryParse(splitInputs[1], out row) && int.TryParse(splitInputs[2], out column))
                    validUserInput = true;
                else
                    Console.WriteLine("Invalid user input! Please try again.");

            } while (!validUserInput || !EnterNumber(number, row, column));

            DisplayCurrentSudoku();
        }

        public bool EnterNumber(int number = 0, int row = 0, int column = 0)
        {
            if (IsSolved())
                return false;

            if (number >= 0 && number <= 9 && row >= 1 && row <= 9 && column >= 1 && column <= 9)
            {
                if (emptySudoku[row - 1, column - 1] == char.Parse("."))
                {
                    if (IsSpotValid(char.Parse(number.ToString()), row - 1, column - 1))
                    {
                        if(number == 0)
                            sudoku[row - 1, column - 1] = char.Parse(".");
                        else
                            sudoku[row - 1, column - 1] = char.Parse(number.ToString());
                        return true;
                    }
                    else
                        Console.WriteLine("The entered answer is already present in the corresponding column and/or row. Please try again.");
                }
                else
                {
                    Console.WriteLine("This spot is already occupied by a static number. Please enter your number in an available spot.");
                }
            }
            else
                Console.WriteLine("Entered digits are not between 1 and 9. Please try again.");

            return false;
        }

        public bool IsSpotValid(char number, int row, int column)
        {
            for (int i = 0; i < (size * size); i++)
            {
                //check row  
                if (sudoku[i, column] != '.' && sudoku[i, column] == number)
                    return false;
                
                //check column  
                if (sudoku[row, i] != '.' && sudoku[row, i] == number)
                    return false;
                
                //check 3*3 block
                if (sudoku[3 * (row / 3) + i / 3, 3 * (column / 3) + i % 3] != '.' && sudoku[3 * (row / 3) + i / 3, 3 * (column / 3) + i % 3] == (char)number)
                    return false;
            }

            //Killer sudoku quota
            if (type == SudokuType.Killer && killerSudokuRelations.Count > 0 && sudoku[row, column] == '.')
            {
                int sum = 0;
                int emptyCount = 0;
                Relation connectedRelation = null;

                //Retrieve connected relation
                foreach (Relation relation in killerSudokuRelations)
                {
                    foreach (int[] cords in relation.relationCords)
                    {
                        if (cords[0] == row && cords[1] == column)
                        {
                            connectedRelation = relation;
                            break;
                        }
                    }

                    if (connectedRelation != null)
                        break;
                }

                //Calculate all filled numbers
                foreach (int[] cords in connectedRelation.relationCords)
                {
                    for (int i = 0; i <= sudoku.GetLength(0) - 1; i++)
                    {
                        for (int j = 0; j <= sudoku.GetLength(1) - 1; j++)
                        {
                            if (cords[0] == i && cords[1] == j)
                            {
                                if (sudoku[i, j] == char.Parse("."))
                                    emptyCount++;
                                else
                                    sum += int.Parse(sudoku[i, j].ToString());
                            }
                        }
                    }
                }

                sum += int.Parse(number.ToString());
                if (emptyCount <= 1 && sum != connectedRelation.value)
                    return false;
            }

            return true;
        }

        public bool IsSolved()
        {
            for (int i = 0; i <= sudoku.GetLength(0) - 1; i++)
                for (int j = 0; j <= sudoku.GetLength(1) - 1; j++)
                    if (sudoku[i, j] == char.Parse("."))
                        return false;

            return true;
        }

        public void SolveSudoku()
        {
            SudokuSolver sudokuSolver = new SudokuSolver();
            sudoku = sudokuSolver.SolveSudoku(this);
        }

        public void GenerateSudoku(SudokuType type = SudokuType.Default, int size = 3)
        {
            switch (type)
            {
                case SudokuType.Default:
                    GenerateDefaultSudoku();
                    break;
                case SudokuType.Killer:
                    GenerateKillerSudoku();
                    break;
                default:
                    GenerateDefaultSudoku();
                    break;
            }
        }

        private void GenerateDefaultSudoku()
        {
            var sudoku = new char[,]
            {
                { '5', '3', '.', '.', '7', '.', '.', '.', '.' },
                { '6', '.', '.', '1', '9', '5', '.', '.', '.' },
                { '.', '9', '8', '.', '.', '.', '.', '6', '.' },
                { '8', '.', '.', '.', '6', '.', '.', '.', '3' },
                { '4', '.', '.', '8', '.', '3', '.', '.', '1' },
                { '7', '.', '.', '.', '2', '.', '.', '.', '6' },
                { '.', '6', '.', '.', '.', '.', '2', '8', '.' },
                { '.', '.', '.', '4', '1', '9', '.', '.', '5' },
                { '.', '.', '.', '.', '8', '.', '.', '7', '9' }
            };
            /*var sudoku = new char[,]
            {
                { '.', '1', '.', '.', '6', '.', '.', '.', '7' },
                { '.', '.', '4', '.', '8', '3', '.', '.', '2' },
                { '8', '2', '.', '1', '.', '.', '.', '5', '3' },
                { '4', '8', '.', '.', '.', '.', '.', '3', '.' },
                { '.', '9', '3', '.', '7', '.', '1', '.', '.' },
                { '7', '.', '.', '.', '9', '1', '.', '.', '.' },
                { '.', '.', '8', '6', '.', '.', '.', '.', '1' },
                { '.', '.', '7', '.', '2', '4', '5', '.', '6' },
                { '.', '6', '9', '.', '.', '.', '.', '4', '.' }
            };*/
            /*var sudoku = new char[,]
            {
                { '6', '.', '.', '.', '.', '.', '.', '.', '1' },
                { '.', '.', '8', '.', '7', '.', '4', '.', '.' },
                { '5', '9', '7', '4', '6', '.', '8', '.', '2' },
                { '.', '.', '.', '.', '.', '5', '.', '.', '4' },
                { '.', '.', '.', '.', '8', '.', '.', '.', '.' },
                { '.', '8', '.', '.', '.', '2', '1', '.', '7' },
                { '4', '2', '1', '7', '5', '.', '.', '6', '.' },
                { '.', '7', '.', '.', '3', '4', '.', '1', '5' },
                { '9', '.', '5', '.', '.', '.', '.', '.', '.' }
            };*/
            this.sudoku = sudoku;
            this.emptySudoku = (char[,])sudoku.Clone();
        }

        private void GenerateKillerSudoku()
        {
            var sudoku = new char[,]
            {
                { '.', '.', '.', '.', '.', '.', '.', '.', '.' },
                { '.', '.', '.', '.', '.', '.', '.', '.', '.' },
                { '.', '.', '.', '.', '.', '.', '.', '.', '.' },
                { '.', '.', '.', '.', '.', '.', '.', '.', '.' },
                { '.', '.', '.', '.', '.', '.', '.', '.', '.' },
                { '.', '.', '.', '.', '.', '.', '.', '.', '.' },
                { '.', '.', '.', '.', '.', '.', '.', '.', '.' },
                { '.', '.', '.', '.', '.', '.', '.', '.', '.' },
                { '.', '.', '.', '.', '.', '.', '.', '.', '.' }
            };

            this.sudoku = sudoku;
            this.emptySudoku = (char[,])sudoku.Clone();

            List<int[]> relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 0, 0 });
            relationCords.Add(new int[] { 0, 1 });
            killerSudokuRelations.Add(new Relation(3, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 0, 2 });
            relationCords.Add(new int[] { 0, 3 });
            relationCords.Add(new int[] { 0, 4 });
            killerSudokuRelations.Add(new Relation(15, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 0, 5 });
            relationCords.Add(new int[] { 1, 5 });
            relationCords.Add(new int[] { 1, 4 });
            relationCords.Add(new int[] { 2, 4 });
            killerSudokuRelations.Add(new Relation(22, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 0, 6 });
            relationCords.Add(new int[] { 1, 6 });
            killerSudokuRelations.Add(new Relation(4, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 0, 7 });
            relationCords.Add(new int[] { 1, 7 });
            killerSudokuRelations.Add(new Relation(16, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 0, 8 });
            relationCords.Add(new int[] { 1, 8 });
            relationCords.Add(new int[] { 2, 8 });
            relationCords.Add(new int[] { 3, 8 });
            killerSudokuRelations.Add(new Relation(15, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 1, 0 });
            relationCords.Add(new int[] { 1, 1 });
            relationCords.Add(new int[] { 2, 0 });
            relationCords.Add(new int[] { 2, 1 });
            killerSudokuRelations.Add(new Relation(25, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 1, 2 });
            relationCords.Add(new int[] { 1, 3 });
            killerSudokuRelations.Add(new Relation(17, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 2, 2 });
            relationCords.Add(new int[] { 2, 3 });
            relationCords.Add(new int[] { 3, 3 });
            killerSudokuRelations.Add(new Relation(9, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 2, 5 });
            relationCords.Add(new int[] { 3, 5 });
            relationCords.Add(new int[] { 4, 5 });
            killerSudokuRelations.Add(new Relation(8, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 2, 6 });
            relationCords.Add(new int[] { 2, 7 });
            relationCords.Add(new int[] { 3, 6 });
            killerSudokuRelations.Add(new Relation(20, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 3, 0 });
            relationCords.Add(new int[] { 4, 0 });
            killerSudokuRelations.Add(new Relation(6, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 3, 1 });
            relationCords.Add(new int[] { 3, 2 });
            killerSudokuRelations.Add(new Relation(14, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 3, 4 });
            relationCords.Add(new int[] { 4, 4 });
            relationCords.Add(new int[] { 5, 4 });
            killerSudokuRelations.Add(new Relation(17, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 3, 7 });
            relationCords.Add(new int[] { 4, 7 });
            relationCords.Add(new int[] { 4, 6 });
            killerSudokuRelations.Add(new Relation(17, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 4, 1 });
            relationCords.Add(new int[] { 4, 2 });
            relationCords.Add(new int[] { 5, 1 });
            killerSudokuRelations.Add(new Relation(13, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 4, 3 });
            relationCords.Add(new int[] { 5, 3 });
            relationCords.Add(new int[] { 6, 3 });
            killerSudokuRelations.Add(new Relation(20, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 4, 8 });
            relationCords.Add(new int[] { 5, 8 });
            killerSudokuRelations.Add(new Relation(12, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 5, 0 });
            relationCords.Add(new int[] { 6, 0 });
            relationCords.Add(new int[] { 7, 0 });
            relationCords.Add(new int[] { 8, 0 });
            killerSudokuRelations.Add(new Relation(27, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 5, 2 });
            relationCords.Add(new int[] { 6, 2 });
            relationCords.Add(new int[] { 6, 1 });
            killerSudokuRelations.Add(new Relation(6, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 5, 5 });
            relationCords.Add(new int[] { 6, 5 });
            relationCords.Add(new int[] { 6, 6 });
            killerSudokuRelations.Add(new Relation(20, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 5, 6 });
            relationCords.Add(new int[] { 5, 7 });
            killerSudokuRelations.Add(new Relation(6, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 6, 4 });
            relationCords.Add(new int[] { 7, 4 });
            relationCords.Add(new int[] { 7, 3 });
            relationCords.Add(new int[] { 8, 3 });
            killerSudokuRelations.Add(new Relation(10, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 6, 7 });
            relationCords.Add(new int[] { 6, 8 });
            relationCords.Add(new int[] { 7, 7 });
            relationCords.Add(new int[] { 7, 8 });
            killerSudokuRelations.Add(new Relation(14, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 7, 1 });
            relationCords.Add(new int[] { 8, 1 });
            killerSudokuRelations.Add(new Relation(8, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 7, 2 });
            relationCords.Add(new int[] { 8, 2 });
            killerSudokuRelations.Add(new Relation(16, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 7, 5 });
            relationCords.Add(new int[] { 7, 6 });
            killerSudokuRelations.Add(new Relation(15, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 8, 4 });
            relationCords.Add(new int[] { 8, 5 });
            relationCords.Add(new int[] { 8, 6 });
            killerSudokuRelations.Add(new Relation(13, relationCords));
            relationCords = new List<int[]> { };
            relationCords.Add(new int[] { 8, 7 });
            relationCords.Add(new int[] { 8, 8 });
            killerSudokuRelations.Add(new Relation(17, relationCords));
        }
    }

    public class Relation
    {
        public int value = 2;
        public ConsoleColor color = ConsoleColor.White;
        public List<int[]> relationCords;

        public Relation(int value, List<int[]> relationCords)
        {
            this.value = value;
            this.color = Color.colors[Color.currentColorId];
            this.relationCords = relationCords;
            Color.NextColor();
        }
    }

    public class Color
    {
        public static int currentColorId = 0;
        public static List<ConsoleColor> colors = new List<ConsoleColor>();

        public static void NextColor()
        {
            currentColorId++;

            if (currentColorId >= colors.Count)
                currentColorId = 0;
        }
    }
}
