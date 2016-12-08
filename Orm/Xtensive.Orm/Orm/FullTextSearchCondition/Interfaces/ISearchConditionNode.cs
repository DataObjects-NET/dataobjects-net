namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Node used in search condition.
  /// </summary>
  public interface ISearchConditionNode
  {
    /// <summary>
    /// Type of node.
    /// </summary>
    SearchConditionNodeType NodeType { get; }

    /// <summary>
    /// Accepts visitor to the node
    /// </summary>
    /// <param name="visitor">A visitor accepted to the node.</param>
    void AcceptVisitor(ISearchConditionNodeVisitor visitor);
  }
}