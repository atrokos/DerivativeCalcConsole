using System;

namespace ExprTree
{
    static class Debugger
    {
        public static void LightProblem(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warning: {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void SevereProblem(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Warning: {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
    interface IParent
    {
        /// <summary>
        /// For all nodes that can be parents.
        /// </summary>
        void Add(INode node);
        void Remove(INode node);
        void SwapChildren(INode newchild); // child -> P  newchild   ==> child  P <- newchild
    }
    interface INode
    {
        void SetParent(OPNode node);
        void SelfCheck();
        void Differentiate();
        INode Clone();
    }

    abstract class OPNode : INode, IParent
    {
        public INode leftchild, rightchild;
        private OPNode parent = null;

        abstract public void SelfCheck();
        abstract public void Differentiate();
        abstract public INode Clone();

        public OPNode GetParent()
        {
            return parent;
        }
        public void SetParent(OPNode node)
        {
            if (node == null)
                return;
            parent = node;
        }
        public void SwapChildren(INode newchild)
        {
            IParent oldparent = GetParent();
            oldparent.Remove(this);
            oldparent.Add(newchild);
        }
        public virtual void Add(INode node)
        {
            if (leftchild == null)
            {
                leftchild = node;
                leftchild.SetParent(this);
                return;
            }
            else if (rightchild == null)
            {
                rightchild = node;
                rightchild.SetParent(this);
                return;
            }
            Debugger.SevereProblem($"{this} was requested to add {node}, but its children are full.");
        }
        public virtual void Remove(INode node)
        {
            if (leftchild == node)
            {
                leftchild.SetParent(null);
                leftchild = null;
                return;
            }
            else if (rightchild == node)
            {
                rightchild.SetParent(null);
                rightchild = null;
                return;
            }
            Debugger.LightProblem($"Could not remove {node.GetHashCode()}, {this}'s ({this.GetHashCode()}) children are:\n\t\t{leftchild.GetHashCode()}, {rightchild.GetHashCode()}");
            
        }
        public virtual void SetChildren(INode first, INode second)
        {
            Add(first);
            Add(second);
        }
    }
    class Head : OPNode
    {
        public override void Add(INode node)
        {
            if (leftchild == null)
            {
                leftchild = node;
                node.SetParent(this);
            }
        }
        public override void Remove(INode node)
        {
            if (leftchild == node)
            {
                node.SetParent(null);
                leftchild = null;
            }
        }

        public override void SelfCheck()
        {
            leftchild.SelfCheck();
        }
        public override void Differentiate()
        {
            leftchild.Differentiate();
        }
        public override INode Clone()
        {
            /// <summary> This method returns a clone of the called node with a null parent. </summary>
            Head head = new();
            head.Add(leftchild.Clone());
            return head;
        }
    }
    abstract class ConstNode : INode
    {
        public OPNode parent;

        public OPNode GetParent()
        {
            return parent;
        }
        public void SetParent(OPNode node)
        {
            if (node == null)
                return;
            parent = node;
        }
        abstract public void SelfCheck();
        abstract public void Differentiate();
        abstract public INode Clone();
    }
    sealed class DiffVariable : ConstNode
    {
        public override void SelfCheck()
        {
            return;
        }
        public override INode Clone()
        {
            DiffVariable variable = new();
            return variable;
        }
        public override void Differentiate()
        {
            Constant one = new(1);
            OPNode oldparent = GetParent();

            GetParent().Remove(this);
            oldparent.Add(one);
        }

    }
    sealed class Constant : ConstNode
    {
        private double value;
        public double Value
        {
            get { return value; }
            set { this.value = value; }
        }
        public Constant(double value)
        {
            Value = value;
        }
        public override void SelfCheck()
        {
            return;
        }
        public override void Differentiate()
        {
            Value = 0;
        }
        public override INode Clone()
        {
            Constant constant = new(Value);
            return constant;
        }
    }
    sealed class Plus : OPNode
    {
        public override void SelfCheck()
        {
            if (leftchild is Constant left && rightchild is Constant right) //All constants
            {
                Constant sum = new(left.Value + right.Value);

                SwapChildren(sum);
            }
            else if (leftchild is Constant left1 && left1.Value == 0) //Left is 0
            {
                SwapChildren(rightchild);
            }
            else if (rightchild is Constant right1 && right1.Value == 0) //Right is 0
            {
                SwapChildren(leftchild);
            }
        }
        public override void Differentiate()
        {
            leftchild.Differentiate();
            leftchild.SelfCheck();
            rightchild.Differentiate();
            rightchild.SelfCheck();
        }
        public override INode Clone()
        {
            /// <summary> This method returns a clone of the called node with a null parent. </summary>
            Plus plus = new();
            plus.SetChildren(leftchild.Clone(), rightchild.Clone());
            return plus;
        }
    }
    sealed class Minus : OPNode
    {
        public override void SelfCheck()
        {
            if (leftchild is Constant left && rightchild is Constant right) //All constants
            {
                Constant sum = new(left.Value - right.Value);

                SwapChildren(sum);
            }
            else if (leftchild is Constant left1 && left1.Value == 0) //Left is 0, so it multiplies by -1
            {
                Multi multi = new();
                Constant constant = new(-1);
                multi.SetChildren(constant, rightchild);

                SwapChildren(multi);
            }
            else if (rightchild is Constant right1 && right1.Value == 0) //Right is 0
            {
                SwapChildren(leftchild);
            }
        }
        public override void Differentiate()
        {
            leftchild.Differentiate();
            leftchild.SelfCheck();
            rightchild.Differentiate();
            rightchild.SelfCheck();
        }
        public override INode Clone()
        {
            /// <summary> This method returns a clone of the called node with a null parent. </summary>
            Minus minus = new();
            minus.SetChildren(leftchild.Clone(), rightchild.Clone());
            return minus;
        }
    }
    sealed class Multi : OPNode
    {
        public void MultiplyBy(double value)
        {
            if (leftchild is Constant constant)
            {
                constant.Value *= value;
            }
        }

        public override void SelfCheck()
        {
            if (leftchild is Constant left && rightchild is Constant right) //All constants
            {
                double prod = left.Value * right.Value;
                Constant product = new(prod);

                SwapChildren(product);
            }
            else if (leftchild is Constant left1 && left1.Value == 0) //Left is 0
            {
                Constant constant = new(0);

                SwapChildren(constant);
            }
            else if (rightchild is Constant right1 && right1.Value == 0) //Right is 0
            {
                Constant constant = new(0);

                SwapChildren(constant);
            }
            else if (leftchild is Constant left2 && left2.Value == 1) //Left is 1
            {
                SwapChildren(rightchild);
            }
            else if (rightchild is Constant right2 && right2.Value == 1) //Right is 1
            {
                SwapChildren(leftchild);
            }
            //!!! ONLY for 1, because -1 wouldn't make a difference
        }
        public override void Differentiate()
        {
            Plus plus = new();
            Multi first = new();
            Multi second = new();
            plus.SetChildren(first, second);

            INode left1 = leftchild.Clone();
            INode right1 = rightchild.Clone();

            first.SetChildren(left1, right1);
            first.leftchild.Differentiate();
            first.SelfCheck();

            second.SetChildren(leftchild, rightchild);
            second.rightchild.Differentiate();
            second.SelfCheck();

            SwapChildren(plus);
            plus.SelfCheck();
        }
        public override INode Clone()
        {
            /// <summary> This method returns a clone of the called node with a null parent. </summary>
            Multi multi = new();
            multi.SetChildren(leftchild.Clone(), rightchild.Clone());
            return multi;
        }
    }
    sealed class Divi : OPNode
    {
        public override void SelfCheck()
        {
            if (leftchild is Constant left1 && left1.Value == 0) //Left is 0
            {
                Constant constant = new(0);

                SwapChildren(constant);
            }
            else if (rightchild is Constant right1 && right1.Value == 0) //Right is 0
            {
                Console.WriteLine("ERROR: Division by zero!");
                //TODO implement some handler that will halt after any error
            }
            else if (rightchild is Constant right2 && right2.Value == 1) //Right is 1
            {
                SwapChildren(leftchild);
            }
            else if (rightchild is Constant right3 && right3.Value == -1) //Right is -1
            {
                Multi multi = new();
                Constant constant = new(-1);
                multi.Add(leftchild);

                SwapChildren(multi);
            }
            //!!! ONLY for 1, because -1 wouldn't make a difference
        }
        public override void Differentiate()
        {
            Divi newdivi = new();

            Minus minus = new();
            Power power = new();
            newdivi.SetChildren(minus, power);

            INode PowerG = rightchild.Clone();
            Constant constant = new(2);
            power.SetChildren(PowerG, constant);

            Multi left = new();
            Multi right = new();
            minus.SetChildren(left, right);

            INode leftL = leftchild.Clone();
            INode leftR = rightchild.Clone();
            left.SetChildren(leftL, leftR);
            leftL.Differentiate();
            leftL.SelfCheck();

            INode rightL = leftchild.Clone();
            INode rightR = rightchild.Clone();
            right.SetChildren(rightL, rightR);
            rightR.Differentiate();
            rightR.SelfCheck();


            left.SelfCheck();
            right.SelfCheck();
            minus.SelfCheck();

            SwapChildren(newdivi);
        }
        public override INode Clone()
        {
            /// <summary> This method returns a clone of the called node with a null parent. </summary>
            Divi divi = new();
            divi.SetChildren(leftchild.Clone(), rightchild.Clone());
            return divi;
        }
    }
    sealed class Power : OPNode
    {
        public override void SelfCheck()
        {
            if (leftchild is Constant left && rightchild is Constant right) //All constants
            {
                double prod = Math.Pow(left.Value, right.Value);
                Constant product = new(prod);

                SwapChildren(product);
            }
            else if (leftchild is Constant left1 && left1.Value == 0) //Left is 0
            {
                Constant constant = new(0);

                SwapChildren(constant);
            }
            else if (rightchild is Constant right1 && right1.Value == 0) //Right is 0
            {
                Constant constant = new(1);

                SwapChildren(constant);
            }
            else if (rightchild is Constant right2 && right2.Value == 1) //Right is 1
            {
                SwapChildren(leftchild);
            }
            else if (leftchild is Constant e && rightchild is Log log)
            {
                if (e.Value == Math.E)
                {
                    SwapChildren(log.leftchild.Clone());
                }
                
            }
            //!!! ONLY for 1, because -1 wouldn't make a difference
        }
        public override void Differentiate()
        {
            Multi multi1 = new();
            Multi multi2 = new();

            INode power = this.Clone();
            multi1.SetChildren(power, multi2);
            power.SelfCheck();

            Log log = new();
            log.Add(leftchild.Clone());
            INode right = rightchild.Clone();
            multi2.SetChildren(log, right);

            multi2.Differentiate();

            SwapChildren(multi1);
            multi1.SelfCheck();
        }
        public override INode Clone()
        {
            /// <summary> This method returns a clone of the called node with a null parent. </summary>
            Power power = new();
            power.SetChildren(leftchild.Clone(), rightchild.Clone());
            return power;
        }
    }

    abstract class Function : OPNode
    {
        public override void SelfCheck()
        {
            return;
        }
        public override void Add(INode node)
        {
            if (leftchild == null)
            {
                leftchild = node;
                leftchild.SetParent(this);
            }
        }
        public override void Remove(INode node)
        {
            if (leftchild == node)
            {
                leftchild.SetParent(null);
                leftchild = null;
            }
        }
    }
    class Sin : Function
    {
        public override void Differentiate()
        {
            Multi multi = new();
            Cos cos = new();

            INode left = leftchild.Clone();
            cos.Add(leftchild.Clone());
            multi.SetChildren(left, cos);
            left.Differentiate();
            left.SelfCheck();


            SwapChildren(multi);
        }
        public override INode Clone()
        {
            Sin sin = new();
            sin.Add(leftchild.Clone());
            return sin;
        }
    }
    class Cos : Function
    {
        public override void Differentiate()
        {
            Multi multi1 = new();
            Multi multi2 = new();

            Constant constant = new(-1);
            multi1.SetChildren(constant, multi2);

            Sin sin = new();
            INode left = leftchild.Clone();
            sin.Add(leftchild.Clone());
            multi2.SetChildren(left, sin);
            left.Differentiate();
            left.SelfCheck();

            SwapChildren(multi1);
        }
        public override INode Clone()
        {
            Cos cos = new();
            cos.Add(leftchild.Clone());
            return cos;
        }
    }
    class Tg : Function
    {
        public override void Differentiate()
        {
            Multi multi = new();
            Divi divi = new();
            multi.SetChildren(divi, leftchild.Clone());
            multi.rightchild.Differentiate();

            Constant one = new(1);
            Constant two = new(2);
            Power power = new();
            Cos cos = new();
            cos.Add(leftchild.Clone());

            divi.SetChildren(one, power);
            power.SetChildren(cos, two);

            SwapChildren(multi);
        }
        public override INode Clone()
        {
            Tg tg = new();
            tg.Add(leftchild.Clone());

            return tg;
        }
    }
    class Cotg : Function
    {
        public override void Differentiate()
        {
            Multi multi1 = new();
            Constant minus = new(-1);
            Multi multi2 = new();

            multi1.SetChildren(minus, multi2);
            Divi divi = new();
            multi2.SetChildren(divi, leftchild.Clone());
            multi2.rightchild.Differentiate();

            Constant one = new(1);
            Constant two = new(2);
            Power power = new();
            Sin sin = new();
            sin.Add(leftchild.Clone());

            divi.SetChildren(one, power);
            power.SetChildren(sin, two);

            SwapChildren(multi1);
        }
        public override INode Clone()
        {
            Cotg cotg = new();
            cotg.Add(leftchild.Clone());

            return cotg;
        }
    }
    class Arcsin : Function
    {
        public override void Differentiate()
        {
            Multi multi = new();
            Divi divi1 = new();
            multi.SetChildren(divi1, leftchild.Clone());
            multi.rightchild.Differentiate();

            Constant one = new(1);
            Power power1 = new();

            divi1.SetChildren(one.Clone(), power1);

            Minus minus = new();
            Constant two = new(2);
            Power power2 = new();
            minus.SetChildren(one.Clone(), power2);
            power2.SetChildren(leftchild.Clone(), two.Clone());

            Divi divi2 = new();
            power1.SetChildren(minus, divi2);
            divi2.SetChildren(one.Clone(), two.Clone());

            SwapChildren(multi);
        }
        public override INode Clone()
        {
            Arcsin arcsin = new();
            arcsin.Add(leftchild.Clone());
            return arcsin;
        }
    }
    class Arccos : Function
    {
        public override void Differentiate()
        {
            Multi multi1 = new();
            Constant minusone = new(-1);
            Multi multi = new();
            Divi divi1 = new();
            multi1.SetChildren(minusone, multi);
            multi.SetChildren(divi1, leftchild.Clone());
            multi.rightchild.Differentiate();

            Constant one = new(1);
            Power power1 = new();

            divi1.SetChildren(one.Clone(), power1);

            Minus minus = new();
            Constant two = new(2);
            Power power2 = new();
            minus.SetChildren(one.Clone(), power2);
            power2.SetChildren(leftchild.Clone(), two.Clone());

            Divi divi2 = new();
            power1.SetChildren(minus, divi2);
            divi2.SetChildren(one.Clone(), two.Clone());

            SwapChildren(multi1);
        }
        public override INode Clone()
        {
            Arccos arccos = new();
            arccos.Add(leftchild.Clone());
            return arccos;
        }
    }
    class Arctg : Function
    {
        public override void Differentiate()
        {
            Multi multi = new();
            Divi divi = new();
            multi.SetChildren(divi, leftchild.Clone());
            multi.rightchild.Differentiate();

            Constant one = new(1);
            Constant two = new(2);
            Plus plus = new();

            divi.SetChildren(one.Clone(), plus);

            Power power = new();
            plus.SetChildren(one.Clone(), power);
            power.SetChildren(leftchild.Clone(), two);


            SwapChildren(multi);
        }
        public override INode Clone()
        {
            Arctg arctg = new();
            arctg.Add(leftchild.Clone());
            return arctg;
        }
    }
    class Arccotg : Function
    {
        public override void Differentiate()
        {
            Multi multi1 = new();
            Constant minusone = new(-1);
            Multi multi = new();
            multi1.SetChildren(minusone, multi);
            Divi divi = new();
            multi.SetChildren(divi, leftchild.Clone());
            multi.rightchild.Differentiate();

            Constant one = new(1);
            Constant two = new(2);
            Plus plus = new();

            divi.SetChildren(one.Clone(), plus);

            Power power = new();
            plus.SetChildren(one.Clone(), power);
            power.SetChildren(leftchild.Clone(), two);


            SwapChildren(multi1);
        }
        public override INode Clone()
        {
            Arccotg arccotg = new();
            arccotg.Add(leftchild.Clone());
            return arccotg;
        }
    }
    class Log : Function // Only the natural log
    {
        public override void SelfCheck()
        {
            if (leftchild is Constant e && e.Value == Math.E)
            {
                Constant constant = new(1);

                SwapChildren(constant);
            }
        }
        public override void Differentiate()
        {
            Multi multi = new();
            Divi divi = new();
            Constant Dleft = new(1);
            INode Dright = leftchild.Clone();
            divi.SetChildren(Dleft, Dright);

            INode Lright = leftchild.Clone();
            multi.SetChildren(divi, Lright);
            Lright.Differentiate();

            SwapChildren(multi);
        }
        public override INode Clone()
        {
            Log newlog = new();
            newlog.Add(leftchild.Clone());
            return newlog;
        }
    }
    class Abs : Function
    {
        public override void Differentiate()
        {
            Divi divi = new();
            Abs abs = new();
            abs.Add(leftchild.Clone());
            divi.SetChildren(leftchild.Clone(), abs);

            SwapChildren(divi);
        }
        public override INode Clone()
        {
            Abs abs = new();
            abs.Add(leftchild.Clone());

            return abs;
        }
    }
}
