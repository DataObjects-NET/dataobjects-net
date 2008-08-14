// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.14

using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage
{
  /// <summary>
  /// <see cref="IndexInfo"/> related extension methods.
  /// </summary>
  public static class IndexInfoExtensions
  {
    /// <summary>
    /// Creates the <see cref="RecordSet"/> allowing to query the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index to create the <see cref="RecordSet"/> for.</param>
    /// <returns>Newly created <see cref="RecordSet"/> object.</returns>
    public static RecordSet ToRecordSet(this IndexInfo index)
    {
      Session.Current.Persist();
      return IndexProvider.Get(index).Result;
    }
  }
}