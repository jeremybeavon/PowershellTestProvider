using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace PowershellTestProvider
{
    public sealed class TestNodeList : Collection<TestNode>
    {
        private readonly TestNode parentNode;

        public TestNodeList(TestNode parentNode)
        {
            this.parentNode = parentNode;
        }

        public TestNodeList(TestNode parentNode, IEnumerable<TestNode> nodes)
            : this(parentNode)
        {
            foreach (TestNode node in nodes)
            {
                Add(node);
            }
        }

        public TestNode this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name) || name == ".")
                {
                    return parentNode;
                }

                if (name == "..")
                {
                    return parentNode.ParentNode ?? parentNode;
                }

                if (name.Contains("*"))
                {
                    WildcardPattern pattern = new WildcardPattern(name, WildcardOptions.IgnoreCase);
                    return Enumerable.FirstOrDefault(this, node => pattern.IsMatch(node.Name));
                }

                return Enumerable.FirstOrDefault(this, node => string.Equals(node.Name, name, StringComparison.OrdinalIgnoreCase));
            }
        }

        protected override void ClearItems()
        {
            foreach (TestNode node in this)
            {
                node.ParentNode = null;
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, TestNode item)
        {
            item.ParentNode = parentNode;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this[index].ParentNode = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TestNode item)
        {
            this[index].ParentNode = null;
            item.ParentNode = parentNode;
            base.SetItem(index, item);
        }
    }
}
