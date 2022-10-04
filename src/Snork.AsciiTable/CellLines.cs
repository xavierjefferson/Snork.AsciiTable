using System.Collections.Generic;

namespace Snork.AsciiTable
{
    internal class CellLines : List<string>
    {
        public object CellValue { get; set; }

        public CellLines()
        {
        }

        public CellLines(IEnumerable<string> lines) : base(lines)
        {
        }
    }
}