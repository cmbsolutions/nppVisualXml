using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace nppVisualXml.Modules
{
    internal static class TreeSearch
    {
        /// <summary>
        /// Highlights every node whose Text matches the query.
        /// Returns the number of matches.
        /// </summary>
        public static int HighlightMatches(
            TreeView tree,
            string query,
            bool caseSensitive = false,
            bool useRegex = false)
        {
            if (tree == null) return 0;
            ClearHighlights(tree);

            if (string.IsNullOrWhiteSpace(query)) return 0;

            Func<string, bool> isMatch;

            if (useRegex)
            {
                var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                var re = new Regex(query, options | RegexOptions.CultureInvariant);
                isMatch = s => re.IsMatch(s ?? string.Empty);
            }
            else
            {
                var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                isMatch = s => (s ?? string.Empty).IndexOf(query, comparison) >= 0;
            }

            int count = 0;
            foreach (var node in Enumerate(tree.Nodes))
            {
                if (isMatch(node.Text))
                {
                    // highlight whole node
                    node.BackColor = Color.Yellow;
                    node.ForeColor = Color.Black;
                    count++;

                    // Expand the path so users can see it
                    ExpandAncestors(node);
                }
            }

            // Optional: scroll to first one
            if (count > 0)
                tree.TopNode = GetFirstHighlighted(tree);

            return count;
        }

        public static void ClearHighlights(TreeView tree)
        {
            foreach (var node in Enumerate(tree.Nodes))
            {
                node.BackColor = Color.Empty; // reset to default
                node.ForeColor = Color.Empty;
            }
        }

        private static IEnumerable<TreeNode> Enumerate(TreeNodeCollection nodes)
        {
            var stack = new Stack<TreeNode>();
            for (int i = nodes.Count - 1; i >= 0; i--)
                stack.Push(nodes[i]);

            while (stack.Count > 0)
            {
                var n = stack.Pop();
                yield return n;

                for (int i = n.Nodes.Count - 1; i >= 0; i--)
                    stack.Push(n.Nodes[i]);
            }
        }

        private static void ExpandAncestors(TreeNode node)
        {
            var p = node.Parent;
            while (p != null)
            {
                p.Expand();
                p = p.Parent;
            }
        }

        private static TreeNode GetFirstHighlighted(TreeView tree)
        {
            foreach (var n in Enumerate(tree.Nodes))
                if (n.BackColor == Color.Yellow) return n;
            return null;
        }
    }
}
