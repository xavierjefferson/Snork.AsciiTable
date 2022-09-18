using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Snork.AsciiTable
{
    internal static class TypeHelper
    {
        public static bool HasNumericType(this object o)
        {
            if (o == null) return false;
            if (o is byte || o is sbyte || o is ushort || o is uint || o is ulong || o is short || o is int ||
                o is long || o is decimal || o is double || o is float) return true;
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }

    public enum CellAlignmentEnum
    {
        Left,
        Right,
        Center
    }


    public class AsciiTableGenerator
    {
        private readonly Dictionary<int, CellAlignmentEnum> _alignments = new Dictionary<int, CellAlignmentEnum>();

        private readonly List<List<object>> _rows = new List<List<object>>();
        private bool _border;
        private char _edge, _top, _bottom, _fill;

        private bool _equalColumnSize;
        private List<object> _heading;

        private int _spacing;


        public AsciiTableGenerator(Options options = null)
        {
            Options = options ?? new Options();
            Clear();
        }


        public Options Options { get; }

        public AsciiTableGenerator Clear()
        {
            _rows.Clear();
            _alignments.Clear();
            _spacing = 1;
            _heading = null;
            SetBorder();
            return this;
        }

        public List<List<object>> GetRows()
        {
            return _rows;
        }

        public AsciiTableGenerator ClearRows()
        {
            _rows.Clear();
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

        private static string Align(CellAlignmentEnum cellAlignment, object str, int length, char padChar)
        {
            switch (cellAlignment)
            {
                case CellAlignmentEnum.Left:
                    return AlignLeft(str, length, padChar);
                case CellAlignmentEnum.Right:
                    return AlignRight(str, length, padChar);
                case CellAlignmentEnum.Center:
                    return AlignCenter(str, length, padChar);
            }

            return AlignAuto(str, length, padChar);
        }

        public AsciiTableGenerator SetTitleAlign(CellAlignmentEnum cellAlignment)
        {
            Options.TitleCellAlignment = cellAlignment;
            return this;
        }

        public AsciiTableGenerator SetBorder(char? edge = null, char? fill = null, char? top = null, char? bottom = null)
        {
            _border = true;
            if (fill == null && top == null && bottom == null) fill = top = bottom = edge;

            _edge = edge ?? '|';
            _fill = fill ?? '-';
            _top = top ?? '.';
            _bottom = bottom ?? '\'';
            return this;
        }

        private static string AlignLeft(object value, int length, char? padChar = ' ')
        {
            return (value ?? "").ToString().PadRight(length, padChar ?? ' ');
        }

        private static string AlignCenter(object value, int length, char? padChar = ' ')
        {
            var coalescedValue = (value ?? "").ToString();
            var allSpace = length - coalescedValue.Length;
            if (allSpace > 0)
            {
                var leftSpaceLeft = Convert.ToInt32(allSpace / 2.0);
                var rightSpaceLength = allSpace - leftSpaceLeft;
                return
                    $"{new string(padChar ?? ' ', leftSpaceLeft)}{coalescedValue}{new string(padChar ?? ' ', rightSpaceLength)}";
            }

            return coalescedValue;

            //return .PadRight(leftSpaceLeft, padChar ?? ' ').PadLeft(length, padChar ?? ' ');
        }

        private static string AlignAuto(object value, int length, char? padChar = ' ')
        {
            if (value == null) value = string.Empty;
            var asString = value.ToString();
            if (asString.Length < length)
                return value.HasNumericType() ? AlignRight(value, length, padChar) : AlignLeft(value, length, padChar);

            return asString;
        }

        public AsciiTableGenerator SetHeading(List<object> row)
        {
            _heading = row;
            return this;
        }

        public AsciiTableGenerator SetHeading(params object[] row)
        {
            _heading = row.ToList();
            return this;
        }

        public AsciiTableGenerator SetHeadingAlign(CellAlignmentEnum cellAlignment)
        {
            Options.HeadingCellAlignment = cellAlignment;
            return this;
        }

        public AsciiTableGenerator Add(IEnumerable<object> row)
        {
            _rows.Add(row.ToList());
            return this;
        }

        public AsciiTableGenerator Add(params object[] row)
        {
            _rows.Add(row.ToList());
            return this;
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

            result.SetHeading(propertyInfos.Select(i => (object)i.Name).ToList());
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


            result.SetHeading(table.Columns.Cast<DataColumn>().Select(i => (object)i.ColumnName).ToList());
            //foreach (DataRow row in table.Rows) result.Add(row.ItemArray);
            foreach (DataRow row in table.Rows)
            {
                List<object> items = new List<object>();
                for (var i = 0; i < table.Columns.Count;i++)
                {
                    items.Add(row[i]);
                }

                result.Add(items);
            }
            return result;
        }


    

        private List<T> ArrayFill<T>(int length, T fill)
        {
            var result = new List<T>();
            for (var i = 0; i < length; i++) result.Add(fill);

            return result;
        }

        public List<object> GetHeading()
        {
            return _heading.ToList();
        }

        private string _RenderTitle(int length)
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
            Options.DisplayHeader = value;
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

        private string RenderRow(RenderInfo renderInfo, List<object> row, char padChar, bool isHeader,
            CellAlignmentEnum? alignment = null)
        {
            var tmp = new List<string> { "" };

            for (var k = 0; k < renderInfo.CellCount; k++)
            {
                var length = _equalColumnSize ? renderInfo.MaxColumnLengths.Max() : renderInfo.MaxColumnLengths[k];

                CellAlignmentEnum? use;
                if (isHeader)
                    use = alignment ?? CellAlignmentEnum.Center;
                else
                    use = _alignments.ContainsKey(k) ? _alignments[k] : alignment;

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

                tmp.Add(alignDelegate(row[k], length, padChar));
            }

            var front = string.Join($"{padChar}{_edge}{padChar}", tmp);
            front = front.Substring(1);
            return $"{front}{padChar}{_edge}";
        }

        private string GetRowSeparator(RenderInfo renderInfo)
        {
            var blanks = ArrayFill<object>(renderInfo.CellCount, _fill);
            return RenderRow(renderInfo, blanks, _fill, false);
        }

        public override string ToString()
        {
            var all = Options.DisplayHeader && _heading != null && _heading.Any()
                ? new List<List<object>> { _heading }.Union(_rows).ToList()
                : _rows;
            var distinctCounts = all.Select(i => i.Count).Distinct().Count();

            if (distinctCounts > 1)
                throw new InvalidOperationException("All rows must have the same number of columns");
            var cellCount = all.Any() ? all.Select(i => i.Count).Max() : 0;
            var info = new RenderInfo
            {
                CellCount = cellCount,
                MaxColumnLengths = ArrayFill(cellCount, 0)
            };
            var body = new List<string>();

            var totalWidth = info.CellCount * 3;


            // Calculate max table cell lengths across all rows
            foreach (var row in all)
                for (var k = 0; k < info.CellCount; k++)
                {
                    var cell = row[k];
                    info.MaxColumnLengths[k] =
                        Math.Max(info.MaxColumnLengths[k], cell != null ? cell.ToString().Length : 0);
                }


            var justify = _equalColumnSize ? info.MaxColumnLengths.Max() : 0;

            // Get 
            foreach (var cellWidth in info.MaxColumnLengths) totalWidth += justify > 0 ? justify : cellWidth + _spacing;

            if (justify > 0) totalWidth += info.MaxColumnLengths.Count;

            totalWidth -= _spacing;

            if (!string.IsNullOrWhiteSpace(Options.Title) && totalWidth < Options.Title.Length + 2)
                totalWidth = Options.Title.Length + 2;

            // Heading
            if (_border) body.Add(GetSeparator(totalWidth - info.CellCount + 1, _top));

            if (!string.IsNullOrWhiteSpace(Options.Title))
            {
                if (totalWidth < Options.Title.Length) totalWidth = Options.Title.Length;
                body.Add(_RenderTitle(totalWidth - info.CellCount + 1));
                if (_border) body.Add(GetSeparator(totalWidth - info.CellCount + 1));
            }

            if (_heading != null && _heading.Any())
            {
                body.Add(RenderRow(info, _heading, ' ', true, Options.HeadingCellAlignment));
                body.Add(GetRowSeparator(info));
            }

            foreach (var row in _rows)
                body.Add(RenderRow(info, row, ' ', false));

            if (_border) body.Add(GetSeparator(totalWidth - info.CellCount + 1, _bottom));

            var prefix = Options.Prefix ?? string.Empty;
            return prefix + string.Join($"{Environment.NewLine}{prefix}", body);
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

        public AsciiTableGenerator SetAlign(int index, CellAlignmentEnum cellAlignment)
        {
            _alignments[index] = cellAlignment;
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
            public List<int> MaxColumnLengths { get; set; } = new List<int>();
            public int CellCount { get; set; }
        }

        private delegate string AlignDelegate(object value, int length, char? padChar);
    }

    public class Options
    {
        public CellAlignmentEnum HeadingCellAlignment { get; set; } = CellAlignmentEnum.Center;
        public CellAlignmentEnum TitleCellAlignment { get; set; } = CellAlignmentEnum.Center;
        public string Title { get; set; }
        public string Prefix { get; set; }
        public bool DisplayHeader { get; set; } = true;
    }

   
}