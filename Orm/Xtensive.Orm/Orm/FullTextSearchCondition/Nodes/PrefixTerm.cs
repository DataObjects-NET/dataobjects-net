using System;
using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public sealed class PrefixTerm : Operand, IPrefixTerm, IWeighableTerm
  {
    public string Prefix { get; private set; }

    public override void AcceptVisitor(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal PrefixTerm(string prefix)
      : this(null, prefix)
    {
    }

    internal PrefixTerm(IOperator source, string prefix)
      : base(SearchConditionNodeType.Prefix, source)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmptyOrWhiteSpace(prefix, "prefix");
      Prefix = prefix;
    }
  }
}