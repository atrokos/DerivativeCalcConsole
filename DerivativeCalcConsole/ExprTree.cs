using System;
using System.Collections.Generic;
using System.Resources;


namespace ExprTree
{
    interface INode
    {
        public void Differentiate();
        public void SelfCheck();
        public void Add(INode node);
        public void Remove(INode node);
    }
    abstract class OPNode : INode
    {
        public INode leftchild, rightchild;
        public OPNode parent = null;

        public OPNode Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        abstract public void SelfCheck();

        public virtual void Add(INode node)
        {
            //TODO Implement Add so that it handles ALL add operations
        }

        public void SetChildren(INode first, INode second)
        {
            leftchild = first;
            rightchild = second;
        }

        public virtual void Remove(INode node)
        {
            //TODO Implement Remove so that it handles ALL remove operations
        }
        abstract public void Differentiate();
    }
    abstract class ConstNode : INode
    {
        public OPNode parent;

        public OPNode Parent
        {
            get { return parent; }
            set { parent = value; }
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
        public DiffVariable(OPNode parent)
        {
            parent.Add(this);
        }
        public override void SelfCheck()
        {
            return;
        }

        public override void Differentiate()
        {
            Constant one = new(1, Parent);

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

        public Constant(double value, OPNode parent)
        {
            this.value = value;
            Parent = parent;
        }

        public override void SelfCheck()
        {
            return;
        }
        public override void Differentiate()
        {
            return;
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
                node.ChangeParent(this);
            }
        }

        public override void Remove(INode node)
        {
            if (leftchild == node)
            {
                node.ChangeParent(null);
                leftchild = null;
            }
        }
    }
    sealed class Plus : OPNode
    {
        public Plus(OPNode parent)
        {
            Parent = parent;
        }

        public override void SelfCheck()
        {
            if (leftchild is Constant && rightchild is Constant)
            {
                var first = (Constant)leftchild;
                var second = (Constant)rightchild;

                first.Value += second.Value;
                Parent.Remove(this);
                first.ChangeParent(Parent);
            }
            if (leftchild is Constant)
            {
                var first = (Constant)leftchild;

                if (first.Value == 0)
                {
                    rightchild.ChangeParent(Parent);
                    Parent.Remove(this);
                }
            }
            else if (rightchild is Constant)
            {
                var second = (Constant)rightchild;

                if (second.Value == 0)
                {
                    leftchild.ChangeParent(Parent);
                    Parent.Remove(this);
                }
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
            Parent = parent;
        }

        public override void SelfCheck()
        {
            if (leftchild is Constant && rightchild is Constant)
            {
                var first = (Constant)leftchild;
                var second = (Constant)rightchild;

                first.Value -= second.Value;

                OPNode oldparent = Parent;
                Parent.Remove(this);
                first.ChangeParent(oldparent);
            }
            if (leftchild is Constant)
            {
                var first = (Constant)leftchild;

                if (first.Value == 0)
                {
                    OPNode oldparent = Parent;
                    Parent.Remove(this);
                    oldparent.Add(rightchild);
                }
            }
            else if (rightchild is Constant)
            {
                var second = (Constant)rightchild;

                if (second.Value == 0)
                {
                    OPNode oldparent = Parent;
                    Parent.Remove(this);
                    oldparent.Add(leftchild);
                }
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
            Parent = parent;
        }
        public override void SelfCheck()
        {
                
        }

        public override void Differentiate()
        {
            Plus plus = new(Parent);
            Multi first = new(plus);
            Multi second = new(plus);

            first.SetChildren(leftchild, rightchild);
            first.leftchild.Differentiate();
            first.SelfCheck();

            second.SetChildren(leftchild, rightchild);
            second.rightchild.Differentiate();
            second.SelfCheck();

            plus.SetChildren(first, second);

            OPNode oldparent = Parent;
            Parent.Remove(this);
            oldparent.Add(plus);
            plus.SelfCheck();
        }
    }
    sealed class Divi : OPNode
    {
        public Divi(OPNode parent)
        {
            Parent = parent;
        }
        public override void SelfCheck()
        {
            if (leftchild is Constant && rightchild is Constant)
            {
                var first = (Constant)leftchild;
                var second = (Constant)rightchild;
                if (second.Value == 0)
                {
                    Console.WriteLine("Division by zero!");
                    return;
                }
                first.Value /= second.Value;

                OPNode oldparent = Parent;
                Parent.Remove(this);
                oldparent.Add(first);
            }
            if (leftchild is Constant)
            {
                var first = (Constant)leftchild;

                if (first.Value == 0)
                {
                    Parent.Remove(this);
                }
            }
            else if (rightchild is Constant)
            {
                var second = (Constant)rightchild;

                if (second.Value == 0)
                {
                    Console.WriteLine("Division by zero!");
                    return;
                }
                else if (second.Value == 1)
                {
                    OPNode oldparent = Parent;
                    Parent.Remove(this);
                    oldparent.Add(leftchild);
                }
                else if (second.Value == -1)
                {

                }
            }
        }

        public override void Differentiate()
        {

        }
    }
    sealed class Power : OPNode
    {
        public Power(OPNode parent)
        {
            Parent = parent;
        }
        public override void SelfCheck()
        {
            if (leftchild is Constant && rightchild is Constant)
            {
                var first = (Constant)leftchild;
                var second = (Constant)rightchild;
                if (second.Value == 0)
                {
                    Console.WriteLine("Division by zero!");
                    return;
                }
                first.Value /= second.Value;

                OPNode oldparent = Parent;
                Parent.Remove(this);
                oldparent.Add(first);
            }
            if (leftchild is Constant)
            {
                var first = (Constant)leftchild;

                if (first.Value == 0)
                {
                    rightchild.ChangeParent(Parent);
                    Parent.Remove(this);
                }
            }
            else if (rightchild is Constant)
            {
                var second = (Constant)rightchild;

                if (second.Value == 0)
                {
                    leftchild.ChangeParent(Parent);
                    Parent.Remove(this);
                }
            }
        }

        public override void Differentiate()
        {

        }
    }
    
}
