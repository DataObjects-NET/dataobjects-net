// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.01

using System;
using System.Buffers;
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
    #region Header build
    private static RecordSetHeader BuildHeader(CompilableProvider left, CompilableProvider right)
    {
      var leftHeader = left.Header;
      var rightHeader = right.Header;
      EnsureUnionIsPossible(leftHeader, rightHeader);
      var mappedColumnIndexes = new List<int>();
      // we can use shared array here and then fast-copy to be more memory-efficient and GC-friendly
      var rented = ArrayPool<Column>.Shared.Rent(Math.Max(leftHeader.Columns.Count, 64)); // reduce pool growth
      var lastIndex = 0;
      for (int i = 0; i < leftHeader.Columns.Count; i++) {
        var leftColumn = leftHeader.Columns[i];
        var rightColumn = rightHeader.Columns[i];
        if (leftColumn is MappedColumn leftMappedColumn && rightColumn is MappedColumn rightMappedColumn) {
          if (leftMappedColumn.ColumnInfoRef.Equals(rightMappedColumn.ColumnInfoRef)) {
            rented[lastIndex++] = leftColumn;
            mappedColumnIndexes.Add(i);
          }
          else {
            rented[lastIndex++] = new SystemColumn(leftColumn.Name, leftColumn.Index, leftColumn.Type);
          }
        }
        else {
          rented[lastIndex++] = new SystemColumn(leftColumn.Name, leftColumn.Index, leftColumn.Type);
        }
      }
      var columns = new Column[lastIndex];
      Array.Copy(rented, columns, lastIndex);
      ArrayPool<Column>.Shared.Return(rented, true); // not sure we can make it false, becasue 

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
        throw new InvalidOperationException(string.Format(Strings.ExXCantBeExecuted, "Union operation"));
      }
    }
    #endregion

    // Constructors

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