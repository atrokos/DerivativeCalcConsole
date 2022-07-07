using AngouriMath;
using ExprTree;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CSharpMath
{
    internal class Differentiation
    {
        public class Expression
        {
            Head tree;
            Parser parser;
            List<string> expr = new();
            string VAR;
            string expr_string = null;

            public Expression(string newexpr, string vAR)
            {
                Entity newexpression = newexpr;
                VAR = vAR;
                parser = new(VAR);
                string[] simplified = newexpression.Simplify().ToString().Split(' ');
                expr = parser.NewExprToList(newexpression.Simplify().ToString());
                expr = parser.ToPrefix(expr);
            }
            public override string ToString()
            {
                Entity sresult = expr_string;
                return sresult.Simplify().ToString();
            }
            public void Differentiate()
            {
                tree = parser.PrefixToTree(expr);
                tree.Differentiate();
                expr = parser.TreeToPrefix(tree);
                expr_string = parser.PrefixToInfix(expr);
                parser.ResetPos();
                Console.WriteLine(expr_string);
            }
        }
        class Parser
        {
            static readonly HashSet<string> functions = new HashSet<string> { "sin", "cos", "tg", "cotg", "abs", "sqrt", "ln" };
            static readonly HashSet<string> operators = new HashSet<string> { "+", "-", "*", "/", "^" };
            string VAR;
            int pos;

            public Parser(string vAR)
            {
                VAR = vAR;
            }

            public void ResetPos() //IMPORTANT for further differentiations!!!
            {
                pos = 0;
            }
            public string Prepare(string expr)
            {
                return "NIC";
            }
            public List<string> ExprToList(string[] simplified) // Adds the expression as a List in expr
            {
                List<string> result = new();
                for (int i = 0; i < simplified.Length; i++)
                {
                    string current = simplified[i];
                    if (current.Length > 1)
                    {
                        if (functions.Contains(current))
                        {
                            result.Add(current);
                        }
                        else if (current[^1] == ')')
                        {
                            result.Add(current[..^1]);
                            result.Add(")");
                        }
                        else if (Char.IsDigit(current[0]))
                        {
                            result.Add(current);
                        }
                        else if (current[0] == '(' && current[^1] == ')')
                        {
                            result.Add("(");
                            result.Add((string)current[1..^1]);
                            result.Add(")");
                        }
                        else if (current[0] == '(')
                        {
                            result.Add("(");
                            result.Add(current[1..]);
                        }
                        else
                        {
                            Console.WriteLine("PARSER ERROR: Unknown function");
                        }
                    }
                    else if (operators.Contains(current) || Char.IsDigit(current[0]) || Char.IsLetter(current[0]))
                    {
                        result.Add(current);
                    }
                }
                return result;
            }
            public List<string> NewExprToList(string simplified)
            {
                List<string> result = new();
                int i = 0;
                while (i < simplified.Length)
                {
                    string str_current = simplified[i].ToString();
                    if (simplified[i] == ' ')
                    {
                        i++;
                        continue;
                    }
                    if (char.IsDigit(simplified[i]))
                    {
                        string numb = "";
                        do
                        {
                            numb += simplified[i];
                            i++;

                        } while (i < simplified.Length && char.IsDigit(simplified[i]));

                        result.Add(numb);
                        continue;
                    }
                    else if (operators.Contains(str_current))
                    {
                        result.Add(str_current);
                        i++;
                        continue;
                    }
                    else if (str_current == VAR)
                    {
                        result.Add(str_current);
                        i++;
                        continue;
                    }
                    else if (simplified[i] == ')' || simplified[i] == '(')
                    {
                        result.Add(str_current);
                        i++;
                        continue;
                    }
                    else
                    {
                        string func = "";
                        do
                        {
                            func += simplified[i];
                            i++;

                        } while (i < simplified.Length && simplified[i] != '(');
                        
                        if (functions.Contains(func))
                        {
                            result.Add(func);
                            i++;
                            continue;
                        }
                        else
                        {
                            throw new Exception("PARSER ERROR: UNKNOWN FUNCTION: " + func);
                        }
                    }
                }
                return result;
            }
            public List<string> ToPrefix(List<string> expr)
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
                    else if (functions.Contains(current))
                    {
                        while (operatorstack.Peek() != ")")
                        {
                            prefix.Add(operatorstack.Pop());
                        }
                        operatorstack.Pop();
                        prefix.Add(current);
                    }
                    else if (current == ")")
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
                return prefix;
            }
            public Head PrefixToTree(List<string> expr)
            {
                Head node = new();
                node.Add(BuildTree(expr));
                return node;
            }
            public List<string> TreeToPrefix(Head tree)
            {
                Stack<INode> stack = new();
                List<string> result = new();

                stack.Push(tree.leftchild);

                while (stack.Count > 0)
                {
                    INode node = stack.Pop();
                    if (node is Constant constant)
                    {
                        double val = constant.Value;
                        result.Add(val.ToString(CultureInfo.InvariantCulture));
                    }
                    else if (node is DiffVariable)
                    {
                        result.Add(VAR);
                    }
                    else if (node is OPNode opnode)
                    {
                        if (node is Plus)
                        {
                            result.Add("+");
                            stack.Push(opnode.rightchild);
                            stack.Push(opnode.leftchild);
                        }
                        else if (node is Minus)
                        {
                            result.Add("-");
                            stack.Push(opnode.rightchild);
                            stack.Push(opnode.leftchild);
                        }
                        else if (node is Multi)
                        {
                            result.Add("*");
                            stack.Push(opnode.rightchild);
                            stack.Push(opnode.leftchild);
                        }
                        else if (node is Divi)
                        {
                            result.Add("/");
                            stack.Push(opnode.rightchild);
                            stack.Push(opnode.leftchild);
                        }
                        else if (node is Power)
                        {
                            result.Add("^");
                            stack.Push(opnode.rightchild);
                            stack.Push(opnode.leftchild);
                        }
                    }
                }
                return result;
            }
            public string PrefixToInfix(List<string> expr)
            {
                Stack<string> stack = new();

                for (int i = expr.Count - 1; i >= 0; i--)
                {
                    string curr = expr[i];
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
                return stack.Pop();
            }

            INode BuildTree(List<string> expr)
            {
                INode node = null;

                if (!operators.Contains(expr[pos]) && expr[pos] != VAR && !functions.Contains(expr[pos])) //It is a number
                {
                    double val = StringToDouble(expr[pos]);
                    Constant constant = new(val);
                    node = constant;
                }
                else if (expr[pos] == VAR) //It is a derivative variable
                {
                    DiffVariable variable = new();
                    node = variable;
                }
                else //It is an operator or a function
                {
                    switch (expr[pos])
                    {
                        case "+":
                            Plus plus = new();
                            pos++;
                            plus.Add(BuildTree(expr));

                            pos++;
                            plus.Add(BuildTree(expr));
                            node = plus;
                            break;

                        case "-":
                            Minus minus = new();
                            pos++;
                            minus.Add(BuildTree(expr));

                            pos++;
                            minus.Add(BuildTree(expr));
                            node = minus;
                            break;

                        case "*":
                            Multi multi = new();
                            pos++;
                            multi.Add(BuildTree(expr));

                            pos++;
                            multi.Add(BuildTree(expr));
                            node = multi;
                            break;

                        case "/":
                            Divi divi = new();
                            pos++;
                            divi.Add(BuildTree(expr));

                            pos++;
                            divi.Add(BuildTree(expr));
                            node = divi;
                            break;

                        case "^":
                            Power power = new();
                            pos++;
                            power.Add(BuildTree(expr));

                            pos++;
                            power.Add(BuildTree(expr));
                            node = power;
                            break;
                    }
                }
                return node;
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
                        if (operators.Contains(oper))
                            return 2;
                        return 0;
                }
            }
        }
    }
}
