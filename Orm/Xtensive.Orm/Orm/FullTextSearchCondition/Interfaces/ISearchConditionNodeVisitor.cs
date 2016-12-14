// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  public interface ISearchConditionNodeVisitor
  {
    void Visit(IOperator node);
    void Visit(ISimpleTerm node);
    void Visit(IPrefixTerm node);
    void Visit(IGenerationTerm node);
    void Visit(IProximityTerm node);
    void Visit(ICustomProximityTerm node);
    void Visit(IWeightedTerm node);
    void Visit(IComplexTerm node);
  }
}