using System;
using System.Collections.Generic;
using AngouriMath;
using ExprTree;

namespace ArithmeticParser
{
    class Expression
    {
        static readonly HashSet<string> functions = new HashSet<string> { "sin", "cos", "tg", "cotg", "abs", "sqrt" };
        static readonly HashSet<string> operators = new HashSet<string> { "+", "-", "*", "/", "^" };
        Head tree;

        private List<string> expr = new();
        public List<string> Expr
        {
            get { return expr; }
            private set { expr = value; }
        }

        public Expression(string newexpr)
        {
            Entity newexpression = newexpr;
            string[] simplified = newexpression.Simplify().ToString().Split(' ');
            Prepare(simplified);
            ToPrefix();
        }

        void Prepare(string[] simplified) // Adds the expression as a List in expr
        {
            for (int i = 0; i < simplified.Length; i++)
            {
                string current = simplified[i];
                if (current.Length > 1)
                {
                    if (Char.IsLetter(current[0]) && functions.Contains(current))
                        expr.Add(current);
                    else if (Char.IsDigit(current[0]))
                        expr.Add(current);
                    else if (current[0] == '(')
                    {
                        expr.Add(Char.ToString(current[0]));
                        expr.Add(current.Substring(1));
                    }
                    else if (current[^1] == ')')
                    {
                        expr.Add(current.Substring(0, current.Length - 1));
                        expr.Add(Char.ToString(current[^1]));
                    }
                    else
                    {
                        Console.WriteLine("PARSER ERROR: Unknown function");
                    }
                }
                else if (operators.Contains(current) || Char.IsDigit(current[0]) || Char.IsLetter(current[0]))
                {
                    expr.Add(current);
                }
            }
        }
        int GetPriority(string oper)
        {
            switch (oper)
            {
                case "+":
                    return 1;
                case "-":
                    return 1;
                case "*":
                    return 2;
                case "/":
                    return 2;
                case "^":
                    return 3;
                default:
                    return 0;
            }
        }
        public void Show()
        {
            for (int i = 0; i < expr.Count; i++)
                Console.Write(expr[i] + ' ');
        }
        void ToPrefix()
        {
            List<string> prefix = new();
            Stack<string> operatorstack = new();
            for (int i = expr.Count - 1; i >= 0; i--)
            {
                string current = expr[i];
                if (operators.Contains(current))
                {
                    if (operatorstack.Count > 0)
                    {
                        if (GetPriority(current) > GetPriority(operatorstack.Peek()))
                        {
                            operatorstack.Push(current);
                        }
                        else if (GetPriority(current) < GetPriority(operatorstack.Peek()))
                        {
                            do
                            {
                                prefix.Add(operatorstack.Pop());
                            }
                            while (operatorstack.Count > 0 && GetPriority(current) < GetPriority(operatorstack.Peek()));
                            operatorstack.Push(current);
                        }
                        else
                        {
                            if (operatorstack.Peek() == "^")
                            {
                                do
                                {
                                    prefix.Add(operatorstack.Pop());
                                }
                                while (operatorstack.Count > 0 && GetPriority(current) <= GetPriority(operatorstack.Peek()) && operatorstack.Peek() != "^");
                                operatorstack.Push(current);
                            }
                            else
                            {
                                operatorstack.Push(current);
                            }
                        }
                    }
                    else
                    {
                        operatorstack.Push(current);
                    }
                }
                else if(current == ")")
                {
                    operatorstack.Push(current);
                }
                else if (current == "(")
                {
                    while (operatorstack.Peek() != ")")
                    {
                        prefix.Add(operatorstack.Pop());
                    }
                    operatorstack.Pop();
                }
                else
                {
                    prefix.Add(current);
                }
            }
            while (operatorstack.Count != 0)
            {
                prefix.Add(operatorstack.Pop());
            }
            prefix.Reverse();
            expr = prefix;
        }

        public void ToTree()
        {
        }
    }
}
