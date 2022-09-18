
# Ascii Table Generator

This library generates tables in Ascii from raw data, enumerables, and instances of System.Data.DataTable.

[![Latest version](https://img.shields.io/nuget/v/Snork.AsciiTable.svg)](https://www.nuget.org/packages/Snork.AsciiTable/) 

## Table of Contents

 - [Example 1 - Populate table in code](#example-1)
 - [Example 2 - Populate table from enumerable](#example-2)
 - API

### <a name="example-1">Example 1 - Populate table in code</a>

    var table = new AsciiTableGenerator("My Title");
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



