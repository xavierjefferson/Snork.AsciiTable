using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Snork.AsciiTable.Tests
{

    public class AsciiGeneratorTests
    {
        private static readonly List<TestInfo> Items = new List<TestInfo>()
        {
            //new() { Field2 = "Im Field2 row1 tmp", Field3 = "Im Field3 row2 longest" },

            new TestInfo { Field2 = "Im Field2 row3 longer", Field3 = 1e6 },
            new TestInfo { Field2 = "Im Field2 row2 longer", Field3 = "Im Field3 row2 wow" },
            new TestInfo { Field2 = "Im Field2 row2 longer", Field3 = null }
        };

        [Fact]
        public void TestEmptyWithPrefix()
        {
            var tmp = new AsciiTableGenerator(new Options { Title = "MyTableName", Prefix = "xxyy  " });
            var asString = tmp.ToString();
            Assert.NotNull(asString);
            Assert.Equal("MyTableName", tmp.GetTitle());
            Assert.Equal(@"xxyy  .-------------.
xxyy  | MyTableName |
xxyy  |-------------|
xxyy  '-------------'", asString);
        }

        [Fact]
        public void TestEmptyWithName()
        {
            var tmp = new AsciiTableGenerator(new Options { Title = "MyTableName" });
            var asString = tmp.ToString();
            Assert.NotNull(asString);
            Assert.Equal("MyTableName", tmp.GetTitle());
            Assert.Equal(@".-------------.
| MyTableName |
|-------------|
'-------------'", asString);
        }

        [Fact]
        public void TestEmpty()
        {
            var tmp = new AsciiTableGenerator();
            var asString = tmp.ToString();
            Assert.NotNull(asString);
            Assert.Equal(@"..
''", asString);
        }

        [Fact]
        public void TestBasic()
        {
            var tmp = new AsciiTableGenerator();
            tmp.SetHeading("Column1", "Column2");
            tmp.AddRange(new[] { new List<object> { "Value1", "Value2" } });
            tmp.Add(new List<object> { "Case1", "Case2" });
            tmp.Add(new[] { new TestInfo { Field3 = "abc", Field2 = "def" } },
                i => new List<object> { i.Field2, i.Field3 });
            tmp.SetAlignLeft(1).SetAlignRight(2);
            var asString = tmp.ToString();
            Assert.NotNull(asString);
            Assert.Equal(@".-------------------.
| Column1 | Column2 |
|---------|---------|
| Value1  | Value2  |
| Case1   | Case2   |
| def     | abc     |
'-------------------'", asString);
            var rows = tmp.GetRows();
            Assert.Equal(3, rows.Count);
            tmp.ClearRows();
            Assert.Empty(tmp.GetRows());
        }

        [Fact]
        public void TestBasic2()
        {
            var tmp = new AsciiTableGenerator().SetTitle("CookedMyGoose").Add("Leroy", "Skillet")
                .Add("E Pluribus", "Veni Vidi Vici")
                .SetAlignCenter(1);
            var asString = tmp.ToString();
            Assert.NotNull(asString);
            Assert.Equal(@".-----------------------------.
|        CookedMyGoose        |
|-----------------------------|
| Leroy      |     Skillet    |
| E Pluribus | Veni Vidi Vici |
'-----------------------------'", asString);
        }

        [Fact]
        public void TestFromIEnumerableWithTitleRight()
        {
            var tmp = AsciiTableGenerator.FromEnumerable(Items).SetTitle("MyTableName").SetTitleAlignRight()
                .SetHeadingAlignLeft();
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            Assert.Equal(@".--------------------------------------------.
|                                MyTableName |
|--------------------------------------------|
| Field2                | Field3             |
|-----------------------|--------------------|
| Im Field2 row3 longer |            1000000 |
| Im Field2 row2 longer | Im Field3 row2 wow |
| Im Field2 row2 longer |                    |
'--------------------------------------------'", rendered);
        }

        [Fact]
        public void TestFromIEnumerableWithTitleLeft()
        {
            var tmp = AsciiTableGenerator.FromEnumerable(Items).SetTitle("MyTableName").SetTitleAlignLeft()
                .SetHeadingAlignLeft();
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            var heading = tmp.GetHeading();
            Assert.NotNull(heading);
            Assert.NotEmpty(heading);
            Assert.Equal(nameof(TestInfo.Field2), heading[0]);
            Assert.Equal(nameof(TestInfo.Field3), heading[1]);
            Assert.Equal(@".--------------------------------------------.
| MyTableName                                |
|--------------------------------------------|
| Field2                | Field3             |
|-----------------------|--------------------|
| Im Field2 row3 longer |            1000000 |
| Im Field2 row2 longer | Im Field3 row2 wow |
| Im Field2 row2 longer |                    |
'--------------------------------------------'", rendered);
        }

        [Fact]
        public void TestFromIEnumerableWithTitleCenter()
        {
            var tmp = AsciiTableGenerator.FromEnumerable(Items).SetTitle("Ghost12").SetTitleAlignCenter()
                .SetHeadingAlignLeft();
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            Assert.Equal(@".--------------------------------------------.
|                   Ghost12                  |
|--------------------------------------------|
| Field2                | Field3             |
|-----------------------|--------------------|
| Im Field2 row3 longer |            1000000 |
| Im Field2 row2 longer | Im Field3 row2 wow |
| Im Field2 row2 longer |                    |
'--------------------------------------------'", rendered);
        }

        [Fact]
        public void TestFromDataTable()
        {
            var table = new DataTable();
            table.Columns.Add(nameof(TestInfo.Field2), typeof(object));
            table.Columns.Add(nameof(TestInfo.Field3), typeof(object));
            foreach (var m in Items) table.Rows.Add(m.Field2, m.Field3);
            var tmp = AsciiTableGenerator.FromDataTable(table).SetHeadingAlignLeft();
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            Assert.Equal(@".--------------------------------------------.
| Field2                | Field3             |
|-----------------------|--------------------|
| Im Field2 row3 longer |            1000000 |
| Im Field2 row2 longer | Im Field3 row2 wow |
| Im Field2 row2 longer |                    |
'--------------------------------------------'", rendered);
        }


        [Fact]
        public void TestFromIEnumerable()
        {
            var tmp = AsciiTableGenerator.FromEnumerable(Items).SetHeadingAlignLeft().SetDisplayHeader(true);
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            Assert.Equal(@".--------------------------------------------.
| Field2                | Field3             |
|-----------------------|--------------------|
| Im Field2 row3 longer |            1000000 |
| Im Field2 row2 longer | Im Field3 row2 wow |
| Im Field2 row2 longer |                    |
'--------------------------------------------'", rendered);
        }

        [Fact]
        public void TestFromIEnumerableNoHeader()
        {
            var tmp = AsciiTableGenerator.FromEnumerable(Items).SetHeadingAlignLeft().SetDisplayHeader(false);
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            Assert.Equal(@".--------------------------------------------.
| Field2                | Field3             |
|-----------------------|--------------------|
| Im Field2 row3 longer |            1000000 |
| Im Field2 row2 longer | Im Field3 row2 wow |
| Im Field2 row2 longer |                    |
'--------------------------------------------'", rendered);
        }

        [Fact]
        public void TestFromIEnumerableWithJustify()
        {
            var tmp = AsciiTableGenerator.FromEnumerable(Items).SetHeadingAlignLeft().SetEqualColumnSize(true);
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            Assert.Equal(@".-----------------------------------------------.
| Field2                | Field3                |
|-----------------------|-----------------------|
| Im Field2 row3 longer |               1000000 |
| Im Field2 row2 longer | Im Field3 row2 wow    |
| Im Field2 row2 longer |                       |
'-----------------------------------------------'", rendered);
        }

        [Fact]
        public void TestFromIEnumerableWithHeading0Left()
        {
            var tmp = AsciiTableGenerator.FromEnumerable(Items).SetHeadingAlignLeft();
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            Assert.Equal(@".--------------------------------------------.
| Field2                | Field3             |
|-----------------------|--------------------|
| Im Field2 row3 longer |            1000000 |
| Im Field2 row2 longer | Im Field3 row2 wow |
| Im Field2 row2 longer |                    |
'--------------------------------------------'", rendered);
        }

        [Fact]
        public void TestFromIEnumerableWithHeading0Right()
        {
            var tmp = AsciiTableGenerator.FromEnumerable(Items).SetHeadingAlignRight();
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            Assert.Equal(@".--------------------------------------------.
|                Field2 |             Field3 |
|-----------------------|--------------------|
| Im Field2 row3 longer |            1000000 |
| Im Field2 row2 longer | Im Field3 row2 wow |
| Im Field2 row2 longer |                    |
'--------------------------------------------'", rendered);
        }

        [Fact]
        public void TestFromIEnumerableWithHeading0Center()
        {
            var tmp = AsciiTableGenerator.FromEnumerable(Items).SetHeadingAlignCenter();
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            Assert.Equal(@".--------------------------------------------.
|         Field2        |       Field3       |
|-----------------------|--------------------|
| Im Field2 row3 longer |            1000000 |
| Im Field2 row2 longer | Im Field3 row2 wow |
| Im Field2 row2 longer |                    |
'--------------------------------------------'", rendered);
        }

        [Fact]
        public void TestFromIEnumerableNoBorder()
        {
            var tmp = AsciiTableGenerator.FromEnumerable(Items).RemoveBorder().SetHeadingAlignLeft();
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            Assert.Equal(@"  Field2                  Field3              
                                              
  Im Field2 row3 longer              1000000  
  Im Field2 row2 longer   Im Field3 row2 wow  
  Im Field2 row2 longer                       ", rendered);
        }

        [Fact]
        public void TestFromIEnumerableAlignCol1Right()
        {
            var tmp = AsciiTableGenerator.FromEnumerable(Items).SetAlignRight(1).SetHeadingAlignLeft();
            var rendered = tmp.ToString();
            Assert.NotNull(rendered);
            Assert.Equal(@".--------------------------------------------.
| Field2                | Field3             |
|-----------------------|--------------------|
| Im Field2 row3 longer |            1000000 |
| Im Field2 row2 longer | Im Field3 row2 wow |
| Im Field2 row2 longer |                    |
'--------------------------------------------'", rendered);
        }
    }
}