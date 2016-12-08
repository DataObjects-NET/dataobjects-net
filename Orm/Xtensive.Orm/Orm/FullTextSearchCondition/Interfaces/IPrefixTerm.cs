namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Specifies a match of words or phrases beginning with the specified text.
  /// </summary>
  public interface IPrefixTerm : IProximityOperand
  {
    /// <summary>
    /// Word or phrase which will be automatically followed by '*'.
    /// </summary>
    string Prefix { get; }
  }
}