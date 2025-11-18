// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.Providers
{
  public abstract class SearchConditionCompiler : ISearchConditionNodeVisitor
  {
    protected const char OpeningParenthesis = '(';
    protected const char ClosingParenthesis = ')';
    protected const string CommaDelimiter = ", ";

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
