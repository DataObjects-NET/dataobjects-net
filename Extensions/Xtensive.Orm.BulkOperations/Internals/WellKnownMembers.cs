// Copyright (C) 2020-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.BulkOperations
{
  internal static class WellKnownMembers
  {
    public const string InMethodName = nameof(QueryableExtensionsEx.In);
    public const string SpanContainsExtensionName = nameof(MemoryExtensions.Contains);
    public const string EnumerableContainsName = nameof(Enumerable.Contains);

    public static readonly MethodInfo TranslateQueryMethod =
      typeof(QueryBuilder).GetMethod(nameof(QueryBuilder.TranslateQuery));

    public static readonly MethodInfo InMethod = GetInMethod();

    private static MethodInfo GetInMethod()
    {
      foreach (var method in WellKnownTypes.QueryableExtensionsType.GetMethods().Where(a => a.Name == InMethodName)) {
        var parameters = method.GetParameters();
        if (parameters.Length == 3 && parameters[2].ParameterType.Name == "IEnumerable`1") {
          return method;
        }
      }

      return null;
    }
  }
}