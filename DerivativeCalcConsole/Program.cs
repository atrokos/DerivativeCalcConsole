using System;
using CSharpMath.Differentiation;
using System.Diagnostics;

namespace DerivativeCalc
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.OutputEncoding = System.Text.Encoding.Unicode;
            MathExpression vzorec = new(Console.ReadLine(), "x");
            if (!vzorec.IsDifferentiable())
                return;
            vzorec.DifferentiateSteps();
            vzorec.Simplify();
            Console.WriteLine(vzorec);
        }
    }
}