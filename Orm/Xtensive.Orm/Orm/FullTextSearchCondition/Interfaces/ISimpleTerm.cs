namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Specifies a match for an exact word or a phrase
  /// </summary>
  public interface ISimpleTerm : IProximityOperand
  {
    /// <summary>
    /// Word or phrase.
    /// </summary>
    string Term { get; }
  }
}