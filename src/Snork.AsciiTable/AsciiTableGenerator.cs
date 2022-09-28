using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace Snork.AsciiTable
{
    public class AsciiTableGenerator
    {
        private readonly DataTable _dataTable = new DataTable();
        private bool _border;
        private char _edge, _top, _bottom, _fill;
        private bool _equalColumnSize;
        private bool _hasCaptions;
        private int _spacing;

        public AsciiTableGenerator(string title) : this(new Options { Title = title })
        {
        }

        public AsciiTableGenerator(Options options = null)
        {
            Options = options ?? new Options();
            Clear();
        }


        public Options Options { get; }

        public AsciiTableGenerator Clear()
        {
            _hasCaptions = false;
            _spacing = 1;

            SetBorder();
            return this.ClearRows();
        }

        public List<List<object>> GetRows()
        {
            return _dataTable.Rows.Cast<DataRow>().Select(i => i.ItemArray.ToList()).ToList();
        }

        public AsciiTableGenerator ClearRows()
        {
            _dataTable.Clear();
            _dataTable.Columns.Clear();
            return this;
        }

        public AsciiTableGenerator AddRange(IEnumerable<List<object>> rows)
        {
            foreach (var item in rows) Add(item);

            return this;
        }

        public AsciiTableGenerator Add<T>(IEnumerable<T> data, Func<T, List<object>> rowCallback)
        {
            foreach (var item in data) Add(rowCallback(item));

            return this;
        }

        private static string Align(CellAlignmentEnum cellAlignment, object cellValue, int length, char padChar)
        {
            switch (cellAlignment)
            {
                case CellAlignmentEnum.NotSpecified:
                case CellAlignmentEnum.Left:
                    return AlignLeft(cellValue, length, padChar);
                case CellAlignmentEnum.Right:
                    return AlignRight(cellValue, length, padChar);
                case CellAlignmentEnum.Center:
                    return AlignCenter(cellValue, length, padChar);
            }

            return AlignAuto(cellValue, length, padChar);
        }

        public AsciiTableGenerator SetTitleAlign(CellAlignmentEnum cellAlignment)
        {
            Options.TitleCellAlignment = cellAlignment;
            return this;
        }

        public AsciiTableGenerator SetBorder(char? edge = null, char? fill = null, char? top = null,
            char? bottom = null)
        {
            _border = true;
            if (fill == null && top == null && bottom == null) fill = top = bottom = edge;

            _edge = edge ?? '|';
            _fill = fill ?? '-';
            _top = top ?? '.';
            _bottom = bottom ?? '\'';
            return this;
        }

        private static string AlignLeft(object cellValue, int length, char? padChar = ' ')
        {
            return (cellValue ?? "").ToString().PadRight(length, padChar ?? ' ');
        }

        private static string AlignCenter(object cellValue, int length, char? padChar = ' ')
        {
            var coalescedValue = (cellValue ?? "").ToString();
            var allSpace = length - coalescedValue.Length;
            if (allSpace > 0)
            {
                var leftSpaceLeft = Convert.ToInt32(allSpace / 2.0);
                var rightSpaceLength = allSpace - leftSpaceLeft;
                return
                    $"{new string(padChar ?? ' ', leftSpaceLeft)}{coalescedValue}{new string(padChar ?? ' ', rightSpaceLength)}";
            }

            return coalescedValue;
        }

        private static string AlignAuto(object cellValue, int length, char? padChar = ' ')
        {
            if (cellValue == null) cellValue = string.Empty;
            var asString = cellValue.ToString();
            if (asString.Length < length)
                return cellValue.HasNumericType() ? AlignRight(cellValue, length, padChar) : AlignLeft(cellValue, length, padChar);

            return asString;
        }

        private List<AsciiDataColumn> GetColumns()
        {
            return _dataTable.Columns.Cast<AsciiDataColumn>().ToList();
        }

        public AsciiTableGenerator SetCaptions(List<string> captions)
        {
            _hasCaptions = true;
            EnsureColumnsExist(captions.Count);

            var columns = GetColumns();
            foreach (var item in captions.Select((value, index) => new { Caption = value, Index = index }))
            {
                columns[item.Index].Caption = item.Caption;
            }

            return this;
        }

        [Obsolete("Use SetCaptions")]
        public AsciiTableGenerator SetHeading(List<string> captions)
        {
            return SetCaptions(captions);
        }

        public AsciiTableGenerator SetCaptions(params string[] captions)
        {

            return SetCaptions(captions.ToList());
        }

        [Obsolete("Use SetCaptions")]
        public AsciiTableGenerator SetHeading(params string[] captions)
        {
            return SetCaptions(captions.ToList());
        }

        public AsciiTableGenerator SetHeadingAlign(CellAlignmentEnum cellAlignment)
        {
            Options.CaptionCellAlignment = cellAlignment;
            return this;
        }

        public AsciiTableGenerator Add(IEnumerable<object> row)
        {
            return Add(row.ToArray());
        }

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

        public AsciiTableGenerator SetEqualColumnSize(bool value)
        {
            _equalColumnSize = value;
            return this;
        }

        public static AsciiTableGenerator FromEnumerable<T>(IEnumerable<T> data, Options options = null)
        {
            options = options ?? new Options();
            var result = new AsciiTableGenerator(options);
            var propertyInfos = typeof(T).GetProperties().Where(i => i.CanRead).ToList();
            result.EnsureColumnsExist(propertyInfos.Count);
            var columns = result.GetColumns();
            foreach (var item in propertyInfos.Select((value, index) => new { PropertyInfo = value, Index = index }))
            {
                columns[item.Index].DataType = item.PropertyInfo.PropertyType;
            }
            result.SetCaptions(propertyInfos.Select(i => i.Name).ToList());
            foreach (var item in data)
            {
                var values = propertyInfos.Select(i => i.GetValue(item)).ToList();
                result.Add(values);
            }

            return result;
        }

        public static AsciiTableGenerator FromDataTable(DataTable table, Options options = null)
        {
            var result = new AsciiTableGenerator(options);
            foreach (var item in table.Columns.Cast<DataColumn>()
                         .Select((value, index) => new { DataColumn = value, Index = index }))
            {
                AsciiDataColumn col = new AsciiDataColumn()
                {
                    DataType = item.DataColumn.DataType,
                    AllowDBNull = item.DataColumn.AllowDBNull,
                    DateTimeMode = item.DataColumn.DateTimeMode,
                    ColumnName = item.DataColumn.ColumnName
                };
                result._dataTable.Columns.Add(col);
            }


            result.SetCaptions(table.Columns.Cast<DataColumn>().Select(i => i.Caption).ToList());

            foreach (DataRow row in table.Rows)
            {
                result._dataTable.Rows.Add(row.ItemArray);
            }

            return result;
        }


        private List<T> ListFill<T>(int length, T fill)
        {
            var result = new List<T>();
            for (var i = 0; i < length; i++) result.Add(fill);
            return result;
        }

        public List<object> GetCaptions()
        {
            return GetColumns().Select(i => (object)i.Caption).ToList();
        }

        private string RenderTitle(int length)
        {
            var name = $" {Options.Title} ";
            var str = Align(Options.TitleCellAlignment, name, length - 1, ' ');
            return $"{_edge}{str}{_edge}";
        }

        public AsciiTableGenerator SetTitleAlignLeft()
        {
            return SetTitleAlign(CellAlignmentEnum.Left);
        }

        public AsciiTableGenerator SetTitleAlignRight()
        {
            return SetTitleAlign(CellAlignmentEnum.Right);
        }

        public AsciiTableGenerator SetDisplayHeader(bool value)
        {
            Options.DisplayCaptions = value;
            return this;
        }

        public AsciiTableGenerator SetTitleAlignCenter()
        {
            return SetTitleAlign(CellAlignmentEnum.Center);
        }

        public AsciiTableGenerator SetHeadingAlignLeft()
        {
            return SetHeadingAlign(CellAlignmentEnum.Left);
        }

        public AsciiTableGenerator SetHeadingAlignRight()
        {
            return SetHeadingAlign(CellAlignmentEnum.Right);
        }

        public AsciiTableGenerator SetHeadingAlignCenter()
        {
            return SetHeadingAlign(CellAlignmentEnum.Center);
        }

        public AsciiTableGenerator SetAlignLeft(int index)
        {
            return SetAlign(index, CellAlignmentEnum.Left);
        }

        public AsciiTableGenerator SetAlignRight(int index)
        {
            return SetAlign(index, CellAlignmentEnum.Right);
        }

        public AsciiTableGenerator SetAlignCenter(int index)
        {
            return SetAlign(index, CellAlignmentEnum.Center);
        }

        private string RenderRow(RenderInfo renderInfo, List<object> row, char padChar,
            bool isHeader,
            List<AsciiDataColumn> columns,
            CellAlignmentEnum? alignment = null)
        {
            var tmp = new List<string> { "" };

            for (var index = 0; index < renderInfo.CellCount; index++)
            {
                var length = _equalColumnSize ? renderInfo.MaxColumnLength : renderInfo.ColumnLengths[index];

                CellAlignmentEnum use;
                if (isHeader)
                {
                    var columnCaptionAlignment = columns[index].CaptionAlignment;
                    if (columnCaptionAlignment == CellAlignmentEnum.NotSpecified)
                    {
                        use = Options.CaptionCellAlignment == CellAlignmentEnum.NotSpecified
                            ? CellAlignmentEnum.Center
                            : Options.CaptionCellAlignment;
                    }
                    else
                    {
                        use = columnCaptionAlignment;
                    }
                    
                }
                else
                {
                    use = columns[index].CellAlignment;
                }

                AlignDelegate alignDelegate;
                switch (use)
                {
                    case CellAlignmentEnum.Center:
                        alignDelegate = AlignCenter;
                        break;
                    case CellAlignmentEnum.Right:
                        alignDelegate = AlignRight;
                        break;

                    case CellAlignmentEnum.Left:
                        alignDelegate = AlignLeft;
                        break;
                    default:
                        alignDelegate = AlignAuto;
                        break;
                }

                tmp.Add(alignDelegate(row[index], length, padChar));
            }

            var front = string.Join($"{padChar}{_edge}{padChar}", tmp);
            front = front.Substring(1);
            return $"{front}{padChar}{_edge}";
        }

        private string GetRowSeparator(RenderInfo renderInfo, List<AsciiDataColumn> columns)
        {
            var blanks = ListFill<object>(renderInfo.CellCount, _fill);
            return RenderRow(renderInfo, blanks, _fill, false, columns);
        }

        public override string ToString()
        {
            var cellCount = _dataTable.Columns.Count;
            var columns = GetColumns();
            var captions = GetCaptions();
            var rows = GetRows();
            var info = GetExtents(captions, rows, columns);
            var totalWidth = info.CellCount * 3;

            var resultLines = new List<string>();
            var justify = _equalColumnSize ? info.MaxColumnLength : 0;

            // Get 
            foreach (var cellWidth in info.ColumnLengths.Values)
                totalWidth += justify > 0 ? justify : cellWidth + _spacing;

            if (justify > 0) totalWidth += info.ColumnLengths.Count;

            totalWidth -= _spacing;

            if (!string.IsNullOrWhiteSpace(Options.Title) && totalWidth < Options.Title.Length + 2)
                totalWidth = Options.Title.Length + 2;

            // Heading
            if (_border) resultLines.Add(GetSeparator(totalWidth - info.CellCount + 1, _top));

            if (!string.IsNullOrWhiteSpace(Options.Title))
            {
                if (totalWidth < Options.Title.Length) totalWidth = Options.Title.Length;
                resultLines.Add(RenderTitle(totalWidth - info.CellCount + 1));
                if (_border) resultLines.Add(GetSeparator(totalWidth - info.CellCount + 1));
            }

            if (_hasCaptions)
            {
                resultLines.Add(RenderRow(info, captions, ' ', true, columns, Options.CaptionCellAlignment));
                resultLines.Add(GetRowSeparator(info, columns));
            }

            foreach (DataRow row in _dataTable.Rows)
            {
                resultLines.Add(RenderRow(info, row.ItemArray.ToList(), ' ', false, columns));
            }

            if (_border) resultLines.Add(GetSeparator(totalWidth - info.CellCount + 1, _bottom));

            var prefix = Options.Prefix ?? string.Empty;
            return prefix + string.Join($"{Environment.NewLine}{prefix}", resultLines);
        }

        private RenderInfo GetExtents(List<object> headings, List<List<object>> rows,
            List<AsciiDataColumn> columns)
        {
            var columnLengths = columns.Select((i, j) => j).ToDictionary(i => i, j => 0);

            if (Options.DisplayCaptions && _hasCaptions)
                foreach (var data in headings.Select((value, index) => new { Value = value, Index = index }))
                    columnLengths[data.Index] = data.Value == null ? 0 : data.Value.ToString().Length;

            // Calculate max table cell lengths across all rows
            foreach (var row in rows)
                for (var index = 0; index < columnLengths.Count; index++)
                {
                    var cell = row[index];
                    columnLengths[index] =
                        Math.Max(columnLengths[index],
                            cell != null && cell != DBNull.Value ? cell.ToString().Length : 0);
                }

            return new RenderInfo(columnLengths, columns.Count);
        }

        private string GetSeparator(int length, char? sep = null)
        {
            return $"{sep ?? _edge}{AlignRight(sep ?? _edge, length, _fill)}";
        }

        private static string AlignRight(object str, int length, char? padChar = ' ')
        {
            return (str ?? "").ToString().PadLeft(length, padChar ?? ' ');
        }

        public AsciiTableGenerator RemoveBorder()
        {
            _border = false;
            _edge = ' ';
            _fill = ' ';
            return this;
        }

        private AsciiDataColumn GetColumn(int index)
        {
            EnsureColumnsExist(index + 1);
            return _dataTable.Columns.Cast<AsciiDataColumn>().Skip(index).First();
        }

        public AsciiTableGenerator SetAlign(int index, CellAlignmentEnum cellAlignment)
        {
            GetColumn(index).CellAlignment = cellAlignment;

            return this;
        }

        public AsciiTableGenerator SetTitle(string name)
        {
            Options.Title = name;
            return this;
        }

        public string GetTitle()
        {
            return Options.Title;
        }

        private class RenderInfo
        {
            public RenderInfo(Dictionary<int, int> columnLengths, int cellCount)
            {
                MaxColumnLength = columnLengths.Any() ? columnLengths.Values.Max() : 0;
                ColumnLengths = columnLengths;
                CellCount = cellCount;
            }

            public int MaxColumnLength { get; }

            public Dictionary<int, int> ColumnLengths { get; }
            public int CellCount { get; }
        }

        private delegate string AlignDelegate(object value, int length, char? padChar);
    }
}