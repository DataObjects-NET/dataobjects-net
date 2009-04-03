// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.03

namespace Xtensive.Storage.Internals
{
  internal static class KeyExtensions
  {
    public static Entity TryResolve(this Key key)
    {
      if (key == null)
        return null;
      bool allFieldsAreNull = true;
      for (int i = 0; i < key.Value.Count && allFieldsAreNull; i++)
        if (!key.Value.IsNull(i))
          allFieldsAreNull = false;
      if (allFieldsAreNull)
        return null;
      return key.Resolve();
    }

    public static T TryResolve<T>(this Key key) where T : Entity
    {
      return (T)TryResolve(key);
    }
  }
}