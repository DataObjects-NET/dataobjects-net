// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.08.13

using System;
using System.IO;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Base class for any binary value serializer.
  /// A version of <see cref="ValueSerializerBase{TStream,T}"/> for binary serialization.
  /// </summary>
  public abstract class BinaryValueSerializerBase<T> : ValueSerializerBase<Stream, T>
  {
    /// <summary>
    /// A thread-static buffer, that can be used during the serialization.
    /// </summary>
    [ThreadStatic]
    protected static byte[] ThreadBuffer;
    
    /// <summary>
    /// Gets the length of the output produced by the serializer.
    /// <see langword="-1" /> means it may vary.
    /// </summary>
    public int OutputLength { get; protected set; }

    /// <summary>
    /// Ensures the <see cref="ThreadBuffer"/> is initialized.
    /// </summary>
    /// <param name="size">The required buffer size.</param>
    protected static void EnsureThreadBufferIsInitialized(int size)
    {
      if (ThreadBuffer==null)
        ThreadBuffer = new byte[size];
    }
    

    // Constructors

    /// <inheritdoc/>
    protected BinaryValueSerializerBase(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      OutputLength = -1;
    }

    // IDeserializationCallback methods

    /// <inheritdoc/>
    public override void OnDeserialization(object sender)
    {
      if (Provider==null || Provider.GetType()==typeof (BinaryValueSerializerProvider))
        Provider = BinaryValueSerializerProvider.Default;
    }
  }
}