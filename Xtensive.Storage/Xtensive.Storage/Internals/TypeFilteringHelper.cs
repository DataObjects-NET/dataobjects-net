// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.03

using System;

namespace Xtensive.Storage.Internals
{
  internal static class TypeFilteringHelper
  {
    public static bool IsPersistentType(Type type)
    {
      return type.IsSubclassOf(typeof(Persistent))
        || (type.IsInterface && typeof(IEntity).IsAssignableFrom(type));
    }
  }
}