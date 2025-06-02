using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sudoku
{
    class SudokuSolver
    {
        public int maxSudokuNumber;
        public int maxTurns;
        public int turn;

        public char[,] SolveSudoku(Sudoku sudoku)
        {
            maxTurns = 300;
            turn = 0;
            maxSudokuNumber = sudoku.size * sudoku.size;
            Console.WriteLine("Solving...");

            /*do
            {
                turn++;
                Console.WriteLine("Turn #" + turn);

                TakeTurn(sudoku, maxTurn);
            } while (!sudoku.IsSolved() && turn < maxTurn);*/

            //Clear user input
            sudoku.sudoku = (char[,])sudoku.emptySudoku.Clone();

            //AI solution
            sudoku = Search(sudoku, turn);

            if (sudoku != null && sudoku.sudoku != null)
            {
                //sudoku.DisplayCurrentSudoku();

                if (sudoku.IsSolved() || sudoku.sudoku == null || sudoku.sudoku.Length == 0)
                    Console.WriteLine("AI has solved the sudoku!");
                else
                    Console.WriteLine("AI has failed to solve the sudoku!");
            }
            else
                Console.WriteLine("AI has failed to solve the sudoku!");
            
            Console.ReadLine();
            return sudoku.sudoku;
        }

        public Sudoku Search(Sudoku sudoku, int turn)
        {
            HashSet<Cords> possibleCords = RetrievePossibleSolutions(sudoku);

            //Constraints
            if (possibleCords.Count < 1 || sudoku.IsSolved())
            {
                Console.WriteLine("AI stopped for some reason!");
                return sudoku;
            }

            if (turn >= maxTurns)
            {
                sudoku.DisplayCurrentSudoku();
                Console.WriteLine("Max tries reached!");
                return null;
            }

            turn++;
            Console.WriteLine("Turn #" + turn);
            Console.WriteLine("PossibleCords left: " + possibleCords.Count);

            // Retrieve solution with the fewest possibilities
            var possibleCordWithLeastOptions = possibleCords
                .Where(e => e.possibleAnswers.Count >= 1)
                .OrderBy(e => e.possibleAnswers.Count)
                .FirstOrDefault();

            if (possibleCordWithLeastOptions == null)
            {
                Console.WriteLine("No possible answers left!");
                return sudoku;
            }

            //Try to enter answers
            foreach (char possibleAnswer in possibleCordWithLeastOptions.possibleAnswers)
            {
                //Alternative universe
                Sudoku possibleSudoku = new Sudoku(sudoku.type, sudoku.size);
                possibleSudoku.sudoku = (char[,])sudoku.sudoku.Clone();
                possibleSudoku.EnterNumber(int.Parse(possibleAnswer.ToString()), possibleCordWithLeastOptions.row + 1, possibleCordWithLeastOptions.column + 1);
                Console.WriteLine("Try " + possibleAnswer + " at row #" + (possibleCordWithLeastOptions.row + 1) + " at column #" + (possibleCordWithLeastOptions.column + 1));

                Sudoku result = Search(possibleSudoku, turn);

                if (result != null)
                    return result;
            }

            return sudoku;
        }

        public HashSet<Cords> RetrievePossibleSolutions(Sudoku sudoku)
        {
            HashSet<Cords> cords = new HashSet<Cords>();

            //Get possible cords
            for (int i = 0; i < maxSudokuNumber; i++)
            {
                for (int j = 0; j < maxSudokuNumber; j++)
                {
                    if (sudoku.sudoku[i, j] == char.Parse("."))
                    {
                        cords.Add(new Cords(i, j));
                    }
                }
            }

            //Apply constraints
            foreach (Cords cord in cords)
            {
                for (int i = 0; i < maxSudokuNumber; i++)
                {
                    if (sudoku.IsSpotValid(char.Parse((i + 1).ToString()), cord.row, cord.column))
                        cord.possibleAnswers.Add(char.Parse((i+1).ToString()));
                }
            }
            return cords;
        }

        //Old
        public void TakeTurn(Sudoku sudoku, int maxTurn)
        {
            int number = 0;
            int row = 1;
            int column = 1;
            int tries = 0;

            do
            {
                number++;
                tries++;

                if (number > 9)
                {
                    number = 1;
                    row++;
                }

                if (row > 9)
                {
                    number = 1;
                    row = 1;
                    column++;
                }

                if (number >= maxSudokuNumber && row >= maxSudokuNumber && column >= maxSudokuNumber)
                {
                    Console.WriteLine("Max tries reached!");
                    break;
                }

            } while (!(sudoku.emptySudoku[row - 1, column - 1] == char.Parse(".") && sudoku.IsSpotValid(char.Parse(number.ToString()), row - 1, column - 1) && sudoku.sudoku[row - 1, column - 1] == char.Parse(".")));

            Console.WriteLine(number + " on row " + row + " at column " + column);
            sudoku.EnterNumber(number, row, column);
        }
    }

    public class Cords
    {
        public int row = 1;
        public int column = 1;
        public HashSet<char> possibleAnswers = new HashSet<char>();

        public Cords(int row, int column)
        {
            this.row = row;
            this.column = column;
        }
    }
}
