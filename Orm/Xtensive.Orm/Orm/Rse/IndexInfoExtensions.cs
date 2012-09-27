// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.14

using Xtensive.Orm.Model;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// <see cref="IndexInfo"/> related extension methods.
  /// </summary>
  public static class IndexInfoExtensions
  {
    /// <summary>
    /// Creates the <see cref="IndexProvider"/> allowing to query the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index to create the <see cref="IndexProvider"/> for.</param>
    /// <returns>Newly created <see cref="IndexProvider"/> object.</returns>
    public static IndexProvider GetQuery(this IndexInfo index)
    {
      return new IndexProvider(index);
    }
  }
}