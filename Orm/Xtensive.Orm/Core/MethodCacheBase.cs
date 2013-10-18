// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.10

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;


namespace Xtensive.Core
{
  /// <summary>
  /// Base class for any method caching class.
  /// </summary>
  /// <typeparam name="TImplementation">The type of <see cref="Implementation"/>.</typeparam>
  [Serializable]
  public abstract class MethodCacheBase<TImplementation>: ISerializable
    where TImplementation: class 
  {
    /// <summary>
    /// Gets underlying implementation object or interface.
    /// </summary>
    public readonly TImplementation Implementation;


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="implementation"><see cref="Implementation"/> property value.</param>
    public MethodCacheBase(TImplementation implementation)
    {
      Implementation = implementation;
    }

    /// <summary>
    /// Deserializes instance of this type.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected MethodCacheBase(SerializationInfo info, StreamingContext context)
    {
      Implementation = (TImplementation)info.GetValue("Implementation", typeof(TImplementation));
    }

    /// <summary>
    /// Serializes instance of this type.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    [SecurityCritical]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Implementation", Implementation);
    }
  }
}