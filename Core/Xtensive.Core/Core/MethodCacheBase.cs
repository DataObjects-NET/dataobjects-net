// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.10

using System;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Internals.DocTemplates;

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
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation"><see cref="Implementation"/> property value.</param>
    public MethodCacheBase(TImplementation implementation)
    {
      Implementation = implementation;
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected MethodCacheBase(SerializationInfo info, StreamingContext context)
    {
      Implementation = (TImplementation)info.GetValue("Implementation", typeof(TImplementation));
    }

    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true" />
    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Implementation", Implementation);
    }
  }
}