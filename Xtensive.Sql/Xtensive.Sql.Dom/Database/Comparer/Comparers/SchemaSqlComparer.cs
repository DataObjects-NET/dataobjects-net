// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.14

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  internal class SchemaSqlComparer : SqlComparerBase<Schema>
  {
    private readonly SqlComparerStruct<Assertion> assertionComparer;
    private readonly SqlComparerStruct<CharacterSet> characterSetComparer;
    private readonly SqlComparerStruct<Collation> collationComparer;
    private readonly SqlComparerStruct<Translation> translationComparer;
    private readonly SqlComparerStruct<Domain> domainComparer;
    private readonly SqlComparerStruct<Sequence> sequenceComparer;
    private readonly SqlComparerStruct<Table> tableComparer;
    private readonly SqlComparerStruct<View> viewComparer;

    public override ComparisonResult<Schema> Compare(Schema originalNode, Schema newNode, IEnumerable<ComparisonHintBase> hints)
    {
      ComparisonResult<Schema> result = new ComparisonResult<Schema>();
      // Compare properties.
      
      // Compare nested nodes.
      throw new NotImplementedException();
    }

    public SchemaSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
      assertionComparer = provider.GetSqlComparer<Assertion>();
      characterSetComparer = provider.GetSqlComparer<CharacterSet>();
      collationComparer = provider.GetSqlComparer<Collation>();
      translationComparer = provider.GetSqlComparer<Translation>();
      domainComparer = provider.GetSqlComparer<Domain>();
      sequenceComparer = provider.GetSqlComparer<Sequence>();
      tableComparer = provider.GetSqlComparer<Table>();
      viewComparer = provider.GetSqlComparer<View>();
    }
  }
}