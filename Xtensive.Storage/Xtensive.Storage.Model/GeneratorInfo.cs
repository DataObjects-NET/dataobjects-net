// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.13

using System;
using System.Diagnostics;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class GeneratorInfo : MappingNode
  {
    private int cacheSize;

    /// <summary>
    /// Gets or sets the size of the generator cache.
    /// </summary>
    public int CacheSize {
      [DebuggerStepThrough]
      get { return cacheSize; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        cacheSize = value;
      }
    }

    /// <summary>
    /// Gets the <see cref="KeyInfo"/> property for this instance.
    /// </summary>
    public KeyInfo KeyInfo { get; private set; }

    /// <summary>
    /// Gets the type instance of which is responsible for key generation.
    /// </summary>
    public Type KeyGeneratorType { get; private set; }

    /// <summary>
    /// Gets or sets the tuple descriptor for key.
    /// </summary>
    public TupleDescriptor TupleDescriptor
    {
      get { return KeyInfo.TupleDescriptor; }
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="keyInfo">The key info.</param>
    public GeneratorInfo(Type type, KeyInfo keyInfo)
    {
      KeyInfo = keyInfo;
      KeyGeneratorType = type;
    }
  }
}