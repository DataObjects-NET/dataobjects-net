// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.14

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class SchemaSqlComparer : SqlComparerBase<Schema>
  {
    private readonly SqlComparerStruct<User> userComparer;
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
      ValidateArguments(originalNode, newNode);
      var result = new SchemaComparisonResult();
      ProcessDbName(originalNode, newNode, result);
      if (originalNode==null) {
        result.ResultType = ComparisonResultType.Added;
      }
      else if (newNode==null) {
        result.ResultType = ComparisonResultType.Removed;
      }
      result.OriginalValue = originalNode;
      result.NewValue = newNode;
      result.Owner = (NodeComparisonResult<User>)userComparer.Compare(originalNode == null ? null : originalNode.Owner, newNode == null ? null : newNode.Owner, hints);
      result.DefaultCharacterSet = (NodeComparisonResult<CharacterSet>)characterSetComparer.Compare(originalNode == null ? null : originalNode.DefaultCharacterSet, newNode == null ? null : newNode.DefaultCharacterSet, hints);
      return result;
      throw new NotImplementedException();
    }

    public SchemaSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
      userComparer = provider.GetSqlComparer<User>();
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