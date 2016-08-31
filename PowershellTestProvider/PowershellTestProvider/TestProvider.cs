using Microsoft.PowerShell.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text.RegularExpressions;

namespace PowershellTestProvider
{
    [CmdletProvider("Test", ProviderCapabilities.None)]
    public sealed class TestProvider : NavigationCmdletProvider, IContentCmdletProvider
    {
        private static readonly TestNode rootNode = new TestNode(null);
        
        public void ClearContent(string path)
        {
        }

        public object ClearContentDynamicParameters(string path)
        {
            return new FileSystemClearContentDynamicParameters();
        }

        public IContentReader GetContentReader(string path)
        {
            FileSystemContentReaderDynamicParameters parameters = DynamicParameters as FileSystemContentReaderDynamicParameters;
            return new TestNodeReaderWriter(GetNode(path), parameters != null && parameters.Raw);
        }

        public object GetContentReaderDynamicParameters(string path)
        {
            return new FileSystemContentReaderDynamicParameters();
        }

        public IContentWriter GetContentWriter(string path)
        {
            return new TestNodeReaderWriter(GetNode(path), false);
        }

        public object GetContentWriterDynamicParameters(string path)
        {
            return new FileSystemContentWriterDynamicParameters();
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            TestNode node = GetNode(path);
            TestNodeList childNodes = node.ChildNodes;
            if (childNodes != null)
            {
                foreach (TestNode childNode in childNodes)
                {
                    GetChildNodes(childNode, recurse);
                }
            }
        }

        protected override void GetItem(string path)
        {
            TestNode node = GetNode(path);
            if (node != null)
            {
                WriteItemObject(node, node.FullName, node.ChildNodes != null);
            }
        }

        protected override bool ItemExists(string path)
        {
            return GetNode(path) != null;
        }

        protected override bool HasChildItems(string path)
        {
            return GetNode(path)?.ChildNodes != null;
        }

        protected override void InvokeDefaultAction(string path)
        {
            TestNode node = GetNode(path);
            if (node != null && node.Content != null)
            {
                WriteItemObject(node.Content, path, false);
            }
        }

        protected override bool IsItemContainer(string path)
        {
            return GetNode(path)?.ChildNodes != null;
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            TestNode node = rootNode;
            foreach (string subPath in GetNodeParts(path))
            {
                TestNode parentNode = node;
                node = node[subPath];
                if (node == null)
                {
                    if (parentNode.ChildNodes == null)
                    {
                        parentNode.ChildNodes = new TestNodeList(parentNode);
                    }

                    node = new TestNode(subPath);
                    parentNode.ChildNodes.Add(node);
                }
            }

            if (newItemValue != null)
            {
                node.Content = newItemValue.ToString();
            }
        }

        

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            return new Collection<PSDriveInfo>()
            {
                new PSDriveInfo("test", ProviderInfo, "test:", "test", null)
            };
        }

        protected override bool IsValidPath(string path)
        {
            return true;
        }

        private static IEnumerable<string> GetNodeParts(string path)
        {
            return Regex.Replace(path, @"^test:\\?", string.Empty).Split('\\').Where(subPath => !string.IsNullOrWhiteSpace(subPath));
        }

        private TestNode GetNode(string path)
        {
            TestNode node = rootNode;
            foreach (string subPath in GetNodeParts(path))
            {
                node = node[subPath];
                if (node == null)
                {
                    return null;
                }
            }

            return node;
        }

        private void GetChildNodes(TestNode node, bool recurse)
        {
            TestNodeList childNodes = node.ChildNodes;
            if (childNodes == null)
            {
                WriteItemObject(node, node.FullName, false);
            }
            else
            {
                WriteItemObject(node, node.FullName, true);
                if (recurse)
                {
                    foreach (TestNode childNode in childNodes)
                    {
                        GetChildNodes(childNode, recurse);
                    }
                }
            }
        }
    }
}
