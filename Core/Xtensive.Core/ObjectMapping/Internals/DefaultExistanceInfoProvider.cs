// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.08

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.ObjectMapping
{
  internal sealed class DefaultExistanceInfoProvider : IExistanceInfoProvider
  {
    public void Get(ReadOnlyDictionary<object,object> modified, ReadOnlyDictionary<object,object> original,
      out IEnumerable<object> created, out IEnumerable<object> removed)
    {
      ArgumentValidator.EnsureArgumentNotNull(modified, "modified");
      ArgumentValidator.EnsureArgumentNotNull(original, "original");

      created = GetCreatedObjects(modified, original);
      removed = GetRemovedObjects(modified, original);
    }

    private static IEnumerable<object> GetCreatedObjects(ReadOnlyDictionary<object,object> modified,
      IDictionary<object, object> original)
    {
      foreach (var modifiedObjectPair in modified)
        if (!original.ContainsKey(modifiedObjectPair.Key))
          yield return modifiedObjectPair.Value;
    }

    private static IEnumerable<object> GetRemovedObjects(IDictionary<object, object> modified,
      ReadOnlyDictionary<object,object> original)
    {
      foreach (var originalObjectPair in original)
        if (!modified.ContainsKey(originalObjectPair.Key))
          yield return originalObjectPair.Value;
    }
  }
}