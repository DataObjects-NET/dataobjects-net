// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.20

using System;
using System.Runtime.Serialization;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Adds strong typing to WeakReference.Target using generics. Also,
  /// the Create factory method is used in place of a constructor
  /// to handle the case where target is null, but we want the 
  /// reference to still appear to be alive.
  /// </summary>
  internal class WeakReference<T> : WeakReference where T : class
  {
    public new T Target
    {
      get { return (T)base.Target; }
    }

    public static WeakReference<T> Create(T target)
    {
      if (target == null)
        return WeakNullReference<T>.Singleton;

      return new WeakReference<T>(target);
    }

    protected WeakReference(T target)
      : base(target, false) { }

    protected WeakReference(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }


  }
}