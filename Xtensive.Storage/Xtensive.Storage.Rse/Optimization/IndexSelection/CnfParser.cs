// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.23

using System.Collections.Generic;
using System.Linq;
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
    private readonly ParserHelper parserHelper;
    private const int defaultExpressionsListSize = 10;

    private readonly List<RangeSetInfo> extractedExpressions =
      new List<RangeSetInfo>(defaultExpressionsListSize);

    private readonly List<Pair<int, RangeSetInfo>> rangeSetAndIndexKeys = 
      new List<Pair<int, RangeSetInfo>>(defaultExpressionsListSize);

    private readonly Dictionary<int, Expression> indexKeyValues =
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
      ParseTerms(normalized);
      return MakeResultExpression();
    }

    private void ParseTerms(Conjunction<Expression> normalized)
    {
      extractedExpressions.Clear();
      foreach (var exp in normalized.Operands) {
        var tupleComparison = extractor.Extract(exp, ParserHelper.DeafultKeySelector);
        extractedExpressions.Add(parserHelper.ConvertToRangeSetInfo(exp, tupleComparison, indexInfo,
          recordSetHeader));
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
      var rangeSetProjection = (from rangeSetInfo in extractedExpressions
      where rangeSetInfo.Origin!=null
      select new Pair<int, RangeSetInfo>(parserHelper.GetPositionInIndexKeys(
        recordSetHeader.Columns[rangeSetInfo.Origin.FieldIndex], indexInfo),rangeSetInfo));
      rangeSetAndIndexKeys.Clear();
      rangeSetAndIndexKeys.AddRange(rangeSetProjection);
      rangeSetAndIndexKeys.Sort((pair0, pair1) => pair0.First.CompareTo(pair1.First));

      indexKeyValues.Clear();
      RangeSetInfo lastRangeSetInfo = null;
      foreach (var item in rangeSetAndIndexKeys) {
        if (item.First < 0)
          break;
        indexKeyValues.Add(item.First, item.Second.Origin.Comparison.Value);
        lastRangeSetInfo = item.Second;
        if (item.Second.Origin.Comparison.Operation!=ComparisonOperation.Equal)
          break;
      }
      if (indexKeyValues.Count <= 1 || lastRangeSetInfo == null)
        return null;

      return RangeSetExpressionBuilder.BuildConstructor(indexKeyValues,
        lastRangeSetInfo.Origin, indexInfo);
    }


    // Constructors

    public CnfParser(DomainModel domainModel)
    {
      parserHelper = new ParserHelper(domainModel);
    }
  }
}