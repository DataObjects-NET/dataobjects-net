// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Compiler;

namespace Xtensive.Orm.Providers
{
  public static class GeneralPurposeExtensions
  {
    /// <summary>
    /// Converts <see cref="TagsLocation"/> to <see cref="SqlCommentLocation"/>.
    /// </summary>
    /// <param name="tagPlace"></param>
    /// <returns></returns>
    public static SqlCommentLocation ToCommentLocation(this TagsLocation tagPlace) =>
      tagPlace switch {
        TagsLocation.Nowhere => SqlCommentLocation.Nowhere,
        TagsLocation.BeforeStatement => SqlCommentLocation.BeforeStatement,
        TagsLocation.WithinStatement => SqlCommentLocation.WithinStatement,
        TagsLocation.AfterStatement => SqlCommentLocation.AfterStatement,
        _ => throw new ArgumentOutOfRangeException(nameof(tagPlace))
      };
  }
}
