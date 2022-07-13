using System;
using CSharpMath.Differentiation;

namespace DerivativeCalc
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Expression vzorec = new("abs(x)", "x");
            vzorec.Differentiate();
            Console.WriteLine(vzorec);
        }
    }
}