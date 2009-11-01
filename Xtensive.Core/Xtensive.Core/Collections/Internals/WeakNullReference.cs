// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.20

using System.Runtime.Serialization;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Provides a weak reference to a null target object, which, unlike
  /// other weak references, is always considered to be alive. This 
  /// facilitates handling null dictionary values, which are perfectly
  /// legal.
  /// </summary>
  internal class WeakNullReference<T> : WeakReference<T> where T : class
  {
    public static readonly WeakNullReference<T> Singleton = new WeakNullReference<T>();

    private WeakNullReference() : base(null)
    {
    }

    public override bool IsAlive
    {
      get { return true; }
    }

    protected WeakNullReference(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}