
using System;
using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public sealed class SimpleTerm : Operand, ISimpleTerm, IWeighableTerm
  {
    public string Term { get; private set; }

    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SimpleTerm(string term)
      : this(null, term)
    {
    }

    internal SimpleTerm(IOperator source, string term)
      : base(SearchConditionNodeType.SimpleTerm, source)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmptyOrWhiteSpace(term, "term");
      Term = term;
    }
  }
}
