using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            get { return Enumerable.FirstOrDefault(this, node => node.Name == name); }
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
