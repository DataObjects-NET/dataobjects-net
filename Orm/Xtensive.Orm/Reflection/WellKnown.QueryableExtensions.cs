// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Reflection
{
  public partial class WellKnown
  {
    /// <summary>
    /// Various well-known constants related to <see cref="Orm.QueryableExtensionsEx" />
    /// </summary>
    public static class QueryableExtensions
    {
      public const string Count = nameof(Orm.QueryableExtensionsEx.Count);
#if !NET10_0_OR_GREATER
      public const string LeftJoin = nameof(Orm.QueryableExtensionsEx.LeftJoin);
#endif
      public const string Lock = nameof(Orm.QueryableExtensionsEx.Lock);
      public const string Take = nameof(Orm.QueryableExtensionsEx.Take);
      public const string Skip = nameof(Orm.QueryableExtensionsEx.Skip);
      public const string ElementAt = nameof(Orm.QueryableExtensionsEx.ElementAt);
      public const string ElementAtOrDefault = nameof(Orm.QueryableExtensionsEx.ElementAtOrDefault);
      public const string Tag = nameof(Orm.QueryableExtensionsEx.Tag);
      public const string In = nameof(Orm.QueryableExtensionsEx.In);
    }
  }
}
