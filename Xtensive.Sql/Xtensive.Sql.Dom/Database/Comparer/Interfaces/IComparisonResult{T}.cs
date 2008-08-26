using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public interface IComparisonResult<T> : IComparisonResult
  {
    /// <summary>
    /// Gets new value.
    /// </summary>
    T NewValue { get; }

    /// <summary>
    /// Gets original value.
    /// </summary>
    T OriginalValue { get; }
  }
}