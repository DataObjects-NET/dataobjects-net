// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.09

using System;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Tests
{
  internal static class IndexInfoExtensions
  {
    private const int IndentMultiplier = 2;

    public static void Dump(this IndexInfo index)
    {
      Console.WriteLine("----------------------------------------");
      Console.WriteLine("Index dump");
      Console.WriteLine("----------------------------------------");
      Dump(index, 0);
    }

    private static void Dump(IndexInfo index, int indent)
    {
      Console.Out.WriteLine("");
      WriteLine("Name: " + index.Name, indent);
      WriteLine("Attributes: " + index.Attributes, indent);
      WriteLine("ReflectedType: " + index.ReflectedType.Name, indent);
      if (index.IsVirtual) {
        if ((index.Attributes & IndexAttributes.Filtered) == IndexAttributes.Filtered)
          WriteLine("FilterByTypes: " + index.FilterByTypes.ToCommaDelimitedString(), indent);
        else if ((index.Attributes & IndexAttributes.View) == IndexAttributes.View)
          WriteLine("SelectColumns: " + index.SelectColumns.ToCommaDelimitedString(), indent);
        WriteLine("BaseIndexes: ", indent);
        foreach (IndexInfo baseIndex in index.UnderlyingIndexes)
          Dump(baseIndex, indent + 1);
      }
      WriteLine("KeyColumns: " + index.KeyColumns.Select(p => p.Key.Name).ToCommaDelimitedString(), indent);
      WriteLine("IncludedColumns: " + index.IncludedColumns.Select(p => p.Name).ToCommaDelimitedString(), indent);
      WriteLine("ValueColumns: " + index.ValueColumns.Select(p => p.Name).ToCommaDelimitedString(), indent);
    }

    private static void WriteLine(string text, int indent)
    {
      Console.Out.Write(Enumerable.Repeat(' ', indent * IndentMultiplier).ToArray());
      Console.Out.WriteLine(text);
    }
  }
}