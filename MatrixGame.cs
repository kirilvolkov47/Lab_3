using System;
using System.Collections.Generic;

namespace Lab_3
{
    class MatrixGame
    {
        public static void StartGame(double[,] a)
        {
            Console.WriteLine("Вхідна матриця виграшу:");
            PrintMatrix(a);

            int si, sj;
            if (Saddle(a, out si, out sj))
            {
                Console.WriteLine("Сідлова точка: a[{0},{1}] = {2}", si + 1, sj + 1, a[si, sj]);
                Console.WriteLine("Розв'язок в чистих стратегіях.");
                return;
            }

            Console.WriteLine("Сідлової точки немає");
            Console.WriteLine();

            double add = 0;
            double minVal = a[0, 0];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    if (a[i, j] < minVal)
                        minVal = a[i, j];
                }
            }

            if (minVal <= 0)
                add = -minVal + 1;

            double[,] newA = AddConst(a, add);

            if (add > 0)
            {
                Console.WriteLine("Матриця після зсуву на {0}:", add);
                PrintMatrix(newA);
            }

            int m = a.GetLength(0);
            int n = a.GetLength(1);

            double[,] tbl = new double[m + 1, n + 1];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                    tbl[i, j] = newA[i, j];
                tbl[i, n] = 1.0;
            }
            for (int j = 0; j < n; j++)
                tbl[m, j] = -1.0;
            tbl[m, n] = 0.0;

            var left = new List<string>();
            var top = new List<string>();
            var dleft = new List<string>();
            var dtop = new List<string>();

            for (int i = 0; i < m; i++)
            {
                left.Add("y" + (i + 1));
                dleft.Add("u" + (i + 1));
            }
            left.Add("Z");
            dleft.Add("1");

            for (int j = 0; j < n; j++)
            {
                top.Add("x" + (j + 1));
                dtop.Add("v" + (j + 1));
            }
            top.Add("1");
            dtop.Add("W");

            MJV.RunDual(tbl, left, top, dleft, dtop, n, m);

            double[] x, u;
            double F;
            MJV.ExtractResult(tbl, left, dtop, n, m, out x, out u, out F);

            if (Math.Abs(F) < 1e-9)
            {
                Console.WriteLine("Помилка: F = 0");
                return;
            }

            double vNew = 1.0 / F;
            double[] p = new double[m];
            double[] q = new double[n];
            for (int i = 0; i < m; i++)
                p[i] = u[i] * vNew;
            for (int j = 0; j < n; j++)
                q[j] = x[j] * vNew;
            double vReal = vNew - add;

            Console.WriteLine("Змішані стратегії:");
            Console.Write("p = (");
            PrintV(p);
            Console.WriteLine(")");
            Console.Write("q = (");
            PrintV(q);
            Console.WriteLine(")");
            Console.WriteLine("Ціна гри = {0:F4}", vReal);
            Console.WriteLine();

            Console.Write("Кількість партій:");
            int games = int.Parse(Console.ReadLine());
            Console.WriteLine();
            RandomGame(a, p, q, games, vReal);
        }

        static void PrintMatrix(double[,] a)
        {
            int r = a.GetLength(0);
            int c = a.GetLength(1);
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                    Console.Write("{0,8:F2}", a[i, j]);
                Console.WriteLine();
            }
        }

        static void PrintV(double[] v)
        {
            for (int i = 0; i < v.Length; i++)
            {
                Console.Write("{0:F4}", v[i]);
                if (i < v.Length - 1)
                    Console.Write("; ");
            }
        }

        static bool Saddle(double[,] a, out int si, out int sj)
        {
            int rows = a.GetLength(0);
            int cols = a.GetLength(1);
            double lower = double.MinValue;
            int lowerRow = -1;

            for (int i = 0; i < rows; i++)
            {
                double rowMin = a[i, 0];
                for (int j = 1; j < cols; j++)
                {
                    if (a[i, j] < rowMin)
                        rowMin = a[i, j];
                }
                if (rowMin > lower)
                {
                    lower = rowMin;
                    lowerRow = i;
                }
            }

            double upper = double.MaxValue;
            int upperCol = -1;
            for (int j = 0; j < cols; j++)
            {
                double colMax = a[0, j];
                for (int i = 1; i < rows; i++)
                {
                    if (a[i, j] > colMax)
                        colMax = a[i, j];
                }
                if (colMax < upper)
                {
                    upper = colMax;
                    upperCol = j;
                }
            }

            Console.WriteLine("Нижня ціна гри = {0}", lower);
            Console.WriteLine("Верхня ціна гри = {0}", upper);

            if (Math.Abs(lower - upper) < 1e-9)
            {
                si = lowerRow;
                sj = upperCol;
                return true;
            }
            si = sj = -1;
            return false;
        }

        static double[,] AddConst(double[,] a, double add)
        {
            int r = a.GetLength(0);
            int c = a.GetLength(1);
            double[,] res = new double[r, c];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                    res[i, j] = a[i, j] + add;
            }
            return res;
        }

        static void RandomGame(double[,] a, double[] p, double[] q, int games, double vReal)
        {
            Random r = new Random();
            int rows = p.Length;
            int cols = q.Length;
            int[] countA = new int[rows];
            int[] countB = new int[cols];
            double sum = 0.0;

            Console.WriteLine("Протокол моделювання");
            Console.WriteLine("{0,5} {1,10} {2,5} {3,10} {4,5} {5,8} {6,12} {7,12}", "№", "randA", "A", "randB", "B", "Виграш", "Накоп.", "Сер.");

            for (int k = 1; k <= games; k++)
            {
                double xa = r.NextDouble();
                double xb = r.NextDouble();
                int i = GetStrat(p, xa);
                int j = GetStrat(q, xb);
                double win = a[i, j];
                sum += win;
                countA[i]++;
                countB[j]++;

                Console.WriteLine("{0,5} {1,10:F4} {2,5} {3,10:F4} {4,5} {5,8:F2} {6,12:F2} {7,12:F4}",
                    k, xa, i + 1, xb, j + 1, win, sum, sum / k);
            }

            Console.WriteLine();
            Console.WriteLine("Експериментальні стратегії гравця A:");
            for (int i = 0; i < rows; i++)
                Console.WriteLine("  p{0} = {1:F4} (теор. {2:F4})", i + 1, (double)countA[i] / games, p[i]);

            Console.WriteLine("Експериментальні стратегії гравця B:");
            for (int j = 0; j < cols; j++)
                Console.WriteLine("  q{0} = {1:F4} (теор. {2:F4})", j + 1, (double)countB[j] / games, q[j]);

            Console.WriteLine("Середній виграш = {0:F4}, теор. ціна = {1:F4}", sum / games, vReal);
        }

        static int GetStrat(double[] prob, double r)
        {
            double s = 0.0;
            for (int i = 0; i < prob.Length; i++)
            {
                s += prob[i];
                if (r < s)
                    return i;
            }
            return prob.Length - 1;
        }
    }
}