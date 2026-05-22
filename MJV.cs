using System;
using System.Collections.Generic;

namespace Lab_3
{
    class MJV
    {
        public static void PrintDualTable(double[,] t, List<string> left, List<string> top, List<string> dleft, List<string> dtop, string label = "")
        {
            if (label != "")
                Console.WriteLine(label);

            string line = "            ";
            for (int j = 0; j < dtop.Count; j++)
                line += dtop[j].PadLeft(12);
            Console.WriteLine(line);

            line = "            ";
            for (int j = 0; j < top.Count; j++)
                line += top[j].PadLeft(12);
            Console.WriteLine(line);

            for (int i = 0; i < t.GetLength(0); i++)
            {
                line = (dleft[i] + " " + left[i]).PadRight(12);
                for (int j = 0; j < t.GetLength(1); j++)
                {
                    double v = t[i, j];
                    if (Math.Abs(v) < 1e-9)
                        v = 0;
                    line += v.ToString("F4").PadLeft(12);
                }
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }

        static void MjeStep(ref double[,] t, List<string> left, List<string> top, List<string> dleft, List<string> dtop, int r, int c)
        {
            int rows = t.GetLength(0);
            int cols = t.GetLength(1);
            double mainElement = t[r, c];
            Console.WriteLine("  Розв. елемент [{0}][{1}] = {2:F4}", r, c, mainElement);
            double[,] nt = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i == r && j == c)
                        nt[i, j] = 1.0;
                    else if (i == r)
                        nt[i, j] = t[i, j];
                    else if (j == c)
                        nt[i, j] = -t[i, j];
                    else
                        nt[i, j] = t[i, j] * mainElement - t[i, c] * t[r, j];
                }
            }
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                    t[i, j] = nt[i, j] / mainElement;
            }

            string tmp;
            tmp = left[r];
            left[r] = top[c];
            top[c] = tmp;
            tmp = dleft[r];
            dleft[r] = dtop[c];
            dtop[c] = tmp;
        }

        public static void PrintW(double[,] t, int m, int n)
        {
            Console.WriteLine("Двоїста задача (W):");
            Console.Write("  W = ");
            for (int i = 0; i < m; i++)
            {
                double coef = t[i, n];
                if (i == 0)
                    Console.Write(coef + "*u" + (i + 1));
                else
                {
                    if (coef >= 0)
                        Console.Write(" + " + coef + "*u" + (i + 1));
                    else
                        Console.Write(" " + coef + "*u" + (i + 1));
                }
            }
            Console.WriteLine(" -> min");
            Console.WriteLine();
            Console.WriteLine("  Обмеження:");
            for (int j = 0; j < n; j++)
            {
                Console.Write("    ");
                for (int i = 0; i < m; i++)
                {
                    double a = t[i, j];
                    if (i == 0)
                        Console.Write(a + "*u" + (i + 1));
                    else
                    {
                        if (a >= 0)
                            Console.Write(" + " + a + "*u" + (i + 1));
                        else
                            Console.Write(" " + a + "*u" + (i + 1));
                    }
                }
                Console.WriteLine(" >= " + (-t[m, j]));
            }
            Console.WriteLine();
        }

        static bool FindBase(ref double[,] t, List<string> left, List<string> top, List<string> dleft, List<string> dtop, int m)
        {
            int cols = t.GetLength(1);
            for (int step = 1; step <= 100; step++)
            {
                int r = -1;
                for (int i = 0; i < m; i++)
                {
                    if (t[i, cols - 1] < -1e-5)
                    {
                        r = i;
                        break;
                    }
                }
                if (r == -1)
                    return true;

                int s = -1;
                for (int j = 0; j < cols - 1; j++)
                {
                    if (t[r, j] < -1e-5)
                    {
                        s = j;
                        break;
                    }
                }
                if (s == -1)
                {
                    Console.WriteLine("Система несумісна");
                    return false;
                }

                double best = double.MaxValue;
                int sel = -1;
                for (int i = 0; i < m; i++)
                {
                    if (Math.Abs(t[i, s]) > 1e-5)
                    {
                        double val = t[i, cols - 1] / t[i, s];
                        if (val >= -1e-5 && val < best - 1e-5)
                        {
                            best = val;
                            sel = i;
                        }
                    }
                }
                if (sel == -1)
                {
                    Console.WriteLine("Система несумісна");
                    return false;
                }

                Console.WriteLine("Крок " + step);
                Console.WriteLine("  Від'ємний b в рядку {0} = {1:F4}", left[r], t[r, cols - 1]);
                Console.WriteLine("  Розв. стовпець: {0}", top[s]);
                Console.WriteLine("  Розв. рядок: {0} (min відн. = {1:F4})", left[sel], best);
                MjeStep(ref t, left, top, dleft, dtop, sel, s);
                PrintDualTable(t, left, top, dleft, dtop, "Таблиця після кроку " + step + ":");
            }
            Console.WriteLine("Перевищено ліміт");
            return false;
        }

        static bool FindBest(ref double[,] t, List<string> left, List<string> top, List<string> dleft, List<string> dtop, int m)
        {
            int rows = t.GetLength(0);
            for (int step = 1; step <= 100; step++)
            {
                int cols = t.GetLength(1);
                int s = -1;
                bool negFound = false;
                for (int j = 0; j < cols - 1; j++)
                {
                    if (t[rows - 1, j] < -1e-5)
                    {
                        negFound = true;
                        for (int i = 0; i < m; i++)
                        {
                            if (t[i, j] > 1e-5)
                            {
                                s = j;
                                break;
                            }
                        }
                        if (s != -1)
                            break;
                    }
                }
                if (!negFound || s == -1)
                    return true;

                double best = double.MaxValue;
                int sel = -1;
                for (int i = 0; i < m; i++)
                {
                    if (t[i, s] > 1e-5)
                    {
                        double val = t[i, cols - 1] / t[i, s];
                        if (val >= -1e-5 && val < best)
                        {
                            best = val;
                            sel = i;
                        }
                    }
                }
                if (sel == -1)
                    return true;

                Console.WriteLine("Крок " + step);
                Console.WriteLine("  Розв. стовпець: {0} (Z = {1:F4})", top[s], t[rows - 1, s]);
                for (int i = 0; i < m; i++)
                {
                    if (t[i, s] > 1e-5)
                    {
                        double val = t[i, cols - 1] / t[i, s];
                        Console.WriteLine("    {0}: {1:F4} / {2:F4} = {3:F4}",
                            left[i], t[i, cols - 1], t[i, s], val);
                    }
                }
                Console.WriteLine("  Розв. рядок: {0} (min відн. = {1:F4})", left[sel], best);
                MjeStep(ref t, left, top, dleft, dtop, sel, s);
                PrintDualTable(t, left, top, dleft, dtop, "Таблиця після кроку " + step + ":");
            }
            Console.WriteLine("Перевищено ліміт");
            return false;
        }

        static void PrintPairResult(double[,] t, List<string> left, List<string> dleft, List<string> dtop, int n, int m)
        {
            int rows = t.GetLength(0);
            int cols = t.GetLength(1);

            double[] x = new double[n];
            for (int k = 0; k < n; k++)
            {
                x[k] = 0;
                for (int i = 0; i < m; i++)
                {
                    if (left[i] == "x" + (k + 1))
                    {
                        x[k] = t[i, cols - 1];
                        break;
                    }
                }
                if (Math.Abs(x[k]) < 1e-9)
                    x[k] = 0;
            }

            double[] u = new double[m];
            for (int k = 0; k < m; k++)
            {
                u[k] = 0;
                for (int j = 0; j < cols - 1; j++)
                {
                    if (dtop[j] == "u" + (k + 1))
                    {
                        u[k] = t[rows - 1, j];
                        break;
                    }
                }
                if (Math.Abs(u[k]) < 1e-9)
                    u[k] = 0;
            }

            Console.Write("X = (");
            for (int k = 0; k < n; k++)
            {
                Console.Write(x[k].ToString("F4"));
                if (k < n - 1)
                    Console.Write("; ");
            }
            Console.WriteLine(")");

            Console.Write("U = (");
            for (int k = 0; k < m; k++)
            {
                Console.Write(u[k].ToString("F4"));
                if (k < m - 1)
                    Console.Write("; ");
            }
            Console.WriteLine(")");
            Console.WriteLine();
        }

        public static void RunDual(double[,] t, List<string> left, List<string> top, List<string> dleft, List<string> dtop, int n, int m)
        {
            Console.WriteLine("Початкова симплекс-таблиця:");
            PrintDualTable(t, left, top, dleft, dtop);
            PrintW(t, m, n);

            Console.WriteLine("Пошук опорного розв'язку");
            Console.WriteLine();
            if (!FindBase(ref t, left, top, dleft, dtop, m))
                return;
            Console.WriteLine("Опорний розв'язок знайдено");
            PrintPairResult(t, left, dleft, dtop, n, m);

            Console.WriteLine("Пошук оптимального розв'язку");
            Console.WriteLine();
            if (!FindBest(ref t, left, top, dleft, dtop, m))
                return;
            Console.WriteLine("Оптимальний розв'язок знайдено");
            PrintPairResult(t, left, dleft, dtop, n, m);

            int lastRow = t.GetLength(0) - 1;
            int lastCol = t.GetLength(1) - 1;
            Console.WriteLine("Max Z = {0:F4}", t[lastRow, lastCol]);
            Console.WriteLine("Min W = {0:F4}", t[lastRow, lastCol]);
            Console.WriteLine();
        }

        public static void ExtractResult(double[,] t, List<string> left, List<string> dtop, int n, int m, out double[] x, out double[] u, out double F)
        {
            int rows = t.GetLength(0);
            int cols = t.GetLength(1);
            x = new double[n];
            for (int k = 0; k < n; k++)
            {
                x[k] = 0;
                for (int i = 0; i < m; i++)
                {
                    if (left[i] == "x" + (k + 1))
                        x[k] = t[i, cols - 1];
                }
                if (Math.Abs(x[k]) < 1e-9)
                    x[k] = 0;
            }
            u = new double[m];
            for (int k = 0; k < m; k++)
            {
                u[k] = 0;
                for (int j = 0; j < cols - 1; j++)
                {
                    if (dtop[j] == "u" + (k + 1))
                        u[k] = t[rows - 1, j];
                }
                if (Math.Abs(u[k]) < 1e-9)
                    u[k] = 0;
            }
            F = t[rows - 1, cols - 1];
        }
    }
}