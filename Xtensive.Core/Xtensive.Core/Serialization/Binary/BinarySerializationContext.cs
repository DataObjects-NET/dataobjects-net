// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using System.Diagnostics;
using System.IO;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// A context specific to binary serialization.
  /// </summary>
  [Serializable]
  public class BinarySerializationContext : SerializationContext
  {
    /// <summary>
    /// Gets the current <see cref="BinarySerializationContext"/>.
    /// </summary>        
    public static BinarySerializationContext Current {
      [DebuggerStepThrough]
      get { return (BinarySerializationContext) Scope<SerializationContext>.CurrentContext; }
    }

    /// <summary>
    /// Gets current <see cref="Serializer"/>.
    /// </summary>
    public new BinarySerializer Serializer { get; private set; }

    protected override void Initialize()
    {
      switch (ProcessType) {
      case SerializerProcessType.Serialization:
        Writer = null;
        break;
      case SerializerProcessType.Deserialization:
        Reader = null;
        break;
      }
      base.Initialize();
    }


    // Constructors

    /// <inheritdoc/>
    public BinarySerializationContext(SerializerBase serializer, Stream stream, SerializerProcessType processType)
      : base(serializer, stream, processType)
    {
      Serializer = (BinarySerializer) serializer;
    }
  }
}