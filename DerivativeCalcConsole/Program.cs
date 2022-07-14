using System;
using CSharpMath.Differentiation;
using System.Diagnostics;

namespace DerivativeCalc
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new();
            MathExpression vzorec = new("25x+10x^2-6x^3", "x");
            vzorec.Differentiate();
            sw.Start();
            Console.WriteLine(vzorec);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}