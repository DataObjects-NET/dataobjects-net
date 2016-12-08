
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public sealed class And : Operator
  {
    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal And(IOperand source)
      : base(SearchConditionNodeType.And, source)
    {
    }
  }
}
