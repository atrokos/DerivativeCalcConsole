using System;
using System.Collections.Generic;
using ArithmeticParser;
using AngouriMath;
using ExprTree;

namespace DerivativeCalc
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Expression expression = new("x", "x"); //+ + 2 * 2 ^ x 2 * 5 x
            expression.Show();
            expression.Differentiate();
            Console.WriteLine();
            expression.ShowDiff();
        }
    }
}
