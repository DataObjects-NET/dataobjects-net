// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Sql.Model
{
  public enum UniqueNullTreatment
  {
    /// <summary>
    /// Unspecified value will be resolved into native for RDBMS treatment.
    /// </summary>
    Unspecified,
    
    /// <summary>
    /// NULL values are distinct, unique indexes will allow more than one row with NULL
    /// </summary>
    Distinct,

    /// <summary>
    /// NULLS are not distinct, unique indexes
    /// </summary>
    NotDistinct
  }
}
