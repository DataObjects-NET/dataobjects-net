// Copyright (C) 2020-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;

namespace Xtensive.Orm.BulkOperations
{
  internal static class WellKnownTypes
  {
    public static readonly Type EnumerableType = typeof(Enumerable);
    public static readonly Type FuncOfTResultType = typeof(Func<>);
    public static readonly Type FuncOfTArgTResultType = typeof(Func<,>);

    public static readonly Type IncludeAlgorithmType = typeof(IncludeAlgorithm);
    public static readonly Type QueryableType = typeof(Queryable);
    public static readonly Type QueryableExtensionsType = typeof(QueryableExtensionsEx);
    public static readonly Type MemoryExtensionsType = typeof(MemoryExtensions);
  }
}