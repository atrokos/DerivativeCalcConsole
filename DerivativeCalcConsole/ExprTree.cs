using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTree
{
    internal class ExpressionTree
    {
        interface INode
        {
            public void Differentiate();
            public void SelfCheck();
            public void ChangeParent(OPNode newParent, INode oldchild);
        }
        abstract class OPNode : INode
        {
            public INode[] children = new INode[2];
            public OPNode parent = null;

            public OPNode Parent
            {
                get { return parent; }
            }

            public void ChangeParent(OPNode newparent, INode oldchild)
            {
                if (parent != null)
                    parent.Remove(this);

                parent = newparent;

                if (oldchild != null)
                    newparent.Remove(oldchild);

                if (newparent != null)
                    newparent.Add(this);
            }
            abstract public void SelfCheck();

            public void Add(INode node)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] == null)
                    {
                        children[i] = node;
                        return;
                    }
                }
                return;
            }

            public void SetChildren(INode first, INode second)
            {
                children[0] = first;
                children[1] = second;
            }

            public void Remove(INode node)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] == node)
                    {
                        children[i] = null;
                        return;
                    }
                }
                return;
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
            public void ChangeParent(OPNode newparent, INode oldchild)
            {
                if (parent != null)
                    parent.Remove(this);
                parent = newparent;
                if (oldchild != null)
                    newparent.Remove(oldchild);

                if (newparent != null)
                    newparent.Add(this);
            }
            abstract public void SelfCheck();

            public void Differentiate()
            {
                Parent.Remove(this);
            }
        }
        sealed class DiffVariable : ConstNode
        {
            public DiffVariable(OPNode parent)
            {
                ChangeParent(parent, null);
            }
            public override void SelfCheck()
            {

            }
        }
        sealed class Constant : ConstNode
        {
            double value;

            public Constant(double value)
            {
                ChangeParent(parent, null);
            }

            public override void SelfCheck()
            {
                if (value == 1)
                    ChangeParent(null, null);
            }
            public double Value
            {
                get { return value; }
                set { this.value = value; }
            }
        }
        sealed class Plus : OPNode
        {
            public Plus(OPNode parent)
            {
                ChangeParent(parent, null);
            }

            public override void SelfCheck()
            {
                
            }

            public override void Differentiate()
            {
                children[0].Differentiate();
                children[1].Differentiate();
            }
        }
        sealed class Minus : OPNode
        {
            public Minus(OPNode parent)
            {
                ChangeParent(parent, null);
            }

            public override void SelfCheck()
            {

            }

            public override void Differentiate()
            {
                children[0].Differentiate();
                children[1].Differentiate();
            }
        }
        sealed class Multi : OPNode
        {
            public Multi(OPNode parent)
            {
                ChangeParent(parent, null);
            }
            public override void SelfCheck()
            {
                if (children[0] is Constant constant)
                {
                    
                }
                else if (children[0] == null || children[1] == null)
                {
                    foreach (INode child in children)
                    {
                        if (child != null)
                        {
                            child.ChangeParent(this.parent, this);
                            return;
                        }
                    }
                    parent.Remove(this);
                    parent = null;
                }
                

            }

            public override void Differentiate()
            {
                Plus plus = new(this);
                Multi first = new(plus);
                Multi second = new(plus);

                first.SetChildren(children[0], children[1]);
                first.children[0].Differentiate();

                second.SetChildren(children[0], children[1]);
                second.children[1].Differentiate();

                plus.SetChildren(first, second);
            }
        }
        sealed class Divi : OPNode
        {
            public Divi(OPNode parent)
            {
                ChangeParent(parent, null);
            }
            public override void SelfCheck()
            {
                if (children[0] == null || children[1] == null)
                {
                    foreach (INode child in children)
                    {
                        if (child != null)
                        {
                            child.ChangeParent(this.parent, this);
                            return;
                        }
                    }
                    parent.ChangeParent(null, this);
                }
            }

            public override void Differentiate()
            {

            }
        }
    }
}
