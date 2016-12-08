using System.Collections.Generic;

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Generic proximity term. Allows to set several terms as located near each other.
  /// </summary>
  public interface IProximityTerm : IOperand
  {
    /// <summary>
    /// Gets list of near terms.
    /// </summary>
    IList<IProximityOperand> Terms { get; }
  }
}