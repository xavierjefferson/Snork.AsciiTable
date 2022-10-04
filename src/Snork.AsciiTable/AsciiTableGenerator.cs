using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Snork.TextWrap;

namespace Snork.AsciiTable
{
    public class AsciiTableGenerator
    {
        //future use?
        private const int _spacing = 1;

        private static readonly Dictionary<int, AlignDelegate> AlignDelegates =
            new Dictionary<int, AlignDelegate>
            {
                {
                    (int)CellAlignmentEnum.Center,
                    (type, value, length, padChar) => AlignmentHelper.AlignCenter(value, length, padChar)
                },
                {
                    (int)CellAlignmentEnum.Right,
                    (type, value, length, padChar) => AlignmentHelper.AlignRight(value, length, padChar)
                },
                {
                    (int)CellAlignmentEnum.Left,
                    (type, value, length, padChar) => AlignmentHelper.AlignLeft(value, length, padChar)
                },
                {
                    0,
                    (type, value, length, padChar) => AlignmentHelper.AlignAuto(type, value, length, padChar)
                }
            };

        private readonly DataTable _dataTable = new DataTable();
        private bool _hasCaptions;

        /// <summary>
        ///     Constructor for AsciiTableGenerator, with title
        /// </summary>
        /// <param name="title">The title</param>
        public AsciiTableGenerator(string title) : this(new Options { Title = title })
        {
        }

        /// <summary>
        ///     Constructor for AsciiTableGenerator, with options
        /// </summary>
        /// <param name="options">The Options instance</param>
        public AsciiTableGenerator(Options options = null)
        {
            Options = options ?? new Options();
            Clear();
        }


        public Options Options { get; }


        /// <summary>
        ///     Clear current table data and reset settings to defaults
        /// </summary>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator Clear()
        {
            _hasCaptions = false;
            return ClearRows();
        }

        /// <summary>
        ///     Get current table data as list of list
        /// </summary>
        /// <returns>The current instance</returns>
        public List<List<object>> GetRows()
        {
            return _dataTable.Rows.Cast<DataRow>().Select(i => i.ItemArray.ToList()).ToList();
        }

        /// <summary>
        ///     Clear current table data
        /// </summary>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator ClearRows()
        {
            _dataTable.Clear();
            _dataTable.Columns.Clear();
            return this;
        }

        /// <summary>
        ///     Set width for a given column, by index.  ColumnWidthType can be Fixed or Auto
        /// </summary>
        /// <param name="index">The index of the column</param>
        /// <param name="columnWidthType">The column width type</param>
        /// <param name="width">If the type is Fixed, a positive integer denoting the width</param>
        /// <returns>The current instance</returns>
        /// <exception cref="ArgumentException"></exception>
        public AsciiTableGenerator SetColumnWidth(int index, ColumnWidthTypeEnum columnWidthType, int? width = null)
        {
            if (columnWidthType == ColumnWidthTypeEnum.Fixed)
                if (width == null || width <= 0)
                    throw new ArgumentException("Width for fixed column must be >=0");
            EnsureColumnsExist(index + 1);
            var col = GetColumn(index);
            col.ColumnWidthType = columnWidthType;
            col.ColumnWidth = width ?? 0;
            return this;
        }

        /// <summary>
        ///     Add a range of rows
        /// </summary>
        /// <param name="rows">An enumerable</param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator AddRange(IEnumerable<List<object>> rows)
        {
            foreach (var item in rows) Add(item);

            return this;
        }


        private static string Align(CellAlignmentEnum cellAlignment, object cellValue, int length, char padChar)
        {
            var valueType = GetValueType(cellValue);
            switch (cellAlignment)
            {
                case CellAlignmentEnum.NotSpecified:
                case CellAlignmentEnum.Left:
                    return AlignmentHelper.AlignLeft(cellValue, length, padChar);
                case CellAlignmentEnum.Right:
                    return AlignmentHelper.AlignRight(cellValue, length, padChar);
                case CellAlignmentEnum.Center:

                    return AlignmentHelper.AlignCenter(cellValue, length, padChar);
            }

            return AlignmentHelper.AlignAuto(valueType, cellValue, length, padChar);
        }

        /// <summary>
        ///     Set alignment for title cell
        /// </summary>
        /// <param name="alignment">Alignment value</param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator SetTitleAlignment(CellAlignmentEnum alignment)
        {
            Options.TitleCellAlignment = alignment;
            return this;
        }

        /// <summary>
        ///     Set alignment for title cell (obsolete)
        /// </summary>
        /// <param name="alignment">Alignment value</param>
        /// <returns>The current instance</returns>
        [Obsolete("Use SetTitleAlignment")]
        public AsciiTableGenerator SetTitleAlign(CellAlignmentEnum alignment)
        {
            return SetTitleAlignment(alignment);
        }

        /// <summary>
        ///     Display row separators between each row of data, for improved visibility
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator DisplayRowSeparators(bool value)
        {
            Options.DisplayRowSeparators = value;
            return this;
        }

        /// <summary>
        ///     Set the border characters for rendering, if no arguments are passed it will be reset to defaults. If a single edge
        ///     arg is passed, it will be used for all borders.
        /// </summary>
        /// <param name="horizontalEdge">Character for horizontal edges, defaults to "|"</param>
        /// <param name="verticalEdge">Character for vertical edges, defaults to "-"</param>
        /// <param name="topCorner">Character for top corners, defaults to "."</param>
        /// <param name="bottomCorner">Character for bottom corners, defaults to "'"</param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator SetBorder(char? horizontalEdge = null, char? verticalEdge = null,
            char? topCorner = null,
            char? bottomCorner = null)
        {
            Options.DisplayBorder = true;
            if (verticalEdge == null && topCorner == null && bottomCorner == null)
                verticalEdge = topCorner = bottomCorner = horizontalEdge;

            Options.HorizontalEdge = horizontalEdge ?? '|';
            Options.VerticalEdge = verticalEdge ?? '-';
            Options.TopCorner = topCorner ?? '.';
            Options.BottomCorner = bottomCorner ?? '\'';
            return this;
        }

        private List<AsciiDataColumn> GetColumns()
        {
            return _dataTable.Columns.Cast<AsciiDataColumn>().ToList();
        }

        /// <summary>
        ///     Set captions for all columns
        /// </summary>
        /// <param name="captions">A list of strings to use for captions</param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator SetCaptions(List<string> captions)
        {
            _hasCaptions = true;
            EnsureColumnsExist(captions.Count);

            var columns = GetColumns();
            foreach (var item in captions.Select((value, index) => new { Caption = value, Index = index }))
                columns[item.Index].Caption = item.Caption;

            return this;
        }

        /// <summary>
        ///     Set captions for all columns (obsolete)
        /// </summary>
        /// <param name="captions">A list of strings to use for captions</param>
        /// <returns>The current instance</returns>
        [Obsolete("Use SetCaptions")]
        public AsciiTableGenerator SetHeading(List<string> captions)
        {
            return SetCaptions(captions);
        }

        /// <summary>
        ///     Set captions for all columns
        /// </summary>
        /// <param name="captions">An array of strings to use for captions</param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator SetCaptions(params string[] captions)
        {
            return SetCaptions(captions.ToList());
        }

        /// <summary>
        ///     Set captions for all columns (obsolete)
        /// </summary>
        /// <param name="captions">An array of strings to use for captions</param>
        /// <returns>The current instance</returns>
        [Obsolete("Use SetCaptions")]
        public AsciiTableGenerator SetHeading(params string[] captions)
        {
            return SetCaptions(captions.ToList());
        }


        /// <summary>
        ///     Add a row of data
        /// </summary>
        /// <param name="row">An instance of IEnumerable</param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator Add(IEnumerable<object> row)
        {
            return Add(row.ToArray());
        }

        /// <summary>
        ///     Add a row of data
        /// </summary>
        /// <param name="row">An array of object type</param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator Add(params object[] row)
        {
            EnsureColumnsExist(row.Length);
            _dataTable.Rows.Add(row);
            return this;
        }

        private void EnsureColumnsExist(int rowLength)
        {
            if (_dataTable.Columns.Count >= rowLength) return;
            var toAdd = rowLength - _dataTable.Columns.Count;
            for (var i = 0; i < toAdd; i++)
                _dataTable.Columns.Add(new AsciiDataColumn());
        }

        /// <summary>
        ///     Create an AsciiTableGenerator instance with an enumerable of some type as its source
        /// </summary>
        /// <param name="data">The enumerable</param>
        /// <param name="options">Options for the AsciiTableGenerator</param>
        /// <param name="autoCaptions">If true, set the table captions using the property names of the class passed in</param>
        /// <returns>The new instance</returns>
        public static AsciiTableGenerator FromEnumerable<T>(IEnumerable<T> data, Options options = null,
            bool autoCaptions = true)
        {
            options = options ?? new Options();
            var result = new AsciiTableGenerator(options);
            var propertyInfos = typeof(T).GetProperties().Where(i => i.CanRead).ToList();
            result.EnsureColumnsExist(propertyInfos.Count);
            var columns = result.GetColumns();

            foreach (var item in propertyInfos.Select((value, index) => new { PropertyInfo = value, Index = index }))
                columns[item.Index].DataType = item.PropertyInfo.PropertyType;
            if (autoCaptions)
                result.SetCaptions(propertyInfos.Select(i => i.Name).ToList());
            foreach (var item in data)
            {
                var values = propertyInfos.Select(i => i.GetValue(item)).ToList();
                result.Add(values);
            }

            return result;
        }

        /// <summary>
        ///     Create an AsciiTableGenerator instance with a datatable as its source
        /// </summary>
        /// <param name="table">The DataTable instance</param>
        /// <param name="options">Options for the AsciiTableGenerator</param>
        /// <param name="autoCaptions">If true, set the table captions using the caption properties of the columns in the DataTable</param>
        /// <returns>The new instance</returns>
        public static AsciiTableGenerator FromDataTable(DataTable table, Options options = null,
            bool autoCaptions = true)
        {
            var result = new AsciiTableGenerator(options);
            foreach (var item in table.Columns.Cast<DataColumn>()
                         .Select((value, index) => new { DataColumn = value, Index = index }))
            {
                var col = new AsciiDataColumn
                {
                    DataType = item.DataColumn.DataType,
                    AllowDBNull = item.DataColumn.AllowDBNull,
                    DateTimeMode = item.DataColumn.DateTimeMode,
                    ColumnName = item.DataColumn.ColumnName
                };
                result._dataTable.Columns.Add(col);
            }

            if (autoCaptions)
                result.SetCaptions(table.Columns.Cast<DataColumn>().Select(i => i.Caption).ToList());

            foreach (DataRow row in table.Rows) result._dataTable.Rows.Add(row.ItemArray);

            return result;
        }


        /// <summary>
        ///     Get list of captions for all columns
        /// </summary>
        /// <returns>List of string</returns>
        public List<string> GetCaptions()
        {
            return GetColumns().Select(i => i.Caption).ToList();
        }

        private string RenderTitle(RenderInfo info, int length)
        {
            var name = $" {Options.Title} ";
            var str = Align(Options.TitleCellAlignment, name, length - 1, ' ');
            return $"{info.HorizontalEdge}{str}{info.HorizontalEdge}";
        }

        /// <summary>
        ///     Setting for whether captions are displayed or not
        /// </summary>
        /// <param name="value">Desired setting as boolean</param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator SetDisplayCaptions(bool value)
        {
            Options.DisplayCaptions = value;
            return this;
        }

        /// <summary>
        ///     Set the alignment of caption for a given column
        /// </summary>
        /// <param name="index">Index of the column</param>
        /// <param name="alignment">Alignment value</param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator SetCaptionAlignment(int index, CellAlignmentEnum alignment)
        {
            EnsureColumnsExist(index + 1);
            GetColumn(index).CaptionAlignment = alignment;
            return this;
        }

        private static Type GetValueType(object value)
        {
            return value == null ? typeof(object) : value.GetType();
        }

        private List<string> RenderFlatRow(RenderInfo renderInfo, List<string> row, char padChar,
            List<AsciiDataColumn> columns, RowTypeEnum rowType)
        {
            if (!row.Any()) return new List<string>();
            return RenderRow(renderInfo, padChar, columns, rowType,
                columnIndex => GetLines(row[columnIndex], columnIndex, renderInfo));
        }

        private List<string> RenderValueRow(RenderInfo renderInfo, char padChar,
            List<AsciiDataColumn> columns, RowTypeEnum rowType, int rowIndex)
        {
            return RenderRow(renderInfo, padChar, columns, rowType,
                columnIndex => renderInfo.Values[rowIndex, columnIndex]);
        }

        private List<string> RenderRow(RenderInfo renderInfo, char padChar,
            List<AsciiDataColumn> columns, RowTypeEnum rowType, CellLinesDelegate cellLinesFunc)
        {
            var result = new List<string>();

            var tmp = new List<List<string>> { new List<string> { "" } };

            var lengths = new Dictionary<int, int>();
            for (var index = 0; index < renderInfo.ColumnCount; index++)
                lengths[index] = renderInfo.ColumnLengths[index];

            for (var columnIndex = 0; columnIndex < renderInfo.ColumnCount; columnIndex++)
            {
                var length = lengths[columnIndex];

                CellAlignmentEnum alignment;
                if (rowType == RowTypeEnum.Caption)
                {
                    var columnCaptionAlignment = columns[columnIndex].CaptionAlignment;
                    alignment = columnCaptionAlignment == CellAlignmentEnum.NotSpecified
                        ? CellAlignmentEnum.Center
                        : columnCaptionAlignment;
                }
                else
                {
                    alignment = columns[columnIndex].CellAlignment;
                }

                var lines = cellLinesFunc(columnIndex);
                var valueType = GetValueType(lines.CellValue);


                int alignIndex;
                switch (alignment)
                {
                    case CellAlignmentEnum.Center:
                    case CellAlignmentEnum.Right:
                    case CellAlignmentEnum.Left:
                        alignIndex = (int)alignment;
                        break;
                    default:
                        alignIndex = 0;
                        break;
                }

                tmp.Add(lines.Select(i => AlignDelegates[alignIndex](valueType, i, length, padChar)).ToList());
            }


            var maxLines = tmp.Max(i => i.Count);
            for (var lineNumber = 0; lineNumber < maxLines; lineNumber++)
            {
                var listForLine = tmp.Select((list, index) =>
                        lineNumber < list.Count ? list[lineNumber] :
                        index == 0 ? "" : new string(' ', lengths[index - 1]))
                    .ToList();
                var front = string.Join($"{padChar}{renderInfo.HorizontalEdge}{padChar}", listForLine);
                front = front.Substring(1);
                var value = $"{front}{padChar}{renderInfo.HorizontalEdge}";
                result.Add(value);
            }

            return result;
        }

        private List<string> GetRowSeparator(RenderInfo renderInfo, List<AsciiDataColumn> columns)
        {
            var result = new List<string>();
            for (var i = 0; i < renderInfo.ColumnCount; i++) result.Add(renderInfo.VerticalEdge.ToString());
            return RenderFlatRow(renderInfo, result, renderInfo.VerticalEdge, columns, RowTypeEnum.Separator);
        }

        /// <summary>
        ///     Render the table
        /// </summary>
        /// <returns>String rendering of table</returns>
        public override string ToString()
        {
            var columns = GetColumns();
            var captions = GetCaptions();
            var info = GetExtents(captions, columns);
            var totalWidth = info.ColumnCount * 3;

            var resultLines = new List<string>();

            // Get 
            foreach (var cellWidth in info.ColumnLengths.Values)
                totalWidth += cellWidth + _spacing;

            totalWidth -= _spacing;

            if (!string.IsNullOrWhiteSpace(Options.Title) && totalWidth < Options.Title.Length + 2)
                totalWidth = Options.Title.Length + 2;

            // Heading
            if (Options.DisplayBorder)
                resultLines.Add(GetSeparator(info, totalWidth - info.ColumnCount + 1, Options.TopCorner));

            if (!string.IsNullOrWhiteSpace(Options.Title))
            {
                if (totalWidth < Options.Title.Length) totalWidth = Options.Title.Length;
                resultLines.Add(RenderTitle(info, totalWidth - info.ColumnCount + 1));
                if (Options.DisplayBorder) resultLines.Add(GetSeparator(info, totalWidth - info.ColumnCount + 1));
            }

            var rowSeparator = GetRowSeparator(info, columns);
            if (_hasCaptions)
            {
                resultLines.AddRange(RenderFlatRow(info, captions, ' ', columns, RowTypeEnum.Caption));
                resultLines.AddRange(rowSeparator);
            }

            for (var rowIndex = 0; rowIndex < _dataTable.Rows.Count; rowIndex++)
            {
                if (rowIndex > 0 && Options.DisplayRowSeparators)
                    resultLines.AddRange(rowSeparator);
                resultLines.AddRange(RenderValueRow(info, ' ', columns, RowTypeEnum.NotSpecified, rowIndex));
            }

            if (Options.DisplayBorder)
                resultLines.Add(GetSeparator(info, totalWidth - info.ColumnCount + 1, Options.BottomCorner));

            var prefix = Options.LinePrefix ?? string.Empty;
            return prefix + string.Join($"{Environment.NewLine}{prefix}", resultLines);
        }

        /// <summary>
        ///     Set text wrapping options for a particular column
        /// </summary>
        /// <param name="index"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public AsciiTableGenerator SetTextWrapperOptions(int index, Action<TextWrapperOptions> action)
        {
            EnsureColumnsExist(index + 1);
            action(GetColumn(index).TextWrapperOptions);
            return this;
        }

        private CellLines GetLines(object cellValue, int columnIndex, RenderInfo renderInfo)
        {
            var result = new CellLines { CellValue = cellValue };
            using (var sr = new StringReader(cellValue.ToString()))
            {
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line == null) return result;
                    if (renderInfo.TabReplacementCharacters[columnIndex] == null)
                        result.Add(line);
                    else
                        result.Add(line.Replace("\t", renderInfo.TabReplacementCharacters[columnIndex]));
                }
            }
        }

        private RenderInfo GetExtents(List<string> captions,
            List<AsciiDataColumn> columns)
        {
            var tabReplacementCharacters = new Dictionary<int, string>();
            foreach (var column in columns.Select((i, j) => new { i, j }))
                tabReplacementCharacters[column.j] = column.i.TextWrapperOptions.ExpandTabs
                    ? new string(' ', column.i.TextWrapperOptions.TabSize)
                    : string.Empty;

            foreach (var item in columns.Select((i, j) => new { Value = i, index = j }))
            {
                var col = item.Value;
                if (col.ColumnWidthType == ColumnWidthTypeEnum.Fixed && col.ColumnWidth <= 0)
                    throw new InvalidOperationException(
                        $"Column {item.index} {nameof(AsciiDataColumn.ColumnWidth)} must be >=0 (currently {col.ColumnWidth})");
            }

            var columnLengths = columns.Select((i, j) => j).ToDictionary(i => i, j => 0);
            var renderInfo = new RenderInfo(columnLengths, columns.Count, tabReplacementCharacters,
                Options.DisplayBorder ? Options.HorizontalEdge : ' ',
                Options.DisplayBorder ? Options.VerticalEdge : ' ');

            if (Options.DisplayCaptions && _hasCaptions)
                foreach (var data in captions.Select((value, index) => new { Value = value, Index = index }))
                {
                    var column = columns[data.Index];

                    int ComputeLengthFunc(int idx)
                    {
                        return data.Value == null
                            ? 0
                            : GetLines(data.Value, idx, renderInfo).Max(i => i.Length);
                    }

                    if (column.ColumnWidthType == ColumnWidthTypeEnum.Fixed)
                        columnLengths[data.Index] = column.ColumnWidth;
                    else
                        columnLengths[data.Index] = Math.Max(0, ComputeLengthFunc(data.Index));
                }

            renderInfo.Values = new CellLines[_dataTable.Rows.Count, columns.Count];

            // Calculate max table cell lengths across all rows
            // and generate lines for each cell
            for (var colIndex = 0; colIndex < columns.Count; colIndex++)
            {
                var column = columns[colIndex];
                for (var rowIndex = 0; rowIndex < _dataTable.Rows.Count; rowIndex++)
                {
                    var value = _dataTable.Rows[rowIndex].ItemArray[colIndex];
                    CellLines lines;
                    if (column.ColumnWidthType == ColumnWidthTypeEnum.Auto)
                    {
                        lines = GetLines(value, colIndex, renderInfo);
                        if (lines.Any())
                            columnLengths[colIndex] = Math.Max(columnLengths[colIndex], lines.Max(i => i.Length));
                    }
                    else
                    {
                        lines = GetWrappedLines(value, column);
                        columnLengths[colIndex] = column.ColumnWidth;
                    }

                    renderInfo.Values[rowIndex, colIndex] = lines;
                }
            }


            return renderInfo;
        }

        private static CellLines GetWrappedLines(object value, AsciiDataColumn column)
        {
            return new CellLines(TextWrapper.Wrap(value.ToString(), column.ColumnWidth,
                    column.TextWrapperOptions))
                { CellValue = value };
        }

        private string GetSeparator(RenderInfo info, int length, char? sep = null)
        {
            return
                $"{sep ?? info.HorizontalEdge}{AlignmentHelper.AlignRight(sep ?? info.HorizontalEdge, length, info.VerticalEdge)}";
        }

        /// <summary>
        ///     Setting for whether or not to display the border around the cells
        /// </summary>
        /// <param name="value">New setting value (bool)</param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator DisplayBorder(bool value)
        {
            Options.DisplayBorder = value;
            return this;
        }

        private AsciiDataColumn GetColumn(int index)
        {
            EnsureColumnsExist(index + 1);
            return _dataTable.Columns.Cast<AsciiDataColumn>().Skip(index).First();
        }

        /// <summary>
        ///     Set alignment for cells in a given column
        /// </summary>
        /// <param name="index">The zero-based index of the column</param>
        /// <param name="cellAlignment">The alignment type</param>
        /// <returns>The current index</returns>
        public AsciiTableGenerator SetCellAlignment(int index, CellAlignmentEnum cellAlignment)
        {
            GetColumn(index).CellAlignment = cellAlignment;

            return this;
        }

        /// <summary>
        ///     Set alignment for cells in a given column
        /// </summary>
        /// <param name="index">The zero-based index of the column</param>
        /// <param name="cellAlignment">The alignment type</param>
        /// <returns>The current index</returns>
        [Obsolete("Use SetCellAlignment")]
        public AsciiTableGenerator SetAlign(int index, CellAlignmentEnum cellAlignment)
        {
            return SetCellAlignment(index, cellAlignment);
        }

        /// <summary>
        ///     Set title for the table.  Will be rendered in single cell that spans all columns
        /// </summary>
        /// <param name="name">The title value</param>
        /// <returns>The current instance</returns>
        public AsciiTableGenerator SetTitle(string name)
        {
            Options.Title = name;
            return this;
        }

        /// <summary>
        ///     Get the current title
        /// </summary>
        /// <returns>The current title</returns>
        public string GetTitle()
        {
            return Options.Title;
        }

        private delegate CellLines CellLinesDelegate(int columnIndex);

        private enum RowTypeEnum
        {
            NotSpecified,
            Caption,
            Separator
        }

        private delegate string AlignDelegate(Type valueType, object value, int length, char padChar);
    }
}