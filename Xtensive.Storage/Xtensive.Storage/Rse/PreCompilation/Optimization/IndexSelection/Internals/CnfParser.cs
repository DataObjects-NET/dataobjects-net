// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.23

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Linq.Normalization;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  /// <summary>
  /// Extractor of <see cref="RangeSet{T}"/> from a boolean expression in a conjunctive normal form.
  /// </summary>
  internal sealed class CnfParser
  {
    private IndexInfo indexInfo;
    private RecordSetHeader recordSetHeader;
    private AdvancedComparer<Entire<Tuple>> comparer;
    private readonly ParserHelper parserHelper;
    private readonly IOptimizationInfoProviderResolver comparerResolver;
    private const int defaultExpressionsListSize = 10;

    private readonly List<RangeSetInfo> extractedExpressions =
      new List<RangeSetInfo>(defaultExpressionsListSize);

    private readonly List<Pair<int, RangeSetInfo>> rangeSetAndIndexKeysCache = 
      new List<Pair<int, RangeSetInfo>>(defaultExpressionsListSize);

    private readonly Dictionary<int, Expression> indexKeyValuesCache =
      new Dictionary<int, Expression>(defaultExpressionsListSize);

    private readonly ComparisonExtractor extractor = new ComparisonExtractor();

    public RangeSetInfo Parse(Conjunction<Expression> normalized, IndexInfo info,
      RecordSetHeader primaryIdxRecordSetHeader)
    {
      ArgumentValidator.EnsureArgumentNotNull(normalized, "normalized");
      ArgumentValidator.EnsureArgumentNotNull(info, "info");
      ArgumentValidator.EnsureArgumentNotNull(primaryIdxRecordSetHeader, "primaryIdxRecordSetHeader");
      indexInfo = info;
      recordSetHeader = primaryIdxRecordSetHeader;
      comparer = comparerResolver.Resolve(indexInfo).GetEntireKeyComparer();
      ParseTerms(normalized);
      return MakeResultExpression();
    }

    #region Private \ internal methods
    private void ParseTerms(Conjunction<Expression> normalized)
    {
      extractedExpressions.Clear();
      foreach (var exp in normalized.Operands) {
        var tupleComparison = extractor.Extract(exp, ParserHelper.DefaultKeySelector);
        extractedExpressions.Add(parserHelper.ConvertToRangeSetInfo(exp, tupleComparison, indexInfo,
          recordSetHeader, comparer));
      }
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
      var rangeSetAndIndexKeys = FindPositionOfIndexKeys();
      int lastKeyPosition;
      var indexKeyValues = TryCreateFirstSetOfMultiColumnIndexKeyValues(rangeSetAndIndexKeys,
        out lastKeyPosition);
      if (indexKeyValues == null)
        return null;
      return IntersectRangeSetsOfMultiColumnIndexKeys(rangeSetAndIndexKeys, indexKeyValues, lastKeyPosition);
    }

    private RangeSetInfo IntersectRangeSetsOfMultiColumnIndexKeys(
      List<Pair<int, RangeSetInfo>> rangeSetAndIndexKeys,
      Dictionary<int, Expression> indexKeyValues, int lastKeyPosition)
    {
      RangeSetInfo result = BuildRangeSetForMultiColumnIndex(indexKeyValues,
        rangeSetAndIndexKeys, lastKeyPosition);
      var indexOfLastField = rangeSetAndIndexKeys[lastKeyPosition].First;
      lastKeyPosition++;
      while (lastKeyPosition < rangeSetAndIndexKeys.Count
        && indexOfLastField == rangeSetAndIndexKeys[lastKeyPosition].First
          && rangeSetAndIndexKeys[lastKeyPosition].First >= 0) {
        indexKeyValues.Remove(indexOfLastField);
        var pair = rangeSetAndIndexKeys[lastKeyPosition];
        indexKeyValues.Add(pair.First, pair.Second.Origin.Comparison.Value);
        var part = BuildRangeSetForMultiColumnIndex(indexKeyValues, rangeSetAndIndexKeys, lastKeyPosition);
        result = RangeSetExpressionBuilder.BuildIntersect(result, part);
        lastKeyPosition++;
      }
      return result;
    }

    private RangeSetInfo BuildRangeSetForMultiColumnIndex(Dictionary<int, Expression> indexKeyValues,
      List<Pair<int, RangeSetInfo>> rangeSetAndIndexKeys, int lastKeyPosition)
    {
      return RangeSetExpressionBuilder.BuildConstructorForMultiColumnIndex(indexKeyValues,
        rangeSetAndIndexKeys[lastKeyPosition].Second.Origin, indexInfo, comparer);
    }

    private List<Pair<int, RangeSetInfo>> FindPositionOfIndexKeys()
    {
      var rangeSetProjection = (from rangeSetInfo in extractedExpressions
      where rangeSetInfo.Origin!=null
      select new Pair<int, RangeSetInfo>(parserHelper.GetPositionInIndexKeys(
        recordSetHeader.Columns[rangeSetInfo.Origin.FieldIndex], indexInfo),rangeSetInfo));
      rangeSetAndIndexKeysCache.Clear();
      rangeSetAndIndexKeysCache.AddRange(rangeSetProjection);
      rangeSetAndIndexKeysCache.Sort((pair0, pair1) => {
        if (pair0.First < 0 ^ pair1.First < 0)
          if (pair0.First < 0)
            return 1;
          else
            return -1;
        return pair0.First.CompareTo(pair1.First);
      });
      return rangeSetAndIndexKeysCache;
    }

    private Dictionary<int, Expression> TryCreateFirstSetOfMultiColumnIndexKeyValues(
      List<Pair<int, RangeSetInfo>> rangeSetAndIndexKeys, out int lastKeyPosition)
    {
      indexKeyValuesCache.Clear();
      lastKeyPosition = -1;
      for (int i = 0; i < rangeSetAndIndexKeys.Count; i++) {
        var item = rangeSetAndIndexKeys[i];
        if (item.First < 0)
          break;
        if (i != item.First || indexKeyValuesCache.ContainsKey(item.First))
          return null;
        indexKeyValuesCache.Add(item.First, item.Second.Origin.Comparison.Value);
        lastKeyPosition = i;
        if (IsNotAllowedComparison(item.Second.Origin.Comparison.Operation))
          return null;
        if (item.Second.Origin.Comparison.Operation != ComparisonOperation.Equal)
          break;
      }
      if (indexKeyValuesCache.Count <= 1 || lastKeyPosition < 0)
        return null;
      return indexKeyValuesCache;
    }

    private static bool IsNotAllowedComparison(ComparisonOperation operation)
    {
      return operation == ComparisonOperation.LikeStartsWith
        || operation == ComparisonOperation.NotLikeStartsWith
        || operation == ComparisonOperation.LikeEndsWith
        || operation == ComparisonOperation.NotLikeEndsWith;
    }

    #endregion


    // Constructors

    public CnfParser(DomainModel domainModel, IOptimizationInfoProviderResolver comparerResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      ArgumentValidator.EnsureArgumentNotNull(comparerResolver, "comparerResolver");
      parserHelper = new ParserHelper(domainModel);
      this.comparerResolver = comparerResolver;
    }
  }
}