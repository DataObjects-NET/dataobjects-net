// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.17

using Xtensive.Core;
using Xtensive.Core.Linq.Normalization;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  /// <summary>
  /// Extractor of <see cref="RangeSet{T}"/> from a boolean expression in disjunctive normal form.
  /// </summary>
  internal sealed class RangeSetExtractor
  {
    private readonly CnfParser cnfParser;

    public CnfParsingResult Extract(DisjunctiveNormalized predicate, IndexInfo info,
      RecordSetHeader primaryIdxRecordSetHeader)
    {
      ArgumentValidator.EnsureArgumentNotNull(predicate, "predicate");
      var result = new CnfParsingResult(info);
      foreach (var cnf in predicate.Operands)
        result.AddPart(cnfParser.Parse(cnf, info, primaryIdxRecordSetHeader));
      return result;
    }

    // Constructors

    public RangeSetExtractor(DomainModel domainModel)
    {
      cnfParser = new CnfParser(domainModel);
    }
  }
}