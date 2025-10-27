using System.Collections.Generic;

namespace nppVisualXml.Modules
{
    public sealed class StructuredQuery
    {
        public List<string> Path;
        public bool Exact;
        public string Value;

        public StructuredQuery(List<string> path, bool exact, string value)
        {
            Path = path;
            Exact = exact;
            Value = value;
        }
    }
}
