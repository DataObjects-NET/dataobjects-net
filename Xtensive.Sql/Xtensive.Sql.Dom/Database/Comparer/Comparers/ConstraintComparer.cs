// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using Xtensive.Sql.Dom.Resources;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class ConstraintComparer : NodeComparerBase<Constraint>
  {
    private readonly NodeComparerStruct<CheckConstraint> checkConstraintComparer;
    private readonly NodeComparerStruct<DomainConstraint> domainConstraintComparer;
    private readonly NodeComparerStruct<PrimaryKey> primaryKeyComparer;
    private readonly NodeComparerStruct<ForeignKey> foreignKeyComparer;
    private readonly NodeComparerStruct<UniqueConstraint> uniqueConstraintComparer;

    public override IComparisonResult<Constraint> Compare(Constraint originalNode, Constraint newNode)
    {
      IComparisonResult<Constraint> result;
      if (originalNode==null && newNode==null)
        result = ComparisonContext.Current.Factory.CreateComparisonResult<Constraint, ConstraintComparisonResult>(originalNode, newNode, ComparisonResultType.Unchanged);
      else if (originalNode!=null && newNode!=null && originalNode.GetType()!=newNode.GetType())
        result = ComparisonContext.Current.Factory.CreateComparisonResult<Constraint, ConstraintComparisonResult>(originalNode, newNode, ComparisonResultType.Modified);
      else if ((originalNode ?? newNode).GetType()==typeof (CheckConstraint))
        result = (IComparisonResult<Constraint>) checkConstraintComparer.Compare(originalNode as CheckConstraint, newNode as CheckConstraint);
      else if ((originalNode ?? newNode).GetType()==typeof (DomainConstraint))
        result = (IComparisonResult<Constraint>) domainConstraintComparer.Compare(originalNode as DomainConstraint, newNode as DomainConstraint);
      else if ((originalNode ?? newNode).GetType()==typeof (PrimaryKey))
        result = (IComparisonResult<Constraint>) primaryKeyComparer.Compare(originalNode as PrimaryKey, newNode as PrimaryKey);
      else if ((originalNode ?? newNode).GetType()==typeof (ForeignKey))
        result = (IComparisonResult<Constraint>) foreignKeyComparer.Compare(originalNode as ForeignKey, newNode as ForeignKey);
      else if ((originalNode ?? newNode).GetType()==typeof (UniqueConstraint))
        result = (IComparisonResult<Constraint>) uniqueConstraintComparer.Compare(originalNode as UniqueConstraint, newNode as UniqueConstraint);
      else
        throw new NotSupportedException(String.Format(Strings.ExConstraintIsNotSupportedByComparer, (originalNode ?? newNode).GetType().FullName, GetType().FullName));
      return result;
    }

    public ConstraintComparer(INodeComparerProvider provider)
      : base(provider)
    {
      checkConstraintComparer = provider.GetNodeComparer<CheckConstraint>();
      domainConstraintComparer = provider.GetNodeComparer<DomainConstraint>();
      primaryKeyComparer = provider.GetNodeComparer<PrimaryKey>();
      foreignKeyComparer = provider.GetNodeComparer<ForeignKey>();
      uniqueConstraintComparer = provider.GetNodeComparer<UniqueConstraint>();
    }
  }
}