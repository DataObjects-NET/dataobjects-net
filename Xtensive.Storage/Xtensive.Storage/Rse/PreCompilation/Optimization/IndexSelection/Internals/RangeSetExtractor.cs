// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.17

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq.Normalization;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  internal sealed class RangeSetExtractor
  {
    private readonly CnfParser cnfParser;
    private readonly GeneralPredicateParser generalParser;

    public Dictionary<Expression, List<RsExtractionResult>> Extract(DisjunctiveNormalized predicate,
      IEnumerable<IndexInfo> indexes, RecordSetHeader primaryIdxRecordSetHeader)
    {
      ArgumentValidator.EnsureArgumentNotNull(predicate, "predicate");
      ArgumentValidator.EnsureArgumentNotNull(indexes, "indexes");
      ArgumentValidator.EnsureArgumentNotNull(primaryIdxRecordSetHeader, "primaryIdxRecordSetHeader");
      predicate.Validate();
      var result = new Dictionary<Expression, List<RsExtractionResult>>();
      var indexCount = indexes.Count();
      foreach (var operand in predicate.Operands) {
        var expressionPart = operand.ToExpression();
        var rangeSets = ProcessExpressionPart(operand, indexes, indexCount, primaryIdxRecordSetHeader);
        result.Add(expressionPart, rangeSets);
      }
      return result;
    }

    public Dictionary<Expression, List<RsExtractionResult>> Extract(Expression predicate,
      IEnumerable<IndexInfo> indexes, RecordSetHeader primaryIdxRecordSetHeader)
    {
      ArgumentValidator.EnsureArgumentNotNull(predicate, "predicate");
      ArgumentValidator.EnsureArgumentNotNull(indexes, "indexes");
      ArgumentValidator.EnsureArgumentNotNull(primaryIdxRecordSetHeader, "primaryIdxRecordSetHeader");
      var result = new Dictionary<Expression, List<RsExtractionResult>>();
      var indexCount = indexes.Count();
      var extractionResult = ProcessExpression(predicate, indexes, indexCount, primaryIdxRecordSetHeader);
      result.Add(predicate, extractionResult);
      return result;
    }

    #region Private \ internal methods
    private List<RsExtractionResult> ProcessExpressionPart(Conjunction<Expression> part,
      IEnumerable<IndexInfo> secondaryIndexes, int indexCount, RecordSetHeader rsHeader)
    {
      var result = new List<RsExtractionResult>(indexCount);
      foreach (var info in secondaryIndexes) {
        var resultPart = cnfParser.Parse(part, info, rsHeader);
        var extractionResult = new RsExtractionResult(info, resultPart);
        result.Add(extractionResult);
      }
      return result;
    }

    private List<RsExtractionResult> ProcessExpression(Expression exp,
      IEnumerable<IndexInfo> secondaryIndexes, int indexCount, RecordSetHeader rsHeader)
    {
      var result = new List<RsExtractionResult>(indexCount);
      foreach (var info in secondaryIndexes) {
        var resultPart = generalParser.Parse(exp, info, rsHeader);
        var extractionResult = new RsExtractionResult(info, resultPart);
        result.Add(extractionResult);
      }
      return result;
    }
    #endregion


    // Constructors

    public RangeSetExtractor(DomainModel domainModel, IOptimizationInfoProviderResolver comparerResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      ArgumentValidator.EnsureArgumentNotNull(comparerResolver, "comparerResolver");
      cnfParser = new CnfParser(domainModel, comparerResolver);
      generalParser = new GeneralPredicateParser(domainModel, comparerResolver);
    }
  }
}