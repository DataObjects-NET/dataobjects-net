// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.17

using Xtensive.Core;
using Xtensive.Core.Linq.Normalization;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimisation
{
  /// <summary>
  /// Extracter of <see cref="RangeSet{T}"/> from a boolean expression in disjunctive normal form.
  /// </summary>
  internal sealed class RangeSetExtractor
  {
    private readonly ExtractingVisitor cnfVisitor;

    public RsExtractionResult Extract(DisjunctiveNormalized predicate, IndexInfo info,
      RecordSetHeader primaryIdxRecordSetHeader)
    {
      ArgumentValidator.EnsureArgumentNotNull(predicate, "predicate");
      var result = new RsExtractionResult(info);
      foreach (var cnf in predicate.Operands) {
        result.AddPart(cnfVisitor.Extract(cnf, info, primaryIdxRecordSetHeader));
      }
      return result;
    }

    public RangeSetExtractor(DomainModel domainModel)
    {
      cnfVisitor = new ExtractingVisitor(domainModel);
    }
  }
}
