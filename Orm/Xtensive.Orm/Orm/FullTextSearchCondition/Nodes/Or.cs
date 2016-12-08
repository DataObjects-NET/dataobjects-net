using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public sealed class Or : Operator
  {
    public override void AcceptVisitor(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal Or(IOperand source)
      : base(SearchConditionNodeType.Or, source)
    {
    }
  }
}