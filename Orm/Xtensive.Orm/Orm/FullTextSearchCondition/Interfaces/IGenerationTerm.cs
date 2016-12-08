using Xtensive.Collections;

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Specifies a match of words when the included simple terms include variants of the original word for which to search.
  /// </summary>
  public interface IGenerationTerm : IOperand
  {
    /// <summary>
    /// Gets type of generator of term variants
    /// </summary>
    GenerationType GenerationType { get; }

    /// <summary>
    /// Gets words or phrases which are basis for variants' generation.
    /// </summary>
    ReadOnlyList<string> Terms { get; } 
  }
}