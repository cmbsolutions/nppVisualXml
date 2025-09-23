using Kbg.NppPluginNET.PluginInfrastructure;
using System.Text;


namespace nppVisualXml.Modules
{
    internal static class NppText
    {
        public static string GetActiveDocumentText()
        {
            var editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            if (editor.TryGetLengthAsInt(out int length))
            {
                var sb = new StringBuilder(length + 1);
                sb.Append(editor.GetText(length));
                return sb.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}
