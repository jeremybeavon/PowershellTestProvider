using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PowershellTestProvider
{
    public sealed class TestNodeFilter
    {
        private readonly WildcardPattern filterPattern;
        private readonly IEnumerable<WildcardPattern> includePatterns;
        private readonly IEnumerable<WildcardPattern> excludePatterns;

        public TestNodeFilter(string filter, IEnumerable<string> include, IEnumerable<string> exclude)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filterPattern = CreatePattern(filter);
            }

            includePatterns = include == null ? new WildcardPattern[0] : include.Select(item => CreatePattern(item)).ToArray();
            excludePatterns = exclude == null ? new WildcardPattern[0] : exclude.Select(item => CreatePattern(item)).ToArray();
        }

        public bool IsMatch(TestNode node)
        {
            if (!includePatterns.Any(include => include.IsMatch(node.Name)) ||
                excludePatterns.Any(exclude => exclude.IsMatch(node.Name)))
            {
                return false;
            }

            if (filterPattern == null)
            {
                return true;
            }

            return filterPattern.IsMatch(node.Name);
        }

        private static WildcardPattern CreatePattern(string pattern)
        {
            return new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
        }
    }
}
