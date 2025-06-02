using System;

namespace sudoku
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Sudoku AI!");
            Console.WriteLine("This application allows the user to solve a sudoku puzzle,");
            Console.WriteLine("alternatively one can enable the AI by typing 'ai' during the playtime");
            Console.WriteLine("as well as 'exit' in order to leave the application.");
            Console.ReadKey();

            Console.Clear();
            Console.WriteLine("Choose a sudoku type.");
            Console.WriteLine("1 = default sudoku");
            Console.WriteLine("2 = killer sudoku");

            int typeRaw = GetTypeUserInput();
            Sudoku sudoku = new Sudoku((SudokuType)typeRaw);
            sudoku.DisplayCurrentSudoku();

            do
            {
                sudoku.UserInputNumber();
            } while (!sudoku.IsSolved());

            Console.Clear();
            sudoku.DisplayCurrentSudoku();
            Console.WriteLine("The sudoku has been succesfully solved! :)");
            Console.ReadLine();
        }

        public static int GetTypeUserInput()
        {
            int minSizeSudoku = 0;
            int maxSizeSudoku = Enum.GetNames(typeof(SudokuType)).Length;
            int type = 0;
            string input = Console.ReadLine();

            if (!int.TryParse(input, out type) || type < minSizeSudoku || type > maxSizeSudoku)
            {
                if (input != "")
                {
                    Console.WriteLine("Bad user input. Sudoku AI takes the default sudoku type instead.");
                    Console.ReadLine();
                }
            }

            if (input != "")
                type--;

            Console.Clear();
            return type;
        }

        public static int GetSizeUserInput()
        {
            int minSizeSudoku = 2;
            int maxSizeSudoku = 12;
            int size;
            string input = Console.ReadLine();

            if (!int.TryParse(input, out size))
            {
                size = 3;

                if (input != "")
                {
                    Console.WriteLine("Bad user input. Sudoku AI takes default size '3' instead.");
                    Console.ReadLine();
                }
            }

            if (size < minSizeSudoku || size > maxSizeSudoku)
            {
                size = 3;
                Console.WriteLine("Bad user input. Sudoku AI takes default size '3' instead.");
                Console.ReadLine();
            }

            Console.Clear();

            return size;
        }
    }
}
