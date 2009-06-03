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
      var persistent = typeof (Persistent);
      return type!=persistent && persistent.IsAssignableFrom(type)
        || persistent.IsAssignableFrom(typeof (IEntity));
    }
  }
}