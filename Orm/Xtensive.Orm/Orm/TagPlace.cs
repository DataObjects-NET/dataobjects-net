// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Orm
{
  /// <summary>
  /// Tag place in result SQL query.
  /// </summary>
  public enum TagPlace
  {
    /// <summary>
    /// Tag will be at the beginning
    /// </summary>
    Beginning = 0,
    /// <summary>
    /// Tag will be placed somewhere
    /// within query.
    /// </summary>
    Within = 1,
    /// <summary>
    /// Tag will be placed at the end
    /// of SQL statement
    /// </summary>
    End = 2,
    /// <summary>
    /// <see cref="Beginning"/>
    /// </summary>
    Default = Beginning
  }
}
