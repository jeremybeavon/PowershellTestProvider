using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowershellTestProvider
{
    public sealed class TestNode
    {
        private object value;

        public TestNode(string name)
        {
            Name = name;
        }

        private TestNode(TestNode node)
        {
            Name = node.Name;
            TestNodeList childNodes = node.ChildNodes;
            value = childNodes == null ?
                (object)node.Content :
                new TestNodeList(this, childNodes.Select(childNode => new TestNode(childNode)));
        }

        public string Name { get; set; }

        public string FullName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                BuildFullName(builder);
                return builder.ToString();
            }
        }

        public string Content
        {
            get { return value as string; }
            set { this.value = value; }
        }

        public TestNode ParentNode { get; internal set; }

        public TestNodeList ChildNodes
        {
            get { return value as TestNodeList; }
            set { this.value = value; }
        }

        public TestNode this[string name]
        {
            get
            {
                TestNodeList childNodes = ChildNodes;
                return childNodes == null ? null : childNodes[name];
            }
        }

        private void BuildFullName(StringBuilder builder)
        {
            if (ParentNode == null)
            {
                builder.Append("test:");
            }
            else
            {
                ParentNode.BuildFullName(builder);
                builder.Append("\\").Append(Name);
            }
        }
    }
}
