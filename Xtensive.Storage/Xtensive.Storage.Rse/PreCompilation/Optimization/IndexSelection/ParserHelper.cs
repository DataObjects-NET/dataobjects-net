// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.03

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Expressions;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  internal sealed class ParserHelper
  {
    public static readonly Func<Expression, bool> DeafultKeySelector = exp => exp.AsTupleAccess() != null;
    
    private readonly DomainModel domainModel;
    private readonly ComparisonExtractor comparisonExtractor = new ComparisonExtractor();

    public RangeSetInfo ConvertToRangeSetInfo(Expression exp, ComparisonInfo tupleComparison,
      IndexInfo indexInfo, RecordSetHeader recordSetHeader, AdvancedComparer<Entire<Tuple>> comparer)
    {
      // The validation of arguments is omitted to increase performance.
      if (tupleComparison == null)
        if (comparisonExtractor.ContainsKey(exp, DeafultKeySelector))
          return RangeSetExpressionBuilder.BuildFullRangeSetConstructor(null, comparer);
        else
          return RangeSetExpressionBuilder.BuildFullOrEmpty(exp);
      if(tupleComparison.IsComplex)
        return RangeSetExpressionBuilder.BuildFullRangeSetConstructor(null, comparer);
      int fieldIndex = tupleComparison.Key.GetTupleAccessArgument();
      var tupleExp = new TupleExpressionInfo(fieldIndex, tupleComparison);
      if (IndexHasKeyAtZeroPoisition(fieldIndex, indexInfo, recordSetHeader))
        return RangeSetExpressionBuilder.BuildConstructor(tupleExp, indexInfo, comparer);
      return RangeSetExpressionBuilder.BuildFullRangeSetConstructor(tupleExp, comparer);
    }

    public bool IndexHasKeyAtSpecifiedPoisition(int tupleFieldPosition, int indexFieldPosition,
      IndexInfo indexInfo, RecordSetHeader recordSetHeader)
    {
      // The validation of arguments is omitted to increase performance.
      var mappedColumn = recordSetHeader.Columns[tupleFieldPosition] as MappedColumn;
      if (mappedColumn == null)
        return false;

      var mappedColumnInfo = mappedColumn.ColumnInfoRef.Resolve(domainModel);
      return indexInfo.KeyColumns.Count > indexFieldPosition &&
        mappedColumnInfo.Equals(indexInfo.KeyColumns[indexFieldPosition].Key);
    }

    public int GetPositionInIndexKeys(Column column, IndexInfo indexInfo)
    {
      var mappedColumn = column as MappedColumn;
      if (mappedColumn == null)
        return -1;
      var columnInfo = mappedColumn.ColumnInfoRef.Resolve(domainModel);
      var index = 0;
      foreach (KeyValuePair<ColumnInfo, Direction> pair in indexInfo.KeyColumns) {
        if (columnInfo.Equals(pair.Key))
          return index;
        index++;
      }
      return -1;
    }

    #region Private \ internal methods
    private bool IndexHasKeyAtZeroPoisition(int tupleFieldIndex, IndexInfo indexInfo,
      RecordSetHeader recordSetHeader)
    {
      return IndexHasKeyAtSpecifiedPoisition(tupleFieldIndex, 0, indexInfo, recordSetHeader);
    }
    #endregion

    // Constructors

    public ParserHelper(DomainModel domainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      this.domainModel = domainModel;
    }
  }
}