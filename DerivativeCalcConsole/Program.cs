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
            Expression expression = new("(2x+5)/x", "x");
            expression.Show();
            expression.Differentiate();
            expression.Show();
        }
    }
}
