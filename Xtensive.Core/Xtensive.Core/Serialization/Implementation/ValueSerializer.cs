// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Implementation
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
  public class ValueSerializer<TStream, T> : MethodCacheBase<IValueSerializer<TStream, T>>
  {
    internal const string AssociateName = "ValueSerializer";

    /// <summary>
    /// Gets the provider this serializer is bound to.
    /// </summary>
    public IValueSerializerProvider<TStream> Provider { get; private set; }

    /// <summary>
    /// Gets <see cref="IValueSerializer{TStream,T}.Serialize(TStream,T)"/> method delegate.
    /// </summary>
    public Action<TStream, T> Serialize;

    /// <summary>
    /// Gets <see cref="IValueSerializer{TStream,T}.Deserialize"/> method delegate.
    /// </summary>
    public Func<TStream, T> Deserialize;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation">Serializer to provide the delegates for.</param>
    public ValueSerializer(IValueSerializer<TStream, T> implementation)
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