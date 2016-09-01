using Microsoft.PowerShell.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text.RegularExpressions;

namespace PowershellTestProvider
{
    [CmdletProvider("Test", ProviderCapabilities.Filter | ProviderCapabilities.Exclude | ProviderCapabilities.Include)]
    public sealed class TestProvider : NavigationCmdletProvider, IContentCmdletProvider
    {
        private static readonly TestNode rootNode = new TestNode(null);

        public static void AddTestFiles(IEnumerable files)
        {
            AddTestFiles(files, rootNode);
        }

        public static void RemoveAllTestFiles()
        {
            if (rootNode.ChildNodes != null)
            {
                rootNode.ChildNodes.Clear();
            }
        }

        public void ClearContent(string path)
        {
            TestNode node = GetNode(path);
            if (node != null)
            {
                node.Content = string.Empty;
            }
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
            GetChildNodes(path, recurse, output => output);
        }

        protected override string GetChildName(string path)
        {
            return GetNode(path)?.Name;
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            GetChildNodes(path, false, output => output.Name);
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

        protected override string MakePath(string parent, string child)
        {
            if (child == null)
            {
                return parent;
            }

            return Path.Combine(parent, child);
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
        
        protected override void RenameItem(string path, string newName)
        {
            TestNode node = GetNode(path);
            if (node != null)
            {
                node.Name = newName;
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

        private static void AddTestFiles(IEnumerable testFiles, TestNode parentNode)
        {
            if (parentNode.ChildNodes == null)
            {
                parentNode.ChildNodes = new TestNodeList(parentNode);
            }

            foreach (IDictionary testFile in testFiles)
            {
                string name = testFile["Name"] as string;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                TestNode node = new TestNode(name);
                parentNode.ChildNodes.Add(node);
                string content = testFile["Content"] as string;
                if (content != null)
                {
                    node.Content = content;
                }
                else
                {
                    IEnumerable files = testFile["Files"] as IEnumerable;
                    if (files != null)
                    {
                        AddTestFiles(files, node);
                    }
                }
            }
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

        private void GetChildNodes(string path, bool recurse, Func<TestNode, object> output)
        {
            TestNode node = GetNode(path);
            TestNodeList childNodes = node.ChildNodes;
            if (childNodes != null)
            {
                TestNodeFilter filter = new TestNodeFilter(Filter, Include, Exclude);
                foreach (TestNode childNode in childNodes.Where(filter.IsMatch))
                {
                    GetChildNodes(childNode, recurse, output, filter);
                }
            }
        }

        private void GetChildNodes(TestNode node, bool recurse, Func<TestNode, object> output, TestNodeFilter filter)
        {
            TestNodeList childNodes = node.ChildNodes;
            if (childNodes == null)
            {
                WriteItemObject(output(node), node.FullName, false);
            }
            else
            {
                WriteItemObject(output(node), node.FullName, true);
                if (recurse)
                {
                    foreach (TestNode childNode in childNodes.Where(filter.IsMatch))
                    {
                        GetChildNodes(childNode, recurse, output, filter);
                    }
                }
            }
        }
    }
}
