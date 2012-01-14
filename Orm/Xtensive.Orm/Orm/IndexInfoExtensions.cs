// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.14

using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm
{
  /// <summary>
  /// <see cref="IndexInfo"/> related extension methods.
  /// </summary>
  public static class IndexInfoExtensions
  {
    /// <summary>
    /// Creates the <see cref="RecordQuery"/> allowing to query the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index to create the <see cref="RecordQuery"/> for.</param>
    /// <returns>Newly created <see cref="RecordQuery"/> object.</returns>
    public static RecordQuery ToRecordQuery(this IndexInfo index)
    {
      return IndexProvider.Get(index).Result;
    }
  }
}