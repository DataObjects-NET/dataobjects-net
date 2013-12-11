using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;

namespace Xtensive.Orm.Linq.Model
{
  internal static class QueryableMethodInfo
  {
    public static readonly MethodInfo Select;
    public static readonly MethodInfo GroupBy;
    public static readonly MethodInfo GroupByWithElementSelector;
    public static readonly MethodInfo GroupByWithResultSelector;
    public static readonly MethodInfo GroupByWithElementAndResultSelectors;

    private static MethodInfo MethodOf<T>(Expression<Func<IQueryable<object>, IQueryable<T>>> methodExpression)
    {
      var callExpression = (MethodCallExpression) methodExpression.Body.StripCasts();
      return callExpression.Method.GetGenericMethodDefinition();
    }

    static QueryableMethodInfo()
    {
      Select = MethodOf(q => q.Select(o => o));
      GroupBy = MethodOf(q => q.GroupBy(o => o));
      GroupByWithElementSelector = MethodOf(q => q.GroupBy(o => o, o => o.ToString()));
      GroupByWithResultSelector = MethodOf(q => q.GroupBy(o => o, (key, items) => items.Count()));
      GroupByWithElementAndResultSelectors = MethodOf(q => q.GroupBy(o => o, o => o.ToString(), (key, items) => items.Count()));
    }
  }
}