using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public sealed class AndNot : Operator
  {
    public override void AcceptVisitor(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal AndNot(IOperand source)
      : base(SearchConditionNodeType.AndNot, source)
    {
    }
  }
}