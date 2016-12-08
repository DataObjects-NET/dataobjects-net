
namespace Xtensive.Orm.FullTextSearchCondition
{
  /// <summary>
  /// The generation type specifies how Search chooses the alternative word forms.
  /// </summary>
  public enum GenerationType
  {
    /// <summary>
    /// Chooses alternative inflection forms for the match words.
    /// </summary>
    Inflectional,

    /// <summary>
    /// Chooses words that have the same meaning, taken from a thesaurus
    /// </summary>
    Thesaurus
  }
}
