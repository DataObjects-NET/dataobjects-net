// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Orm
{
  /// <summary>
  /// Tags location in result SQL query.
  /// </summary>
  public enum TagsLocation
  {
    /// <summary>
    /// Tags, if any, won't appear in SQL query.
    /// </summary>
    Nowhere = 0,
    /// <summary>
    /// Tags, if any, will appear before SQL statement.
    /// </summary>
    BeforeStatement = 1,
    /// <summary>
    /// Tags, if any, will appear within SQL statement.
    /// </summary>
    WithinStatement = 2,
    /// <summary>
    /// Tag will be placed at the end
    /// of SQL statement
    /// </summary>
    AfterStatement = 3,
    /// <summary>
    /// <see cref="BeforeStatement"/>
    /// </summary>
    Default = BeforeStatement
  }
}
