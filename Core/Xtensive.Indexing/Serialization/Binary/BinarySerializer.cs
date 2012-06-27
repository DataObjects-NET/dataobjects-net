// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using System.Diagnostics;
using System.IO;
using Xtensive.Internals.DocTemplates;
using Xtensive.Threading;

namespace Xtensive.Indexing.Serialization.Binary
{
  /// <summary>
  /// Binary serializer implementation.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="SingletonDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class BinarySerializer : WorkingSerializerBase
  {
    private static ThreadSafeCached<BinarySerializer> cachedInstance =
      ThreadSafeCached<BinarySerializer>.Create(new object());

    /// <see cref="SingletonDocTemplate.Instance" copy="true"/>
    public static BinarySerializer Instance {
      [DebuggerStepThrough]
      get {
        return cachedInstance.GetValue(
          () => new BinarySerializer(new SerializerConfiguration()));
      }
    }

    /// <inheritdoc/>
    protected override SerializationContext CreateContext(Stream stream, SerializerProcessType processType)
    {
      return new BinarySerializationContext(this, stream, processType);
    }

    /// <inheritdoc/>
    protected override void OnConfigured()
    {
      ObjectSerializerProvider = BinaryObjectSerializerProvider.Default;
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