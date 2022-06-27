using System;

namespace ExprTree
{
    interface INode
    {
        void Differentiate();
        void SelfCheck();
        void Add(INode node);
        void Remove(INode node);
        void SetParent(OPNode node);
        //INode Clone();
    }
    abstract class OPNode : INode
    {
        public INode leftchild, rightchild;
        public OPNode parent = null;

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
        public virtual void Add(INode node)
        {
            if (leftchild == null)
            {
                leftchild = node;
                leftchild.SetParent(this);
            }
            else if (rightchild == null)
            {
                rightchild = node;
                rightchild.SetParent(this);
            }
        }
        public virtual void Remove(INode node)
        {
            if (leftchild == node)
            {
                leftchild.SetParent(null);
                leftchild = null;
            }
            else if (rightchild == node)
            {
                rightchild.SetParent(null);
                rightchild = null;
            }
        }
        public void SetChildren(INode first, INode second)
        {
            Add(first);
            Add(second);
        }
        abstract public void Differentiate();
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

        public void Add(INode node)
        {
            return;
        }

        public void Remove(INode node)
        {
            return;
        }
    }
    sealed class DiffVariable : ConstNode
    {
        public DiffVariable(OPNode node)
        {
            if (node == null)
                return;
            parent.Add(this);
        }
        public override void SelfCheck()
        {
            return;
        }

        public override void Differentiate()
        {
            Constant one = new(1, GetParent());
            OPNode oldparent = GetParent();

            GetParent().Remove(this);
            oldparent.Add(one);
        }

    }
    sealed class Constant : ConstNode
    {
        double value;
        public double Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public Constant(double value, OPNode node)
        {
            this.value = value;
            if (node == null)
                return;
            SetParent(parent);
        }

        public override void SelfCheck()
        {
            return;
        }
        public override void Differentiate()
        {
            Value = 0;
        }
    }
    class Head : OPNode
    {
        public Head()
        {

        }

        public override void Differentiate()
        {
            leftchild.Differentiate();
        }

        public override void SelfCheck()
        {
            leftchild.SelfCheck();
        }

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
    }
    sealed class Plus : OPNode
    {
        public Plus(OPNode parent)
        {
            SetParent(parent);
        }

        public override void SelfCheck()
        {
            if (leftchild is Constant left && rightchild is Constant right) //All constants
            {
                Constant sum = new(left.Value + right.Value, null);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(sum);
            }
            else if (leftchild is Constant left1 && left1.Value == 0) //Left is 0
            {
                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(rightchild);
            }
            else if (rightchild is Constant right1 && right1.Value == 0) //Right is 0
            {
                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(leftchild);
            }
        }

        public override void Differentiate()
        {
            leftchild.Differentiate();
            rightchild.Differentiate();
            SelfCheck();
        }
    }
    sealed class Minus : OPNode
    {
        public Minus(OPNode parent)
        {
            SetParent(parent);
        }

        public override void SelfCheck()
        {
            if (leftchild is Constant left && rightchild is Constant right) //All constants
            {
                Constant sum = new(left.Value - right.Value, null);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(sum);
            }
            else if (leftchild is Constant left1 && left1.Value == 0) //Left is 0, so it multiplies by -1
            {
                Multi multi = new(null);
                Constant constant = new(-1, multi);
                multi.Add(rightchild);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(multi);
            }
            else if (rightchild is Constant right1 && right1.Value == 0) //Right is 0
            {
                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(leftchild);
            }
        }

        public override void Differentiate()
        {
            leftchild.Differentiate();
            rightchild.Differentiate();
            SelfCheck();
        }
    }
    sealed class Multi : OPNode
    {
        public Multi(OPNode parent)
        {
            SetParent(parent);
        }
        public override void SelfCheck()
        {
            if (leftchild is Constant left && rightchild is Constant right) //All constants
            {
                double prod = left.Value * right.Value;
                Constant product = new(prod, null);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(product);
            }
            else if (leftchild is Constant left1 && left1.Value == 0) //Left is 0
            {
                Constant constant = new(0, null);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(constant);
            }
            else if (rightchild is Constant right1 && right1.Value == 0) //Right is 0
            {
                Constant constant = new(0, null);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(constant);
            }
            else if (leftchild is Constant left2 && left2.Value == 1) //Left is 1
            {
                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(rightchild);
            }
            else if (rightchild is Constant right2 && right2.Value == 1) //Right is 1
            {
                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(leftchild);
            }
            //!!! ONLY for 1, because -1 wouldn't make a difference
        }
        public void MultiplyBy(double value)
        {
            if (leftchild is Constant constant)
            {
                constant.Value *= value;
            }
        }
        public override void Differentiate()
        {
            Plus plus = new(GetParent());
            Multi first = new(plus);
            Multi second = new(plus);

            INode left1 = leftchild;
            INode right1 = rightchild;

            first.SetChildren(left1, right1);
            first.leftchild.Differentiate();
            first.SelfCheck();

            second.SetChildren(leftchild, rightchild);
            second.rightchild.Differentiate();
            second.SelfCheck();

            plus.SetChildren(first, second);

            OPNode oldparent = GetParent();
            GetParent().Remove(this);
            oldparent.Add(plus);
            plus.SelfCheck();
        }
    }
    sealed class Divi : OPNode
    {
        public Divi(OPNode parent)
        {
            SetParent(parent);
        }
        public override void SelfCheck()
        {
            if (leftchild is Constant left && rightchild is Constant right) //All constants
            {
                double prod = left.Value / right.Value;
                Constant product = new(prod, null);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(product);
            }
            else if (leftchild is Constant left1 && left1.Value == 0) //Left is 0
            {
                Constant constant = new(0, null);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(constant);
            }
            else if (rightchild is Constant right1 && right1.Value == 0) //Right is 0
            {
                Console.WriteLine("ERROR: Division by zero!");
                //TODO implement some handler that will halt after any error
            }
            else if (rightchild is Constant right2 && right2.Value == 1) //Right is 1
            {
                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(leftchild);
            }
            else if (rightchild is Constant right3 && right3.Value == -1) //Right is -1
            {
                Multi multi = new(null);
                Constant constant = new(-1, multi);
                multi.Add(leftchild);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(multi);
            }
            //!!! ONLY for 1, because -1 wouldn't make a difference
        }

        public override void Differentiate()
        {
            Divi newdivi = new(null);

            Minus minus = new(newdivi);
            Power power = new(newdivi);

            Multi left = new(minus);
            Multi right = new(minus);

            INode rightpower = rightchild;
            INode left1 = leftchild;
            INode right1 = rightchild;

            left.SetChildren(left1, right1); //F'G
            left.leftchild.Differentiate();
            left.SelfCheck();

            right.SetChildren(leftchild, rightchild); //FG'
            right.rightchild.Differentiate();
            right.SelfCheck();

            power.Add(rightpower); //G^2
            Constant two = new(2, power);
            power.SelfCheck();

            minus.SelfCheck();
            newdivi.SelfCheck();

            OPNode oldparent = GetParent();
            oldparent.Remove(this);
            oldparent.Add(newdivi);
        }
    }
    sealed class Power : OPNode
    {
        public Power(OPNode parent)
        {
            SetParent(parent);
        }
        public override void SelfCheck()
        {
            if (leftchild is Constant left && rightchild is Constant right) //All constants
            {
                double prod = Math.Pow(left.Value, right.Value);
                Constant product = new(prod, null);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(product);
            }
            else if (leftchild is Constant left1 && left1.Value == 0) //Left is 0
            {
                Constant constant = new(0, null);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(constant);
            }
            else if (rightchild is Constant right1 && right1.Value == 0) //Right is 0
            {
                Constant constant = new(1, null);

                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(constant);
            }
            else if (rightchild is Constant right2 && right2.Value == 1) //Right is 1
            {
                INode oldparent = GetParent();
                GetParent().Remove(this);
                oldparent.Add(leftchild);
            }
            //!!! ONLY for 1, because -1 wouldn't make a difference
        }

        public override void Differentiate()
        {
            if (leftchild is DiffVariable x && rightchild is Constant m) //x^m
            {
                Multi multi = new(null);
                Constant newconstant = new(m.Value, multi);

                multi.Add(x);
                this.Remove(x);
                this.Add(multi);

                m.Value -= 1;
                
                multi.SelfCheck();
                SelfCheck();
            }
            else if (leftchild is Constant a && rightchild is DiffVariable x2) //a^x
            {
                //TODO Implement logarithm to finish this
            }
            else if (leftchild is DiffVariable x3 && rightchild is DiffVariable x4) //TODO x^x -> e^(x*ln(x))
            {

            }
            else //TODO (maybe) Implement derivation of two constants = 0
            {

            }
        }
    }
}
