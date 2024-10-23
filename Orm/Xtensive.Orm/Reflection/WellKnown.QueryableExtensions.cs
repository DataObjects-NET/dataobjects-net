// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Reflection
{
  public partial class WellKnown
  {
    /// <summary>
    /// Various well-known constants related to <see cref="Orm.QueryableExtensions" />
    /// </summary>
    public static class QueryableExtensions
    {
      public const string Count = nameof(Orm.QueryableExtensions.Count);
      public const string LeftJoin = nameof(Orm.QueryableExtensions.LeftJoin);
      public const string Lock = nameof(Orm.QueryableExtensions.Lock);
      public const string Take = nameof(Orm.QueryableExtensions.Take);
      public const string Skip = nameof(Orm.QueryableExtensions.Skip);
      public const string ElementAt = nameof(Orm.QueryableExtensions.ElementAt);
      public const string ElementAtOrDefault = nameof(Orm.QueryableExtensions.ElementAtOrDefault);
      public const string Tag = nameof(Orm.QueryableExtensions.Tag);
      public const string In = nameof(Orm.QueryableExtensions.In);
    }
  }
}
