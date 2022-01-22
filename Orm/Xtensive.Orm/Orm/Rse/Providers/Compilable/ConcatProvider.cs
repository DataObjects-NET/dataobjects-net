// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.01

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Produces concatenation between <see cref="BinaryProvider.Left"/> and 
  /// <see cref="BinaryProvider.Right"/> sources.
  /// </summary>
  [Serializable]
  public sealed class ConcatProvider : BinaryProvider
  {


    // Constructors

    private static RecordSetHeader BuildHeader(CompilableProvider left, CompilableProvider right)
    {
      var leftHeader = left.Header;
      var rightHeader = right.Header;
      EnsureConcatIsPossible(leftHeader, rightHeader);
      var mappedColumnIndexes = new List<int>();
      var columns = new List<Column>();
      for (int i = 0; i < leftHeader.Columns.Count; i++) {
        var leftColumn = leftHeader.Columns[i];
        var rightColumn = rightHeader.Columns[i];
        if (leftColumn is MappedColumn leftMappedColumn && rightColumn is MappedColumn rightMappedColumn) {
          if (leftMappedColumn.ColumnInfoRef.Equals(rightMappedColumn.ColumnInfoRef)) {
            columns.Add(leftMappedColumn);
            mappedColumnIndexes.Add(i);
          }
          else {
            columns.Add(new SystemColumn(leftColumn.Name, leftColumn.Index, leftColumn.Type));
          }
        }
        else {
          columns.Add(new SystemColumn(leftColumn.Name, leftColumn.Index, leftColumn.Type));
        }
      }
      var columnGroups = leftHeader.ColumnGroups.Where(cg => cg.Keys.All(mappedColumnIndexes.Contains)).ToList();

      return new RecordSetHeader(
        leftHeader.TupleDescriptor,
        columns,
        columnGroups,
        null,
        null);
    }

    private static void EnsureConcatIsPossible(RecordSetHeader leftHeader, RecordSetHeader rightHeader)
    {
      var left = leftHeader.TupleDescriptor;
      var right = rightHeader.TupleDescriptor;
      if (!left.Equals(right)) {
        throw new InvalidOperationException(String.Format(Strings.ExXCantBeExecuted, "Concatenation"));
      }
    }

    /// <summary>
    ///  Initializes a new instance of this class.
    /// </summary>
    /// <param name="left">The left provider to intersect.</param>
    /// <param name="right">The right provider to intersect.</param>
    public ConcatProvider(CompilableProvider left, CompilableProvider right)
      : base(ProviderType.Concat, BuildHeader(left, right), left, right)
    {
    }
  }
}