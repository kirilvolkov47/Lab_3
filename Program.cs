using System;
using System.Text;

namespace Lab_3
{
    class Program
    {
        static double[,] A1Matrix = {
            { 1,  1,  2 },
            {-1, -1,  5 },
            { 2,  3, -3 }
        };

        static double[,] ReadMatrix()
        {
            int rows = 0;
            while (rows <= 0)
            {
                Console.Write("Кількість рядків: ");
                if (!int.TryParse(Console.ReadLine(), out rows) || rows <= 0)
                    Console.WriteLine("Введіть коректне число");
            }

            int cols = 0;
            while (cols <= 0)
            {
                Console.Write("Кількість стовпців: ");
                if (!int.TryParse(Console.ReadLine(), out cols) || cols <= 0)
                    Console.WriteLine("Введіть коректне число");
            }

            double[,] mat = new double[rows, cols];
            Console.WriteLine("Вводіть рядки через пробіл:");
            for (int i = 0; i < rows; i++)
            {
                bool ok = false;
                while (!ok)
                {
                    Console.Write("  Рядок " + (i + 1) + ": ");
                    string[] parts = Console.ReadLine().Split(' ');
                    if (parts.Length != cols)
                    {
                        Console.WriteLine("Невірна кількість елементів");
                        continue;
                    }
                    ok = true;
                    for (int j = 0; j < cols; j++)
                    {
                        if (!double.TryParse(parts[j], out mat[i, j]))
                        {
                            Console.WriteLine("Невірний формат числа");
                            ok = false;
                            break;
                        }
                    }
                }
            }
            return mat;
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("1 - Варіант 7 (A1, 3x3)");
                Console.WriteLine("2 - Ввести матрицю вручну");
                Console.WriteLine("0 - Вихід");
                Console.Write(">> ");
                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1": MatrixGame.StartGame(A1Matrix); break;
                    case "2": MatrixGame.StartGame(ReadMatrix()); break;
                    case "0": exit = true; break;
                    default: Console.WriteLine("Невірний вибір"); break;
                }
                Console.WriteLine();
            }
        }
    }
}