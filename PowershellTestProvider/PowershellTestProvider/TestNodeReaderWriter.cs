using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Provider;
using System.Text;
using System.Threading.Tasks;

namespace PowershellTestProvider
{
    internal sealed class TestNodeReaderWriter : IContentReader, IContentWriter
    {
        private readonly TestNode node;
        private readonly bool isRaw;
        private bool isReadComplete;

        public TestNodeReaderWriter(TestNode node, bool isRaw)
        {
            this.node = node;
            this.isRaw = isRaw;
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        public IList Read(long readCount)
        {
            if(isReadComplete)
            {
                return new string[0];
            }

            if (isRaw)
            {
                isReadComplete = true;
                return new string[] { node.Content };
            }

            throw new NotSupportedException();
        }

        public void Seek(long offset, SeekOrigin origin)
        {
        }

        public IList Write(IList content)
        {
            StringBuilder builder = new StringBuilder();
            foreach (object item in content)
            {
                builder.Append(item).AppendLine();
            }

            node.Content = builder.ToString();
            return content;
        }
    }
}
