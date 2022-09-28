using System;

namespace Snork.AsciiTable
{
    public class Options
    {
        [Obsolete("Use CaptionCellAlignment")]
        public CellAlignmentEnum HeaderCellAlignment
        {
            get { return this.CaptionCellAlignment; }
            set { this.CaptionCellAlignment = value; }
        }
        public CellAlignmentEnum CaptionCellAlignment { get; set; } = CellAlignmentEnum.Center;
        public CellAlignmentEnum TitleCellAlignment { get; set; } = CellAlignmentEnum.Center;
        public string Title { get; set; }
        public string Prefix { get; set; }
        public bool DisplayCaptions { get; set; } = true;

        [Obsolete("Use DisplayCaptions")]
        public bool DisplayHeader
        {
            get { return this.DisplayCaptions;}
            set { this.DisplayCaptions = value; }
        }
    }
}