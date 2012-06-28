// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Indexing.Serialization
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
    internal const string AssociateName = "ValueSerializer";
    private static ThreadSafeCached<ValueSerializer<T>> cachedSerializer =
      ThreadSafeCached<ValueSerializer<T>>.Create(new object());

    /// <summary>
    /// Gets default serializer for type <typeparamref name="T"/>
    /// (uses <see cref="ValueSerializerProvider.Default"/>).
    /// </summary>
    public static ValueSerializer<T> Default {
      [DebuggerStepThrough]
      get {
        return cachedSerializer.GetValue(
          () => ValueSerializerProvider.Default.GetSerializer<T>());
      }
    }
    /// <summary>
    /// Gets the provider this serializer is bound to.
    /// </summary>
    public IValueSerializerProvider Provider { get; private set; }

    /// <summary>
    /// Gets <see cref="IValueSerializer{T}.Serialize(Stream,T)"/> method delegate.
    /// </summary>
    public Action<Stream, T> Serialize;

    /// <summary>
    /// Gets <see cref="IValueSerializer{T}.Deserialize"/> method delegate.
    /// </summary>
    public Func<Stream, T> Deserialize;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation">Serializer to provide the delegates for.</param>
    public ValueSerializer(IValueSerializer<T> implementation)
      : base(implementation) 
    {
      Provider = Implementation.Provider;
      Deserialize = Implementation.Deserialize;
      Serialize = Implementation.Serialize;
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true"/>
    public ValueSerializer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Provider = Implementation.Provider;
      Deserialize = Implementation.Deserialize;
      Serialize = Implementation.Serialize;
    }
  }
}