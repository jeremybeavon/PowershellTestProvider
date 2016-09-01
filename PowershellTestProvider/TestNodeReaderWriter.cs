using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Provider;
using System.Text;

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

            isReadComplete = true;
            if (isRaw)
            {
                return new string[] { node.Content };
            }

            List<string> lines = new List<string>();
            using (StringReader reader = new StringReader(node.Content))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
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
