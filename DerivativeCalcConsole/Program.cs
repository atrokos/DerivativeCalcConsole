using System;
using static CSharpMath.Differentiation;

namespace DerivativeCalc
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Expression vzorec = new("sin(x)", "x");
            vzorec.Differentiate();
            Console.WriteLine(vzorec);
        }
    }
}