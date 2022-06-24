using System;
using System.Collections.Generic;
using ArithmeticParser;

namespace DerivativeCalc
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Expression expression = new("2x");
            expression.Show();

        }
    }
}
