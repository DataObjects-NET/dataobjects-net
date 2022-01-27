// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Produces union between <see cref="BinaryProvider.Left"/> and 
  /// <see cref="BinaryProvider.Right"/> sources.
  /// </summary>
  [Serializable]
  public sealed class UnionProvider : BinaryProvider
  {

    // Constructors

    private static RecordSetHeader BuildHeader(CompilableProvider left, CompilableProvider right)
    {
      var leftHeader = left.Header;
      var rightHeader = right.Header;
      EnsureUnionIsPossible(leftHeader, rightHeader);
      var mappedColumnIndexes = new List<int>();
      var columns = new List<Column>();
      for (int i = 0; i < leftHeader.Columns.Count; i++) {
        var leftColumn = leftHeader.Columns[i];
        var rightColumn = rightHeader.Columns[i];
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
      var columnGroups = leftHeader.ColumnGroups.Where(cg => cg.Keys.All(mappedColumnIndexes.Contains)).ToList();

      return new RecordSetHeader(
        leftHeader.TupleDescriptor, 
        columns, 
        columnGroups,
        null,
        null);
    }

    private static void EnsureUnionIsPossible(RecordSetHeader leftHeader, RecordSetHeader rightHeader)
    {
      var left = leftHeader.TupleDescriptor;
      var right = rightHeader.TupleDescriptor;
      if (!left.Equals(right)) {
        throw new InvalidOperationException(String.Format(Strings.ExXCantBeExecuted, "Union operation"));
      }
    }

    /// <summary>
    ///  Initializes a new instance of this class.
    /// </summary>
    /// <param name="left">The left provider for union.</param>
    /// <param name="right">The right provider for union.</param>
    public UnionProvider(CompilableProvider left, CompilableProvider right)
      : base(ProviderType.Union, BuildHeader(left, right), left, right)
    {
    }
  }
}