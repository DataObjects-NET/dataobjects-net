using System.Linq.Expressions;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm
{
  /// <summary>
  /// Extended version of <see cref="IQueryPreprocessor"/>.
  /// Consider inheriting from <see cref="QueryPreprocessor"/> instead.
  /// </summary>
  public interface IQueryPreprocessor2 : IQueryPreprocessor
  {
    /// <summary>
    ///  Applies the preprocessor to the specified query.
    ///  </summary>
    /// <param name="session">Current session.</param>
    /// <param name="query">The query to apply the preprocessor to.</param>
    /// <returns>Application (preprocessing) result.</returns>
    Expression Apply(Session session, Expression query);
  }
}