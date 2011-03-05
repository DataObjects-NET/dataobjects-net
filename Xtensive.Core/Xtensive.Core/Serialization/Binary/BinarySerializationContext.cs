// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using System.Diagnostics;
using System.IO;
using Xtensive.IoC;

namespace Xtensive.Serialization.Binary
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
    public static new BinarySerializationContext Current {
      [DebuggerStepThrough]
      get { return (BinarySerializationContext) Scope<SerializationContext>.CurrentContext; }
    }

    /// <summary>
    /// Gets current <see cref="Serializer"/>.
    /// </summary>
    public new BinarySerializer Serializer { get; private set; }

    protected override void Initialize()
    {
      base.Initialize();
      switch (ProcessType) {
      case SerializerProcessType.Serialization:
        Writer = new BinarySerializationDataWriter();
        break;
      case SerializerProcessType.Deserialization:
        Reader = new BinarySerializationDataReader();
        break;
      }
    }


    // Constructors

    /// <inheritdoc/>
    public BinarySerializationContext(WorkingSerializerBase serializer, Stream stream, SerializerProcessType processType)
      : base(serializer, stream, processType)
    {
      Serializer = (BinarySerializer) serializer;
    }
  }
}