// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.12

using Xtensive.Core.Collections;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// <see cref="RecordSetHeader"/> related extension methods.
  /// </summary>
  public static class RecordSetHeaderExtensions
  {
    /// <summary>
    /// Gets the <see cref="RecordSetHeader"/> object for the specified <paramref name="indexInfo"/>.
    /// </summary>
    /// <param name="indexInfo">The index info to get the header for.</param>
    /// <returns>The <see cref="RecordSetHeader"/> object.</returns>
    public static RecordSetHeader GetRecordSetHeader(this IndexInfo indexInfo)
    {
      return RecordSetHeader.GetHeader(indexInfo);
    }
  }
}