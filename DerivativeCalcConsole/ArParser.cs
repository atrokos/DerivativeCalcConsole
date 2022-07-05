﻿using System;
using System.Collections.Generic;
using AngouriMath;
using ExprTree;

namespace ArithmeticParser
{
    class Expression
    {
        static readonly HashSet<string> functions = new HashSet<string> { "sin", "cos", "tg", "cotg", "abs", "sqrt", "ln" };
        static readonly HashSet<string> operators = new HashSet<string> { "+", "-", "*", "/", "^" };
        string VAR;
        int pos = 0;
        string result = null;
        Head tree;
        List<string> expr = new();
        List<string> diffexpr = new();

        public Expression(string newexpr, string vAR)
        {
            Entity newexpression = newexpr;
            string[] simplified = newexpression.Simplify().ToString().Split(' ');
            Prepare(simplified);
            ToPrefix();
            VAR = vAR;
        }
        double StringToDouble(string number)
        {
            double result = 0;
            try
            {
                result = Double.Parse(number);
            }
            catch (FormatException) // If it fails, it has to be division (AngouriMath.Simplify does not output any other types of numbers like that)
            {
                int divpos = 0;
                for (int i = 0; i < number.Length; i++) // Find the '/'
                {
                    if (number[i] == '/')
                        divpos = i;
                }
                result = Double.Parse(number[0..^(divpos + 1)]) / Double.Parse(number[(divpos + 1)..]);
            }
            return result;
        }
        void Prepare(string[] simplified) // Adds the expression as a List in expr
        {
            for (int i = 0; i < simplified.Length; i++)
            {
                string current = simplified[i];
                if (current.Length > 1)
                {
                    if (functions.Contains(current))
                        expr.Add(current);
                    else if (Char.IsDigit(current[0]))
                        expr.Add(current);
                    else if (current[0] == '(' && current[^1] == ')')
                    {
                        expr.Add("(");
                        expr.Add((string)current[1..^1]);
                        expr.Add(")");
                    }
                    else if (current[0] == '(')
                    {
                        expr.Add("(");
                        expr.Add(current[1..]);
                    }
                    else if (current[^1] == ')')
                    {
                        expr.Add(current[..^1]);
                        expr.Add(")");
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
        public void ShowResult()
        {
            //for (int i = 0; i < expr.Count; i++)
            //    Console.Write(expr[i] + ' ');
            //Console.WriteLine();
            Console.WriteLine(result);
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
        void PrefixToTree()
        {
            Head node = new();
            node.Add(buildTree());
            tree = node;
        }
        INode buildTree()
        {
            INode node = null;
            
            if (!operators.Contains(expr[pos]) && expr[pos] != VAR) //It is a number
            {
                double val = StringToDouble(expr[pos]);
                Constant constant = new(val, null);
                node = constant;
            }
            else if (expr[pos] == VAR) //It is a derivative variable
            {
                DiffVariable variable = new(null);
                node = variable;
            }
            else //It is an operator
            {
                switch (expr[pos])
                {
                    case "+":
                        Plus plus = new(null);
                        pos++;
                        plus.Add(buildTree());

                        pos++;
                        plus.Add(buildTree());
                        node = plus;
                        break;

                    case "-":
                        Minus minus = new(null);
                        pos++;
                        minus.Add(buildTree());

                        pos++;
                        minus.Add(buildTree());
                        node = minus;
                        break;

                    case "*":
                        Multi multi = new(null);
                        pos++;
                        multi.Add(buildTree());

                        pos++;
                        multi.Add(buildTree());
                        node = multi;
                        break;

                    case "/":
                        Divi divi = new(null);
                        pos++;
                        divi.Add(buildTree());

                        pos++;
                        divi.Add(buildTree());
                        node = divi;
                        break;

                    case "^":
                        Power power = new(null);
                        pos++;
                        power.Add(buildTree());

                        pos++;
                        power.Add(buildTree());
                        node = power;
                        break;
                }
            }
            return node;
        }
        public void Differentiate()
        {
            PrefixToTree();
            tree.Differentiate();
            TreeToPrefix();
            expr = diffexpr;
            PrefixToInfix();
        }
        void TreeToPrefix()
        {
            Stack<INode> stack = new();

            stack.Push(tree.leftchild);

            while (stack.Count > 0)
            {
                INode node = stack.Pop();
                if (node is Constant constant)
                {
                    double val = constant.Value;
                    diffexpr.Add(val.ToString("G"));
                }
                else if (node is DiffVariable)
                {
                    diffexpr.Add(VAR);
                }
                else if (node is OPNode opnode)
                {
                    if (node is Plus)
                    {
                        diffexpr.Add("+");
                        stack.Push(opnode.rightchild);
                        stack.Push(opnode.leftchild);
                    }
                    else if (node is Minus)
                    {
                        diffexpr.Add("-");
                        stack.Push(opnode.rightchild);
                        stack.Push(opnode.leftchild);
                    }
                    else if (node is Multi)
                    {
                        diffexpr.Add("*");
                        stack.Push(opnode.rightchild);
                        stack.Push(opnode.leftchild);
                    }
                    else if (node is Divi)
                    {
                        diffexpr.Add("/");
                        stack.Push(opnode.rightchild);
                        stack.Push(opnode.leftchild);
                    }
                    else if (node is Power)
                    {
                        diffexpr.Add("^");
                        stack.Push(opnode.rightchild);
                        stack.Push(opnode.leftchild);
                    }
                }
            }
        }
        void PrefixToInfix()
        {
            Stack<string> stack = new();

            for (int i = diffexpr.Count - 1; i >= 0; i--)
            {
                string curr = diffexpr[i];
                if (operators.Contains(curr))
                {
                    string op1 = stack.Pop();
                    string op2 = stack.Pop();

                    string temp = "(" + op1 + curr + op2 + ")";
                    stack.Push(temp);
                }
                else
                {
                    stack.Push(curr);
                }
            }
            result = stack.Pop();
        }
    }
}
