// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Places where comment can be inserted in
  /// SQL statement.
  /// </summary>
  public enum SqlCommentPlace
  {
    /// <summary>
    /// Comment will be at the beginning.
    /// </summary>
    Beginning = 1,
    /// <summary>
    /// Tag will be placed somewhere
    /// within query.
    /// </summary>
    Within = 2,
    /// <summary>
    /// Tag will be placed at the end
    /// of SQL statement
    /// </summary>
    End = 3,
    /// <summary>
    /// <see cref="Beginning"/>.
    /// </summary>
    Default = Beginning,
  }
}