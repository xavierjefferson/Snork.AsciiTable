
# Ascii Table Generator

This library generates tables in Ascii from raw data, enumerables, and instances of System.Data.DataTable.  Within cells, it has text wrapping capability based on [Snork.TextWrap](#https://github.com/xavierjefferson/Snork.TextWrap)

[![Latest version](https://img.shields.io/nuget/v/Snork.AsciiTable.svg)](https://www.nuget.org/packages/Snork.AsciiTable/) 

## Table of Contents

 - [Example 1 - Populate table in code](#example-1)
 - [Example 2 - Populate table from enumerable](#example-2)
 - [API](#api)

### <a name="example-1">Example 1 - Populate table in code</a>

    var table = new AsciiTableGenerator("A Title");
    table.SetHeading("", "Name", "Age");
    table.Add(1, "Bob", 52).Add(2, "John", 34).Add(3, "Jim", 83);
    Console.Write(table.ToString());

### Example 1 output:

    .----------------.
    |    A Title     |
    |----------------|
    |   | Name | Age |
    |---|------|-----|
    | 1 | Bob  |  52 |
    | 2 | John |  34 |
    | 3 | Jim  |  83 |
    '----------------'

### <a name="example-2">Example 2 - Populate table from enumerable</a>

    class MyClass {
        public string Column1 { get; set; }
        public string Column2 { get; set; }
    }
    var items = new List<MyClass>
                {
                    new MyClass { Column1 = "Bozo", Column2 = "TheClown" },
                    new MyClass { Column1 = "Popeye", Column2 = "TheSailor" }
                };
    var table = AsciiTableGenerator.FromEnumerable(items);
    Console.Write(table.ToString());

### Example 2 Output:

    .---------------------.
    | Column1 |  Column2  |
    |---------|-----------|
    | Bozo    | TheClown  |
    | Popeye  | TheSailor |
    '---------------------'



## <a name="api">API</a>

```csharp
public class Snork.AsciiTable.AsciiTableGenerator

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Options` | Options |  | 


Methods

| Return Type | Name | Summary | 
| --- | --- | --- | 
| `AsciiTableGenerator` | Add(`IEnumerable<Object>` row) | Add a row of data | 
| `AsciiTableGenerator` | Add(`Object[]` row) | Add a row of data | 
| `AsciiTableGenerator` | AddRange(`IEnumerable<List<Object>>` rows) | Add a range of rows | 
| `AsciiTableGenerator` | Clear() | Clear current table data and reset settings to defaults | 
| `AsciiTableGenerator` | ClearRows() | Clear current table data | 
| `AsciiTableGenerator` | DisplayBorder(`Boolean` value) | Setting for whether or not to display the border around the cells | 
| `AsciiTableGenerator` | DisplayRowSeparators(`Boolean` value) | Display row separators between each row of data, for improved visibility | 
| `List<String>` | GetCaptions() | Get list of captions for all columns | 
| `List<List<Object>>` | GetRows() | Get current table data as list of list | 
| `String` | GetTitle() | Get the current title | 
| `AsciiTableGenerator` | SetBorder(`Nullable<Char>` horizontalEdge = null, `Nullable<Char>` verticalEdge = null, `Nullable<Char>` topCorner = null, `Nullable<Char>` bottomCorner = null) | Set the border characters for rendering, if no arguments are passed it will be reset to defaults. If a single edge  arg is passed, it will be used for all borders. | 
| `AsciiTableGenerator` | SetCaptionAlignment(`Int32` index, `CellAlignmentEnum` alignment) | Set the alignment of caption for a given column | 
| `AsciiTableGenerator` | SetCaptions(`List<String>` captions) | Set captions for all columns | 
| `AsciiTableGenerator` | SetCaptions(`String[]` captions) | Set captions for all columns | 
| `AsciiTableGenerator` | SetCellAlignment(`Int32` index, `CellAlignmentEnum` cellAlignment) | Set alignment for cells in a given column | 
| `AsciiTableGenerator` | SetColumnWidth(`Int32` index, `ColumnWidthTypeEnum` columnWidthType, `Nullable<Int32>` width = null) | Set width for a given column, by index.  ColumnWidthType can be Fixed or Auto | 
| `AsciiTableGenerator` | SetDisplayCaptions(`Boolean` value) | Setting for whether captions are displayed or not | 
| `AsciiTableGenerator` | SetTextWrapperOptions(`Int32` index, `Action<TextWrapperOptions>` action) | Set text wrapping options for a particular column with options from [Snork.TextWrap library] (https://github.com/xavierjefferson/Snork.TextWrap) | 
| `AsciiTableGenerator` | SetTitle(`String` name) | Set title for the table.  Will be rendered in single cell that spans all columns | 
| `AsciiTableGenerator` | SetTitleAlignment(`CellAlignmentEnum` alignment) | Set alignment for title cell | 
| `String` | ToString() | Render the table | 


Static Methods

| Return Type | Name | Summary | 
| --- | --- | --- | 
| `AsciiTableGenerator` | FromDataTable(`DataTable` table, `Options` options = null, `Boolean` autoCaptions = True) | Create an AsciiTableGenerator instance with a datatable as its source | 
| `AsciiTableGenerator` | FromEnumerable(`IEnumerable<T>` data, `Options` options = null, `Boolean` autoCaptions = True) | Create an AsciiTableGenerator instance with an enumerable of some type as its source | 


## `CellAlignmentEnum`

```csharp
public enum Snork.AsciiTable.CellAlignmentEnum
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | NotSpecified |  | 
| `1` | Left |  | 
| `2` | Right |  | 
| `3` | Center |  | 


## `ColumnWidthTypeEnum`

```csharp
public enum Snork.AsciiTable.ColumnWidthTypeEnum
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Auto |  | 
| `1` | Fixed |  | 


## `Options`

```csharp
public class Snork.AsciiTable.Options

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Char` | BottomCorner |  | 
| `CellAlignmentEnum` | CaptionCellAlignment | Cell alignment for captions | 
| `Boolean` | DisplayBorder | Show border around table | 
| `Boolean` | DisplayCaptions | Show captions with a row separator | 
| `Boolean` | DisplayRowSeparators | Add row separators between each row of data | 
| `Char` | HorizontalEdge |  | 
| `String` | LinePrefix | Prefix to add to each line on render | 
| `String` | Title | Table title.  Defaults to null | 
| `CellAlignmentEnum` | TitleCellAlignment | Cell alignment for title, if set | 
| `Char` | TopCorner |  | 
| `Char` | VerticalEdge |  | 




