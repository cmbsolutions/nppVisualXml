using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;

namespace nppVisualXml.Modules
{
    internal static class TreeNodeIndex
    {
        private static readonly Dictionary<XObject, TreeNode> _nodeByX = new Dictionary<XObject, TreeNode>();

        public static void Register(TreeNode node, XObject xobj)
        {
            if (node == null || xobj == null) return;
            if (_nodeByX.ContainsKey(xobj)) return;
            _nodeByX.Add(xobj, node);
        }

        public static void Clear()
        {
            _nodeByX.Clear();
        }

        public static bool TryGetNode(XObject xobj, out TreeNode value)
        {
            if (xobj == null)
            {
                value = null;
                return false;
            }
            if (_nodeByX.TryGetValue(xobj, out var node))
            {
                value = node;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
