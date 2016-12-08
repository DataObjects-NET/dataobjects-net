namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Custom proximity term.
  /// Allow to configure parameters of proximity.
  /// </summary>
  public interface ICustomProximityTerm : IProximityTerm
  {
    /// <summary>
    /// Gets maximum distance between <see cref="IProximityTerm.Terms"/>.
    /// </summary>
    long? MaxDistance { get; }

    /// <summary>
    /// Gets value which indicates whether should keep <see cref="IProximityTerm.Terms"/> order.
    /// </summary>
    bool MatchOrder { get; }
  }
}