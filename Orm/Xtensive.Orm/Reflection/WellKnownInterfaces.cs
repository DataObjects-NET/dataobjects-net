using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Reflection
{
  internal static class WellKnownInterfaces
  {
    public static readonly Type QueryableOfT = typeof(IQueryable<>);
    public static readonly Type EnumerableOfT = typeof(IEnumerable<>);
    public static readonly Type AsyncEnumerableOfT = typeof(IAsyncEnumerable<>);
  }
}