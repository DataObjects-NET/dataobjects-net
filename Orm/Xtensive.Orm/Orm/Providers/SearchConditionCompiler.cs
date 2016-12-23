// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.Providers
{
  public abstract class SearchConditionCompiler : ISearchConditionNodeVisitor
  {
    public virtual string CurrentOutput { get { return string.Empty; } }

    public abstract void Visit(IOperator node);

    public abstract void Visit(ISimpleTerm node);

    public abstract void Visit(IPrefixTerm node);

    public abstract void Visit(IGenerationTerm node);

    public abstract void Visit(IProximityTerm node);

    public abstract void Visit(ICustomProximityTerm node);

    public abstract void Visit(IWeightedTerm node);

    public abstract void Visit(IComplexTerm node);
  }
}
