// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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