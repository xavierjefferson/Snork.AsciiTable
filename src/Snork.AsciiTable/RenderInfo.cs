using System.Collections.Generic;
using System.Linq;

namespace Snork.AsciiTable
{
    internal class RenderInfo
    {
        public Dictionary<int, string> TabReplacementCharacters { get; }

        public CellLines[,] Values { get; set; }

        public char HorizontalEdge { get; }
        public char VerticalEdge { get; }
        public RenderInfo(Dictionary<int, int> columnLengths, int columnCount,
            Dictionary<int, string> tabReplacementCharacters, char horizontalEdge, char verticalEdge)
        {
            MaxColumnLength = columnLengths.Any() ? columnLengths.Values.Max() : 0;
            ColumnLengths = columnLengths;
            ColumnCount = columnCount;
            TabReplacementCharacters = tabReplacementCharacters;
            HorizontalEdge = horizontalEdge;
            VerticalEdge = verticalEdge;
        }

        public int MaxColumnLength { get; }

        public Dictionary<int, int> ColumnLengths { get; }
        public int ColumnCount { get; }
    }
}