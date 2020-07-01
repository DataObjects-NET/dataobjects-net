using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Reflection
{
  internal static class WellKnownInterfaces
  {
    public static readonly Type Collection = typeof(ICollection);

    public static readonly Type Comparable = typeof(IComparable);
    public static readonly Type ComparableOfT = typeof(IComparable<>);

    public static readonly Type Enumerable = typeof(IEnumerable);
    public static readonly Type EnumerableOfT = typeof(IEnumerable<>);

    public static readonly Type Queryable = typeof(IQueryable);
    public static readonly Type QueryableOfT = typeof(IQueryable<>);
    public static readonly Type AsyncEnumerableOfT = typeof(IAsyncEnumerable<>);
    public static readonly Type GroupingOfTKeyTElement = typeof(IGrouping<,>);
  }
}