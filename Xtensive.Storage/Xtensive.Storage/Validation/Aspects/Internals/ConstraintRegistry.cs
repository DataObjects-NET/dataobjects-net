// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using System.Collections.Generic;
using System.Threading;
using Xtensive.Threading;

namespace Xtensive.Storage.Validation
{
  /// <summary>
  /// The registry of all applied constraints.
  /// </summary>
  public static class ConstraintRegistry
  {
    #region Nested type: TypeEntry

    private class TypeEntry
    {
      public ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
      public List<PropertyConstraintAspect> Constraints = new List<PropertyConstraintAspect>();
    }

    #endregion

    private static ThreadSafeDictionary<Type, TypeEntry> registry = 
      ThreadSafeDictionary<Type, TypeEntry>.Create(new object());

    internal static void RegisterConstraint(Type targetType, PropertyConstraintAspect constraint)
    {
      var entry = registry.GetValue(targetType, _targetType => new TypeEntry());
      using (entry.Lock.WriteRegion()) {
        entry.Constraints.Add(constraint);
      }
    }

    /// <summary>
    /// Gets all constraints targeted to the specified type and it's ancestors.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <returns>Enumerable of constraints.</returns>
    public static PropertyConstraintAspect[] GetConstraints(Type targetType)
    {
      var result = new List<PropertyConstraintAspect>();
      var type = targetType;
      while (type!=null) {
        TypeEntry entry;
        if (!registry.TryGetValue(type, out entry)) {
          type = type.BaseType;
          continue;
        }
        entry.Lock.BeginRead();
        try {
          result.AddRange(entry.Constraints);
        }
        finally {
          entry.Lock.EndRead();
        }
        type = type.BaseType;
      }
      return result.ToArray();
    }
  }
}