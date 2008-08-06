// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.21

using System;
using System.Runtime.Serialization;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Provides a weak reference to an object of the given type to be used in
  /// a <see cref="WeakDictionary{TKey,TValue}"/> along with the given comparer.
  /// </summary>
  [Serializable]
  internal sealed class WeakKeyReference<T> : WeakReference<T> 
    where T : class
  {
    public readonly int HashCode;

    
    // Constructors

    public WeakKeyReference(T key, WeakKeyComparer<T> comparer)
      : base(key)
    {
      // retain the object's hash code immediately so that even
      // if the target is GC'ed we will be able to find and
      // remove the dead weak reference.
      HashCode = comparer.GetHashCode(key);
    }

    // Serialization

    protected WeakKeyReference(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      HashCode = info.GetInt32("HashCode");
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("HashCode", HashCode);
    }
  }
}