// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.17

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Linq.Normalization;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  internal sealed class RangeSetExtractor
  {
    private readonly CnfParser cnfParser;
    private readonly GeneralPredicateParser generalParser;

    public Dictionary<Expression, List<RSExtractionResult>> Extract(DisjunctiveNormalized predicate,
      IEnumerable<IndexInfo> secondaryIndexes, RecordSetHeader primaryIdxRecordSetHeader)
    {
      ArgumentValidator.EnsureArgumentNotNull(predicate, "predicate");
      ArgumentValidator.EnsureArgumentNotNull(secondaryIndexes, "secondaryIndexes");
      ArgumentValidator.EnsureArgumentNotNull(primaryIdxRecordSetHeader, "primaryIdxRecordSetHeader");
      predicate.Validate();
      var result = new Dictionary<Expression, List<RSExtractionResult>>();
      var indexCount = secondaryIndexes.Count();
      foreach (var operand in predicate.Operands) {
        var expressionPart = operand.ToExpression();
        var rangeSets = ProcessExpressionPart(operand, secondaryIndexes, indexCount, primaryIdxRecordSetHeader);
        result.Add(expressionPart, rangeSets);
      }
      return result;
    }

    public Dictionary<Expression, List<RSExtractionResult>> Extract(Expression predicate,
      IEnumerable<IndexInfo> secondaryIndexes, RecordSetHeader primaryIdxRecordSetHeader)
    {
      ArgumentValidator.EnsureArgumentNotNull(predicate, "predicate");
      ArgumentValidator.EnsureArgumentNotNull(secondaryIndexes, "secondaryIndexes");
      ArgumentValidator.EnsureArgumentNotNull(primaryIdxRecordSetHeader, "primaryIdxRecordSetHeader");
      var result = new Dictionary<Expression, List<RSExtractionResult>>();
      var indexCount = secondaryIndexes.Count();
      var extractionResult = ProcessExpression(predicate, secondaryIndexes, indexCount, primaryIdxRecordSetHeader);
      result.Add(predicate, extractionResult);
      return result;
    }

    private List<RSExtractionResult> ProcessExpressionPart(Conjunction<Expression> part,
      IEnumerable<IndexInfo> secondaryIndexes, int indexCount, RecordSetHeader rsHeader)
    {
      var result = new List<RSExtractionResult>(indexCount);
      foreach (var info in secondaryIndexes) {
        var resultPart = cnfParser.Parse(part, info, rsHeader);
        var extractionResult = new RSExtractionResult(info, resultPart);
        result.Add(extractionResult);
      }
      return result;
    }

    private List<RSExtractionResult> ProcessExpression(Expression exp,
      IEnumerable<IndexInfo> secondaryIndexes, int indexCount, RecordSetHeader rsHeader)
    {
      var result = new List<RSExtractionResult>(indexCount);
      foreach (var info in secondaryIndexes) {
        var resultPart = generalParser.Parse(exp, info, rsHeader);
        var extractionResult = new RSExtractionResult(info, resultPart);
        result.Add(extractionResult);
      }
      return result;
    }


    // Constructors

    public RangeSetExtractor(DomainModel domainModel)
    {
      cnfParser = new CnfParser(domainModel);
      generalParser = new GeneralPredicateParser(domainModel);
    }
  }
}