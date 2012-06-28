// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A collection of <see cref="FullTextIndexInfo"/> objects.
  /// </summary>
  [Serializable]
  public sealed class FullTextIndexInfoCollection : LockableBase,
    IEnumerable<FullTextIndexInfo>
  {
    private readonly HashSet<FullTextIndexInfo> container = new HashSet<FullTextIndexInfo>();
    private readonly Dictionary<TypeInfo,FullTextIndexInfo> indexMap = new Dictionary<TypeInfo, FullTextIndexInfo>();

    /// <summary>
    /// Gets the <see cref="FullTextIndexInfo"/> by the specified type.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Index is not found.</exception>
    public FullTextIndexInfo this[TypeInfo type] {
      get {
        FullTextIndexInfo fulltextIndex;
        if (!TryGetValue(type, out fulltextIndex))
          throw new KeyNotFoundException();
        return fulltextIndex;
      }
    }

    /// <summary>
    /// Tries get <see cref="FullTextIndexInfo"/> by provided <see cref="TypeInfo"/>.
    /// </summary>
    /// <param name="typeInfo">The type info.</param>
    /// <param name="fullTextIndexInfo">The full text index info.</param>
    /// <returns><see langword="true" /> when the full-text index is found; otherwise <see langword="false" />.</returns>
    public bool TryGetValue(TypeInfo typeInfo, out FullTextIndexInfo fullTextIndexInfo)
    {
      return indexMap.TryGetValue(typeInfo, out fullTextIndexInfo);
    }

    /// <summary>
    /// Registers specified full-text index by type key.
    /// </summary>
    /// <param name="typeInfo">The type info.</param>
    /// <param name="fullTextIndexInfo">The full text index info.</param>
    public void Add(TypeInfo typeInfo, FullTextIndexInfo fullTextIndexInfo)
    {
      this.EnsureNotLocked();
      if (!container.Contains(fullTextIndexInfo))
        container.Add(fullTextIndexInfo);
      indexMap.Add(typeInfo, fullTextIndexInfo);
    }

    #region IEnumerable<...> members

    /// <inheritdoc/>
    public IEnumerator<FullTextIndexInfo> GetEnumerator()
    {
      return container.ToList().GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }
}