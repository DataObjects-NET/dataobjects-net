// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Provides delegates allowing to call serialization methods faster.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IValueSerializer{T}"/> generic argument.</typeparam>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public sealed class ValueSerializer<T> : MethodCacheBase<IValueSerializer<T>>
  {
    private static readonly object _lock = new object();
    private static volatile ValueSerializer<T> @default;

    /// <summary>
    /// Gets default serializer for type <typeparamref name="T"/>
    /// (uses <see cref="ValueSerializerProvider.Default"/> <see cref="ValueSerializerProvider"/>).
    /// </summary>
    [DebuggerHidden]
    public static ValueSerializer<T> Default {
      get {
        if (@default==null) lock (_lock) if (@default==null) {
          try {
            @default = ValueSerializerProvider.Default.GetSerializer<T>();
          }
          catch {
          }
        }
        return @default;
      }
    }

    /// <summary>
    /// Deserializes the data on the provided stream.
    /// </summary>
    public Func<Stream, T> Deserialize;

    /// <summary>
    /// Serializes an object to the provided stream.
    /// </summary>
    public Action<Stream, T> Serialize;

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation"><see cref="Formatter"/> to provide the delegates for.</param>
    public ValueSerializer(IValueSerializer<T> implementation)
      : base(implementation)
    {
      Deserialize = Implementation.Deserialize;
      Serialize = Implementation.Serialize;
    }

    /// <summary>
    /// Deserializes the instance of <see cref="Formatter"/>.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    private ValueSerializer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Deserialize = Implementation.Deserialize;
      Serialize = Implementation.Serialize;
    }
  }
}