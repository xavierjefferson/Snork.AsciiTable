using System.Data;

namespace Snork.AsciiTable
{
    internal class AsciiDataColumn : DataColumn
    {
        public CellAlignmentEnum CellAlignment { get; set; } = CellAlignmentEnum.NotSpecified;

        public CellAlignmentEnum CaptionAlignment { get; set; } = CellAlignmentEnum.NotSpecified;

        public AsciiDataColumn()
        {
            this.DataType = typeof(object);
        }
    }
}