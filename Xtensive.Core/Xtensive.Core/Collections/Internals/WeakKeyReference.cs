// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.21

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Provides a weak reference to an object of the given type to be used in
  /// a WeakDictionary along with the given comparer.
  /// </summary>
  internal sealed class WeakKeyReference<T> : WeakReference<T> where T : class
  {
    public readonly int HashCode;

    public WeakKeyReference(T key, WeakKeyComparer<T> comparer)
      : base(key)
    {
      // retain the object's hash code immediately so that even
      // if the target is GC'ed we will be able to find and
      // remove the dead weak reference.
      HashCode = comparer.GetHashCode(key);
    }
  }
}