// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class ConstraintSqlComparer : SqlComparerBase<Constraint>
  {
    private readonly SqlComparerStruct<CheckConstraint> checkConstraintComparer;
    private readonly SqlComparerStruct<DomainConstraint> domainConstraintComparer;
    private readonly SqlComparerStruct<PrimaryKey> primaryKeyComparer;
    private readonly SqlComparerStruct<ForeignKey> foreignKeyComparer;
    private readonly SqlComparerStruct<UniqueConstraint> uniqueConstraintComparer;

    public override IComparisonResult<Constraint> Compare(Constraint originalNode, Constraint newNode, IEnumerable<ComparisonHintBase> hints)
    {
      IComparisonResult<Constraint> result;
      if (originalNode==null && newNode==null) {
        result = new ConstraintComparisonResult
          {
            OriginalValue = originalNode,
            NewValue = newNode,
            ResultType = ComparisonResultType.Unchanged
          };
      }
      else if (originalNode!=null && newNode!=null && originalNode.GetType()!=newNode.GetType()) {
        result = new ConstraintComparisonResult
          {
            OriginalValue = originalNode,
            NewValue = newNode,
            ResultType = ComparisonResultType.Modified
          };
      }
      else if ((originalNode ?? newNode).GetType()==typeof (CheckConstraint)) {
        result = (IComparisonResult<Constraint>) checkConstraintComparer.Compare(originalNode as CheckConstraint, newNode as CheckConstraint, hints);
      }
      else if ((originalNode ?? newNode).GetType()==typeof (DomainConstraint)) {
        result = (IComparisonResult<Constraint>) domainConstraintComparer.Compare(originalNode as DomainConstraint, newNode as DomainConstraint, hints);
      }
      else if ((originalNode ?? newNode).GetType()==typeof (PrimaryKey)) {
        result = (IComparisonResult<Constraint>) primaryKeyComparer.Compare(originalNode as PrimaryKey, newNode as PrimaryKey, hints);
      }
      else if ((originalNode ?? newNode).GetType()==typeof (ForeignKey)) {
        result = (IComparisonResult<Constraint>) foreignKeyComparer.Compare(originalNode as ForeignKey, newNode as ForeignKey, hints);
      }
      else if ((originalNode ?? newNode).GetType()==typeof (UniqueConstraint)) {
        result = (IComparisonResult<Constraint>) uniqueConstraintComparer.Compare(originalNode as UniqueConstraint, newNode as UniqueConstraint, hints);
      }
      else {
        throw new NotSupportedException(String.Format(Resources.Strings.ExConstraintIsNotSupportedByComparer, (originalNode ?? newNode).GetType().FullName, GetType().FullName));
      }
      result.Lock();
      return result;
    }

    public ConstraintSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
      checkConstraintComparer = provider.GetSqlComparer<CheckConstraint>();
      domainConstraintComparer = provider.GetSqlComparer<DomainConstraint>();
      primaryKeyComparer = provider.GetSqlComparer<PrimaryKey>();
      foreignKeyComparer = provider.GetSqlComparer<ForeignKey>();
      uniqueConstraintComparer = provider.GetSqlComparer<UniqueConstraint>();
    }
  }
}