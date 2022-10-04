using System;

namespace Snork.AsciiTable
{
    public class Options
    {
        public char HorizontalEdge { get; set; } = '|';
        public char VerticalEdge { get; set; } = '-';
        public char TopCorner { get; set; } = '.';
        public char BottomCorner { get; set; } = '\'';

        [Obsolete("Use CaptionCellAlignment")]
        public CellAlignmentEnum HeaderCellAlignment
        {
            get => CaptionCellAlignment;
            set => CaptionCellAlignment = value;
        }

        /// <summary>
        /// Cell alignment for captions
        /// </summary>
        public CellAlignmentEnum CaptionCellAlignment { get; set; } = CellAlignmentEnum.Center;

        /// <summary>
        /// Cell alignment for title, if set
        /// </summary>
        public CellAlignmentEnum TitleCellAlignment { get; set; } = CellAlignmentEnum.Center;

        /// <summary>
        /// Table title.  Defaults to null
        /// </summary>
        public string Title { get; set; }

        [Obsolete("Use LinePrefix")]
        public string Prefix
        {
            get => LinePrefix;
            set => LinePrefix = value;
        }

        /// <summary>
        /// Prefix to add to each line on render
        /// </summary>
        public string LinePrefix { get; set; }

        /// <summary>
        /// Show captions with a row separator
        /// </summary>
        public bool DisplayCaptions { get; set; } = true;

        [Obsolete("Use DisplayCaptions")]
        public bool DisplayHeader
        {
            get => DisplayCaptions;
            set => DisplayCaptions = value;
        }

        /// <summary>
        /// Add row separators between each row of data
        /// </summary>
        public bool DisplayRowSeparators { get; set; } = false;

        /// <summary>
        /// Show border around table
        /// </summary>
        public bool DisplayBorder { get; set; } = true;
    }
}