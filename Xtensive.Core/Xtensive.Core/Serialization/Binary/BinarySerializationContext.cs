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

    /// <summary>
    /// Gets the value serializer provider.
    /// </summary>
    public IValueSerializerProvider<Stream> ValueSerializerProvider { get; protected set; }

    /// <summary>
    /// Gets the <see cref="Int32"/> value serializer.
    /// </summary>
    public ValueSerializer<Stream, int> IntSerializer { get; protected set; }

    /// <summary>
    /// Gets the <see cref="Int64"/> value serializer.
    /// </summary>
    public ValueSerializer<Stream, long> LongSerializer { get; protected set; }

    /// <summary>
    /// Gets the <see cref="String"/> value serializer.
    /// </summary>
    public ValueSerializer<Stream, string> StringSerializer { get; protected set; }

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
      var serializer = (BinarySerializer) base.Serializer;
      ValueSerializerProvider = serializer.ValueSerializerProvider;
      IntSerializer = ValueSerializerProvider.GetSerializer<int>();
      LongSerializer = ValueSerializerProvider.GetSerializer<long>();
      StringSerializer = ValueSerializerProvider.GetSerializer<string>();
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