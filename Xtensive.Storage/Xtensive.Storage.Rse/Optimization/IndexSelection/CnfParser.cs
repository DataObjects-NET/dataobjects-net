// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Linq.Normalization;
using Xtensive.Indexing;
using Xtensive.Core.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  /// <summary>
  /// Extractor of <see cref="RangeSet{T}"/> from a boolean expression in a conjunctive normal form.
  /// </summary>
  internal sealed class CnfParser
  {
    private IndexInfo indexInfo;
    private RecordSetHeader recordSetHeader;
    private readonly DomainModel domainModel;
    private const int defaultExpressionsListSize = 10;
    private readonly List<RangeSetInfo> extractedExpressions =
      new List<RangeSetInfo>(defaultExpressionsListSize);
    private readonly Dictionary<int, Expression> indexKeyValues =
      new Dictionary<int, Expression>(defaultExpressionsListSize);

    private readonly Comparison<RangeSetInfo> cashedComparison =
      (r1, r2) =>
      {
        if (r1.Origin != null && r2.Origin != null)
          return r1.Origin.FieldIndex.CompareTo(r2.Origin.FieldIndex);
        else {
          if (r1.Origin == null && r2.Origin == null)
            return 0;
          if (r2.Origin == null)
            return -1;
          return 1;
        }
      };

    public RangeSetInfo Parse(Conjunction<Expression> normalized, IndexInfo info,
      RecordSetHeader primaryIdxRecordSetHeader)
    {
      ArgumentValidator.EnsureArgumentNotNull(normalized, "normalized");
      ArgumentValidator.EnsureArgumentNotNull(info, "info");
      ArgumentValidator.EnsureArgumentNotNull(primaryIdxRecordSetHeader, "primaryIdxRecordSetHeader");
      indexInfo = info;
      recordSetHeader = primaryIdxRecordSetHeader;
      ParseTerms(normalized);
      return MakeResultExpression();
    }

    private void ParseTerms(Conjunction<Expression> normalized)
    {
      extractedExpressions.Clear();
      var extractor = new ComparisonExtractor();
      Func<Expression, bool> keySelector = exp => exp.AsTupleAccess() != null;
      foreach (var exp in normalized.Operands) {
        var tupleComparison = extractor.Extract(exp, keySelector);
        extractedExpressions.Add(ConvertToRangeSetExpression(exp, tupleComparison, keySelector));
      }
    }

    private bool IndexHasKeyAtZeroPoisition(int tupleFieldIndex)
    {
      return IndexHasKeyAtSpecifiedPoisition(tupleFieldIndex, 0);
    }

    private bool IndexHasKeyAtSpecifiedPoisition(int tupleFieldPosition, int indexFieldPosition)
    {
      var mappedColumn = recordSetHeader.Columns[tupleFieldPosition] as MappedColumn;
      if (mappedColumn == null)
        return false;

      var mappedColumnInfo = mappedColumn.ColumnInfoRef.Resolve(domainModel);
      return indexInfo.KeyColumns.Count > indexFieldPosition &&
        mappedColumnInfo.Equals(indexInfo.KeyColumns[indexFieldPosition].Key);
    }

    private RangeSetInfo ConvertToRangeSetExpression(Expression operand, ComparisonInfo tupleComparison,
      Func<Expression, bool> keySelector)
    {
      if (tupleComparison == null)
        if (ComparisonExtractor.ContainsKey(operand, keySelector))
          return RangeSetExpressionBuilder.BuildFullRangeSetConstructor(null);
        else
          return RangeSetExpressionBuilder.BuildFullOrEmpty(operand);
      int fieldIndex = tupleComparison.Key.GetTupleAccessArgument();
      var tupleExp = new TupleFieldInfo(fieldIndex, tupleComparison);
      if(IndexHasKeyAtZeroPoisition(fieldIndex))
        return RangeSetExpressionBuilder.BuildConstructor(tupleExp, indexInfo);
      return RangeSetExpressionBuilder.BuildFullRangeSetConstructor(tupleExp);
    }

    private RangeSetInfo MakeResultExpression()
    {
      RangeSetInfo result = TryUseMultiFieldIndex();
      if (result != null)
        return result;
      foreach (var rangeSet in extractedExpressions) {
        if (result == null)
          result = rangeSet;
        else
          result = RangeSetExpressionBuilder.BuildIntersect(rangeSet, result);
      }
      return result;
    }

    private RangeSetInfo TryUseMultiFieldIndex()
    {
      extractedExpressions.Sort(cashedComparison);
      int lastFieldPosition = -1;
      foreach (var rangeSet in extractedExpressions) {
        if (rangeSet.Origin == null)
          break;
        bool presentInIndex = IndexHasKeyAtSpecifiedPoisition(rangeSet.Origin.FieldIndex,
          lastFieldPosition + 1);
        if (!presentInIndex)
          break;

        if (rangeSet.Origin.Comparison.Operation == ComparisonOperation.Equal)
          lastFieldPosition++;
        else {
          lastFieldPosition++;
          break;
        }
      }

      if (lastFieldPosition <= 0)
        return null;

      indexKeyValues.Clear();
      for (int i = 0; i <= lastFieldPosition; i++)
        indexKeyValues.Add(i, extractedExpressions[i].Origin.Comparison.Value);

      return RangeSetExpressionBuilder.BuildConstructor(indexKeyValues,
        extractedExpressions[lastFieldPosition].Origin, indexInfo);
    }

    // Constructors

    public CnfParser(DomainModel domainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      this.domainModel = domainModel;
    }
  }
}