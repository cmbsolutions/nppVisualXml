using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace nppVisualXml.Modules
{

    internal static class TreeSearchOwnerDraw
    {
        private struct Range { public int Start; public int Length; }

        // One dictionary per TreeView (supports multiple trees if needed)
        private static readonly Dictionary<TreeView, Dictionary<TreeNode, List<Range>>> _maps
            = new Dictionary<TreeView, Dictionary<TreeNode, List<Range>>>();

        /// <summary>
        /// Call once (e.g., Form constructor) to enable owner-draw on this TreeView.
        /// </summary>
        public static void Attach(TreeView tree)
        {
            if (tree == null) return;
            if (!_maps.ContainsKey(tree))
            {
                _maps[tree] = new Dictionary<TreeNode, List<Range>>();
                tree.DrawMode = TreeViewDrawMode.OwnerDrawText;
                tree.DrawNode -= OnDrawNode;
                tree.DrawNode += OnDrawNode;
                tree.Disposed -= OnTreeDisposed;
                tree.Disposed += OnTreeDisposed;
            }
        }

        /// <summary>
        /// Compute and highlight matches. Returns match count (nodes x hits).
        /// </summary>
        public static int SearchAndHighlight(
            TreeView tree,
            string query,
            bool caseSensitive = false,
            bool useRegex = false)
        {
            if (tree == null || !_maps.ContainsKey(tree)) return 0;
            var map = _maps[tree];
            map.Clear();
            if (string.IsNullOrEmpty(query))
            {
                tree.Invalidate();
                return 0;
            }

            int totalHits = 0;

            if (useRegex)
            {
                var options = caseSensitive ? RegexOptions.CultureInvariant : (RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                var re = new Regex(query, options);
                foreach (var node in Enumerate(tree.Nodes))
                {
                    var text = node.Text ?? string.Empty;
                    var matches = re.Matches(text);
                    if (matches.Count > 0)
                    {
                        var list = new List<Range>(matches.Count);
                        foreach (Match m in matches)
                        {
                            if (m.Length <= 0) continue;
                            list.Add(new Range { Start = m.Index, Length = m.Length });
                            totalHits++;
                        }
                        map[node] = list;
                        ExpandAncestors(node);
                    }
                }
            }
            else
            {
                var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                foreach (var node in Enumerate(tree.Nodes))
                {
                    var text = node.Text ?? string.Empty;
                    int idx = 0;
                    List<Range> list = null;
                    while (true)
                    {
                        idx = text.IndexOf(query, idx, comparison);
                        if (idx < 0) break;
                        if (list == null) list = new List<Range>();
                        list.Add(new Range { Start = idx, Length = query.Length });
                        totalHits++;
                        idx += Math.Max(1, query.Length); // avoid infinite loop on empty
                    }
                    if (list != null)
                    {
                        map[node] = list;
                        ExpandAncestors(node);
                    }
                }
            }

            // Optional: scroll the first hit into view
            foreach (var n in Enumerate(tree.Nodes))
            {
                if (map.ContainsKey(n))
                {
                    tree.TopNode = n;
                    break;
                }
            }

            tree.Invalidate();
            return totalHits;
        }

        /// <summary>Clear all highlights.</summary>
        public static void Clear(TreeView tree)
        {
            if (tree == null || !_maps.ContainsKey(tree)) return;
            _maps[tree].Clear();
            tree.Invalidate();
        }

        // ---- Rendering ----

        private static void OnDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var tree = (TreeView)sender;
            _maps.TryGetValue(tree, out var map);

            string text = e.Node.Text ?? string.Empty;
            var font = e.Node.NodeFont ?? tree.Font;

            // Rectangle where the label text should appear
            Rectangle textBounds = e.Bounds;

            // Normal colors
            Color fore = e.Node.ForeColor.IsEmpty ? tree.ForeColor : e.Node.ForeColor;

            // If selected, let the system draw the highlight behind the text
            if ((e.State & TreeNodeStates.Selected) != 0)
            {
                e.DrawDefault = true;  // draws background + text normally
                                       // overlay match highlights afterward if needed
            }

            if (map != null && map.TryGetValue(e.Node, out var ranges) && ranges.Count > 0)
            {
                // Draw segmented text ourselves (over default)
                int x = textBounds.Left;
                int y = textBounds.Top;
                ranges.Sort((a, b) => a.Start.CompareTo(b.Start));

                int cursor = 0;
                foreach (var r in ranges)
                {
                    if (r.Start > cursor)
                    {
                        string pre = text.Substring(cursor, r.Start - cursor);
                        DrawSegment(e.Graphics, pre, font, ref x, y, fore, Color.Transparent);
                    }
                    string hit = text.Substring(r.Start, Math.Min(r.Length, text.Length - r.Start));
                    DrawSegment(e.Graphics, hit, font, ref x, y, Color.Black, Color.Yellow);
                    cursor = r.Start + r.Length;
                }
                if (cursor < text.Length)
                {
                    string tail = text.Substring(cursor);
                    DrawSegment(e.Graphics, tail, font, ref x, y, fore, Color.Transparent);
                }
            }
            else
            {
                // Default drawing is fine if no matches
                e.DrawDefault = true;
            }
        }

        private static void DrawSegment(Graphics g, string s, Font font, ref int x, int y, Color fore, Color back)
        {
            if (string.IsNullOrEmpty(s)) return;

            // Measure without padding to keep tight alignment
            var size = TextRenderer.MeasureText(g, s, font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            var rect = new Rectangle(x, y, size.Width, size.Height);

            if (back.A > 0 && back != Color.Transparent)
            {
                using (var b = new SolidBrush(back))
                    g.FillRectangle(b, rect);
            }

            TextRenderer.DrawText(
                g, s, font, new Point(x, y), fore,
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            x += size.Width;
        }

        // ---- Utilities ----

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
            for (var p = node.Parent; p != null; p = p.Parent)
                p.Expand();
        }

        private static void OnTreeDisposed(object sender, EventArgs e)
        {
            var tree = (TreeView)sender;
            if (_maps.ContainsKey(tree))
            {
                _maps.Remove(tree);
            }
        }
    }
}