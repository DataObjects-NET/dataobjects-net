// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Provides delegates allowing to call serialization methods faster.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IValueSerializer{TStream, T}"/> generic argument.</typeparam>
  /// <typeparam name="TStream">Type of the stream to write to or read from.</typeparam>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class ValueSerializer<TStream, T> :
    MethodCacheBase<IValueSerializer<TStream, T>>,
    IValueSerializer<TStream, T>
  {
    private static readonly object _lock = new object();
    private static volatile ValueSerializer<TStream, T> @default;

    /// <summary>
    /// Gets default serializer for type <typeparamref name="T"/>
    /// (uses <see cref="ValueSerializerProvider{TStream}.Default"/> <see cref="ValueSerializerProvider{TStream}"/>).
    /// </summary>
    [DebuggerHidden]
    public static ValueSerializer<TStream, T> Default {
      get {
        if (@default == null)
          lock (_lock)
            if (@default == null)
              try {
                var serializer =
                  new ValueSerializer<TStream, T>(ValueSerializerProvider<TStream>.Default.GetSerializer<T>());
                Thread.MemoryBarrier();
                @default = serializer;
              }
              catch {}
        return @default;
      }
    }

    /// <summary>
    /// Deserializes the data on the provided stream.
    /// </summary>
    public Func<TStream, T> Deserialize;

    /// <summary>
    /// Serializes an object to the provided stream.
    /// </summary>
    public Action<TStream, T> Serialize;

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation"><see cref="Formatter"/> to provide the delegates for.</param>
    public ValueSerializer(IValueSerializer<TStream, T> implementation)
      : base(
        implementation is ValueSerializer<TStream, T>
          ?
            ((ValueSerializer<TStream, T>) implementation).Implementation
          : implementation) {
      Deserialize = Implementation.Deserialize;
      Serialize = Implementation.Serialize;
    }

    /*/// <summary>
    /// Deserializes the instance of <see cref="Formatter"/>.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    private ValueSerializer(SerializationInfo info, StreamingContext context)
      : base(info, context) {
      Deserialize = Implementation.Deserialize;
      Serialize = Implementation.Serialize;
    }*/

    /// <inheritdoc/>
    void IValueSerializer<TStream, T>.Serialize(TStream stream, T graph) {
      Serialize.Invoke(stream, graph);
    }

    /// <inheritdoc/>
    T IValueSerializer<TStream, T>.Deserialize(TStream stream) {
      return Deserialize.Invoke(stream);
    }

    /// <inheritdoc/>
    public IValueSerializerProvider<TStream> Provider {
      get { return Implementation.Provider; }
    }

    #region IValueSerializer<TStream> Members

    void IValueSerializer<TStream>.Serialize(TStream stream, object value) {
      Serialize.Invoke(stream, (T) value);
    }

    object IValueSerializer<TStream>.Deserialize(TStream stream) {
      return Deserialize(stream);
    }

    #endregion
  }
}