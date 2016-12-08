using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  /// <summary>
  /// The root of search condition node sequence
  /// </summary>
  public sealed class ConditionEndpoint : Operator
  {
    public override void AcceptVisitor(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    public ConditionEndpoint()
      : base(SearchConditionNodeType.Root, null)
    {
    }
  }
}