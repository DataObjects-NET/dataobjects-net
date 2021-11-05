// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// Places where comment can be inserted in
  /// SQL statement.
  /// </summary>
  public enum SqlCommentLocation
  {
    Nowhere = 0,
    BeforeStatement = 1,
    WithinStatement = 2,
    AfterStatement = 3,
  }
}