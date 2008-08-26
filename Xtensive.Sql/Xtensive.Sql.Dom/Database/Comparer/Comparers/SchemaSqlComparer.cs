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

    public override IComparisonResult<Schema> Compare(Schema originalNode, Schema newNode, IEnumerable<ComparisonHintBase> hints)
    {
      SchemaComparisonResult result = InitializeResult<Schema, SchemaComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
      result.Owner = (NodeComparisonResult<User>)userComparer.Compare(originalNode == null ? null : originalNode.Owner, newNode == null ? null : newNode.Owner, hints);
      hasChanges |= result.Owner.HasChanges;
      result.DefaultCharacterSet = (NodeComparisonResult<CharacterSet>)characterSetComparer.Compare(originalNode == null ? null : originalNode.DefaultCharacterSet, newNode == null ? null : newNode.DefaultCharacterSet, hints);
      hasChanges |= result.DefaultCharacterSet.HasChanges;
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Tables, newNode == null ? null : newNode.Tables, hints, tableComparer, result.Tables);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Views, newNode == null ? null : newNode.Views, hints, viewComparer, result.Views);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Assertions, newNode == null ? null : newNode.Assertions, hints, assertionComparer, result.Assertions);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.CharacterSets, newNode == null ? null : newNode.CharacterSets, hints, characterSetComparer, result.CharacterSets);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Collations, newNode == null ? null : newNode.Collations, hints, collationComparer, result.Collations);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Translations, newNode == null ? null : newNode.Translations, hints, translationComparer, result.Translations);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Domains, newNode == null ? null : newNode.Domains, hints, domainComparer, result.Domains);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Sequences, newNode == null ? null : newNode.Sequences, hints, sequenceComparer, result.Sequences);
      if (hasChanges && result.ResultType == ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
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