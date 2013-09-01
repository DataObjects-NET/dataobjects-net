// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.01

using System;
using System.Collections.Generic;
using Xtensive.Collections;


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
    protected override RecordSetHeader BuildHeader()
    {
      EnsureConcatIsPossible();
      var mappedColumnIndexes = new List<int>();
      var columns = new List<Column>();
      for (int i = 0; i < Left.Header.Columns.Count; i++) {
        var leftColumn = Left.Header.Columns[i];
        var rightColumn = Right.Header.Columns[i];
        if (leftColumn is MappedColumn && rightColumn is MappedColumn) {
          var leftMappedColumn = (MappedColumn) leftColumn;
          var rightMappedColumn = (MappedColumn) rightColumn;
          if (leftMappedColumn.ColumnInfoRef.Equals(rightMappedColumn.ColumnInfoRef)) {
            columns.Add(leftMappedColumn);
            mappedColumnIndexes.Add(i);
            }
          else
            columns.Add(new SystemColumn(leftColumn.Name, leftColumn.Index, leftColumn.Type));
        }
        else
          columns.Add(new SystemColumn(leftColumn.Name, leftColumn.Index, leftColumn.Type));
      }
      var columnGroups = Left.Header.ColumnGroups.Where(cg => cg.Keys.All(mappedColumnIndexes.Contains)).ToList();

      return new RecordSetHeader(
        Left.Header.TupleDescriptor, 
        columns, 
        columnGroups,
        null,
        null);
    }

    /// <exception cref="InvalidOperationException">Something went wrong.</exception>
    private void EnsureConcatIsPossible()
    {
      var left = Left.Header.TupleDescriptor;
      var right = Right.Header.TupleDescriptor;
      if (!left.Equals(right))
        throw new InvalidOperationException(String.Format(Strings.ExXCantBeExecuted, "Concatenation"));
    }


    // Constructors

    /// <summary>
    ///  Initializes a new instance of this class.
    /// </summary>
    /// <param name="left">The left provider to intersect.</param>
    /// <param name="right">The right provider to intersect.</param>
    public ConcatProvider(CompilableProvider left, CompilableProvider right)
      : base(ProviderType.Concat, left, right)
    {
      Initialize();
    }
  }
}