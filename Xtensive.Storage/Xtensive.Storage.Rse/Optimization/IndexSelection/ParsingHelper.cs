// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.03

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  internal class ParsingHelper
  {
    public static readonly Func<Expression, bool> DeafultKeySelector = exp => exp.AsTupleAccess() != null;
    
    private readonly DomainModel domainModel;

    public RangeSetInfo ConvertToRangeSetInfo(Expression exp, ComparisonInfo tupleComparison,
      IndexInfo indexInfo, RecordSetHeader recordSetHeader)
    {
      // The validation of arguments is omitted to increase performance.
      if (tupleComparison == null)
        if (ComparisonExtractor.ContainsKey(exp, DeafultKeySelector))
          return RangeSetExpressionBuilder.BuildFullRangeSetConstructor(null);
        else
          return RangeSetExpressionBuilder.BuildFullOrEmpty(exp);
      int fieldIndex = tupleComparison.Key.GetTupleAccessArgument();
      var tupleExp = new TupleExpressionInfo(fieldIndex, tupleComparison);
      if (IndexHasKeyAtZeroPoisition(fieldIndex, indexInfo, recordSetHeader))
        return RangeSetExpressionBuilder.BuildConstructor(tupleExp, indexInfo);
      return RangeSetExpressionBuilder.BuildFullRangeSetConstructor(tupleExp);
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

    private bool IndexHasKeyAtZeroPoisition(int tupleFieldIndex, IndexInfo indexInfo,
      RecordSetHeader recordSetHeader)
    {
      return IndexHasKeyAtSpecifiedPoisition(tupleFieldIndex, 0, indexInfo, recordSetHeader);
    }

    // Constructors

    public ParsingHelper(DomainModel domainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      this.domainModel = domainModel;
    }
  }
}