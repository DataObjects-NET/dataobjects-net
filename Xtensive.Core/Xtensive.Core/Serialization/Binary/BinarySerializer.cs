// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using System.Diagnostics;
using System.IO;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Binary serializer implementation.
  /// </summary>
  [Serializable]
  public class BinarySerializer : Serializer<Stream>
  {
    /// <summary>
    /// Gets the serialization context instance used by this serializer.
    /// </summary>
    protected new BinarySerializationContext Context {
      [DebuggerStepThrough]
      get { return (BinarySerializationContext) base.Context; }
      [DebuggerStepThrough]
      set { base.Context = value; }
    }

    /// <inheritdoc/>
    protected override void OnConfigured()
    {
      ObjectSerializerProvider = Implementation.ObjectSerializerProvider.Default;
      ValueSerializerProvider  = BinaryValueSerializerProvider.Default;
      Context = new BinarySerializationContext(this);
      base.OnConfigured();
    }


    // Constructors

    /// <inheritdoc/>
    public BinarySerializer()
    {
    }

    /// <inheritdoc/>
    public BinarySerializer(SerializerConfiguration configuration)
      : base(configuration)
    {
    }
  }
}