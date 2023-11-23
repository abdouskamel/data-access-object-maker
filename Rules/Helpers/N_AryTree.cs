using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rules.Helpers
{
    [Serializable]
    public class N_AryTree<T>
    {
        public T Value { get; set; }

        public N_AryTree<T> Father { get; protected set; }

        public List<N_AryTree<T>> Children { get; protected set; }

        public N_AryTree()
        {
            this.Children = new List<N_AryTree<T>>();
        }

        public N_AryTree(T Value, N_AryTree<T> Father = null, params N_AryTree<T>[] Children)
        {
            this.Value = Value;
            this.Father = Father;

            if (Children.Length != 0)
            {
                this.Children = new List<N_AryTree<T>>(Children.Length);
                foreach (N_AryTree<T> child in Children)
                    AddChild(child);
            }

            else
                this.Children = new List<N_AryTree<T>>();
        }

        public void AddChild(N_AryTree<T> child)
        {
            child.Father = this;
            Children.Add(child);
        }

        public void RemoveChild(N_AryTree<T> child)
        {
            Children.Remove(child);
            child.Father = null;
        }

        public void ClearChildren()
        {
            foreach (N_AryTree<T> child in Children)
                child.Father = null;

            Children.Clear();
        }
    }
}
