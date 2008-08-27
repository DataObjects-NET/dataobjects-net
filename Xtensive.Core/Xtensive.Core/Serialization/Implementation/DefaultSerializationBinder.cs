// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Implementation
{
  /// <summary>
  /// Default <see cref="SerializationBinder"/> implementation.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="SingletonDocTemplate" copy="true" /></para>
  /// </remarks>
  public class DefaultSerializationBinder : SerializationBinder
  {
    private static readonly DefaultSerializationBinder instance = 
      new DefaultSerializationBinder();

    /// <see cref="SingletonDocTemplate.Instance" copy="true" />
    public static DefaultSerializationBinder Instance {
      [DebuggerStepThrough]
      get { return instance; }
    }

    /// <inheritdoc/>
    public override Type BindToType(string assemblyName, string typeName) 
    {
      return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
    }


    // Constructors
    
    private DefaultSerializationBinder()
    {
    }
  }
}