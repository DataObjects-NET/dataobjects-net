using System.Collections.Generic;

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Specifies that the matching rows match a list of words and phrases, each optionally given a weighting value.
  /// </summary>
  public interface IWeightedTerm : IOperand
  {
    /// <summary>
    /// Terms mapped to its weights.
    /// </summary>
    IDictionary<IWeighableTerm, float?> WeighedTerms { get; }
  }
}