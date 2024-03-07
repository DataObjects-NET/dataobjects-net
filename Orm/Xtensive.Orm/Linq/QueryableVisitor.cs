// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.02.25

using System.Linq.Expressions;
using Xtensive.Reflection;

namespace Xtensive.Linq
{
  /// <summary>
  /// Abstract base visitor that handles methods of <see cref="IQueryable"/> and <see cref="IEnumerable{T}"/> by calling <see cref="VisitQueryableMethod"/>.
  /// </summary>
  [Serializable]
  public abstract class QueryableVisitor : ExpressionVisitor
  {
    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Arguments.Count > 0 && mc.Arguments[0].Type == WellKnownTypes.String) {
        return base.VisitMethodCall(mc);
      }

      var method = GetQueryableMethod(mc);
      if (method == null) {
        return base.VisitMethodCall(mc);
      }

      return VisitQueryableMethod(mc, method.Value);
    }

    /// <summary>
    /// Visits method of <see cref="IQueryable"/> or <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="mc">The method call expression.</param>
    /// <param name="methodKind">Kind of the method.</param>
    protected abstract Expression VisitQueryableMethod(MethodCallExpression mc, QueryableMethodKind methodKind);

    /// <summary>
    /// Parses <see cref="QueryableMethodKind"/> for the specified expression.
    /// </summary>
    /// <param name="call">A call to process.</param>
    /// <returns><see cref="QueryableMethodKind"/> for the specified expression,
    /// or null if method is not a LINQ method.</returns>
    public static QueryableMethodKind? GetQueryableMethod(MethodCallExpression call)
    {
      if (call == null) {
        return null;
      }

      var declaringType = call.Method.DeclaringType;
      if (declaringType == WellKnownTypes.Queryable || declaringType == WellKnownTypes.Enumerable) {
        return ParseQueryableMethodKind(call.Method.Name);
      }

      return null;
    }

    private static QueryableMethodKind? ParseQueryableMethodKind(string methodName)
    {
      if (Enum.TryParse(methodName, out QueryableMethodKind result)) {
        return result;
      }

      return null;
    }
  }
}