// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.14

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class SchemaComparer : NodeComparerBase<Schema>
  {
    private readonly NodeComparerStruct<User> userComparer;
    private readonly NodeComparerStruct<Assertion> assertionComparer;
    private readonly NodeComparerStruct<CharacterSet> characterSetComparer;
    private readonly NodeComparerStruct<Collation> collationComparer;
    private readonly NodeComparerStruct<Translation> translationComparer;
    private readonly NodeComparerStruct<Domain> domainComparer;
    private readonly NodeComparerStruct<Sequence> sequenceComparer;
    private readonly NodeComparerStruct<Table> tableComparer;
    private readonly NodeComparerStruct<View> viewComparer;

    public override IComparisonResult<Schema> Compare(Schema originalNode, Schema newNode)
    {
      var result = ComparisonContext.Current.Factory.CreateComparisonResult<Schema, SchemaComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
      result.Owner = (UserComparisonResult) userComparer.Compare(originalNode==null ? null : originalNode.Owner, newNode==null ? null : newNode.Owner);
      hasChanges |= result.Owner.HasChanges;
      result.DefaultCharacterSet = (CharacterSetComparisonResult) characterSetComparer.Compare(originalNode==null ? null : originalNode.DefaultCharacterSet, newNode==null ? null : newNode.DefaultCharacterSet);
      hasChanges |= result.DefaultCharacterSet.HasChanges;
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.Tables, newNode==null ? null : newNode.Tables, tableComparer, result.Tables);
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.Views, newNode==null ? null : newNode.Views, viewComparer, result.Views);
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.Assertions, newNode==null ? null : newNode.Assertions, assertionComparer, result.Assertions);
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.CharacterSets, newNode==null ? null : newNode.CharacterSets, characterSetComparer, result.CharacterSets);
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.Collations, newNode==null ? null : newNode.Collations, collationComparer, result.Collations);
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.Translations, newNode==null ? null : newNode.Translations, translationComparer, result.Translations);
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.Domains, newNode==null ? null : newNode.Domains, domainComparer, result.Domains);
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.Sequences, newNode==null ? null : newNode.Sequences, sequenceComparer, result.Sequences);
      if (hasChanges && result.ResultType==ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      return result;
    }

    public SchemaComparer(INodeComparerProvider provider)
      : base(provider)
    {
      userComparer = provider.GetNodeComparer<User>();
      assertionComparer = provider.GetNodeComparer<Assertion>();
      characterSetComparer = provider.GetNodeComparer<CharacterSet>();
      collationComparer = provider.GetNodeComparer<Collation>();
      translationComparer = provider.GetNodeComparer<Translation>();
      domainComparer = provider.GetNodeComparer<Domain>();
      sequenceComparer = provider.GetNodeComparer<Sequence>();
      tableComparer = provider.GetNodeComparer<Table>();
      viewComparer = provider.GetNodeComparer<View>();
    }
  }
}