using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public sealed class AndNot : Operator
  {
    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal AndNot(IOperand source)
      : base(SearchConditionNodeType.AndNot, source)
    {
    }
  }
}