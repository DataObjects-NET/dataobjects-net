// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.08

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class DefaultExistanceInfoProvider : IExistanceInfoProvider
  {
    private ReadOnlyDictionary<object, object> modifiedObjects;
    private ReadOnlyDictionary<object, object> originalObjects;

    public IDisposable Open(ReadOnlyDictionary<object, object> modified,
      ReadOnlyDictionary<object, object> original)
    {
      ArgumentValidator.EnsureArgumentNotNull(modified, "modified");
      ArgumentValidator.EnsureArgumentNotNull(original, "original");

      modifiedObjects = modified;
      originalObjects = original;

      return new Disposable<DefaultExistanceInfoProvider>(this, (isDisposing, _this) => _this.Clear());
    }

    public IEnumerable<object> GetCreatedObjects()
    {
      foreach (var modifiedObjectPair in modifiedObjects)
        if (!originalObjects.ContainsKey(modifiedObjectPair.Key))
          yield return modifiedObjectPair.Value;
    }

    public IEnumerable<object> GetRemovedObjects()
    {
      foreach (var originalObjectPair in originalObjects)
        if (!modifiedObjects.ContainsKey(originalObjectPair.Key))
          yield return originalObjectPair.Value;
    }

    private void Clear()
    {
      modifiedObjects = null;
      originalObjects = null;
    }
  }
}