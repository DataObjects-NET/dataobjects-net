// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.09

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests
{
  public static class IndexInfoExtensions
  {
    private const int IndentMultiplier = 2;

    public static void Dump(this IndexInfo index)
    {
      Console.WriteLine("----------------------------------------");
      Console.WriteLine("Index dump");
      Console.WriteLine("----------------------------------------");
      Dump(index, 0);
    }

    public static void Dump(IndexInfo index, int indent)
    {
      Console.Out.WriteLine("");
      WriteLine("Name: " + index.Name, indent);
      WriteLine("Attributes: " + index.Attributes, indent);
      WriteLine("ReflectedType: " + index.ReflectedType.Name, indent);
      WriteLine("Columns: " + index.Columns.Select(p => p.Name).ToCommaDelimitedString(), indent);
      if (index.IsVirtual) {
        if ((index.Attributes & IndexAttributes.Filtered) == IndexAttributes.Filtered)
          WriteLine("FilterByTypes: " + index.FilterByTypes.ToCommaDelimitedString(), indent);
        else if ((index.Attributes & IndexAttributes.View) == IndexAttributes.View)
          WriteLine("SelectColumns: " + index.SelectColumns.ToCommaDelimitedString(), indent);
        WriteLine("BaseIndexes: ", indent);
        foreach (IndexInfo baseIndex in index.UnderlyingIndexes)
          Dump(baseIndex, indent + 1);
      }
    }

    private static void WriteLine(string text, int indent)
    {
      Console.Out.Write(Enumerable.Repeat(' ', indent * IndentMultiplier).ToArray());
      Console.Out.WriteLine(text);
    }
  }
}