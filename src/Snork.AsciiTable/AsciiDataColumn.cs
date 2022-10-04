using System.Data;
using System.Threading;
using Snork.TextWrap;

namespace Snork.AsciiTable
{
    internal class AsciiDataColumn : DataColumn
    {
        public TextWrapperOptions TextWrapperOptions { get; set; } = new TextWrapperOptions()
            { ExpandTabs = true, BreakLongWords = true };

        public CellAlignmentEnum CellAlignment { get; set; } = CellAlignmentEnum.NotSpecified;

        public CellAlignmentEnum CaptionAlignment { get; set; } = CellAlignmentEnum.NotSpecified;

        public ColumnWidthTypeEnum ColumnWidthType { get; set; } = ColumnWidthTypeEnum.Auto;

        public int ColumnWidth { get; set; }

        public AsciiDataColumn()
        {
            DataType = typeof(object);
        }
    }
}