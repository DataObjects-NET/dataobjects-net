// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Orm
{
  /// <summary>
  /// Defines NULLs treatment by unique indexes.
  /// </summary>
  public enum UniqueNullTreatment
  {
    /// <summary>
    /// Means no interfierence from DataObjects.Net. The treatemet is up to storage.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// NULLs are distinct values, so more than one NULL value is allowed in Unique indexes for a column.
    /// Applied only to Unique indexes and when storage supports such setting.
    /// </summary>
    Distinct = 1,

    /// <summary>
    /// NULLs are NOT distinct values, so ONLY ONE NULL value is allowed in Unique indexes for a column.
    /// Applied only to Unique indexes and when storage supports such setting.
    /// </summary>
    NotDistinct = 2,

    /// <summary>
    /// See <see cref="Unspecified" />.
    /// </summary>
    Default = Unspecified

  }
}
