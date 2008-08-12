// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.20

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization
{
  [Serializable]
  public sealed class Serializer<T> : MethodCacheBase<ISerializer<T>>, ISerializer
  {
    private static readonly object _lock = new object();
    private static volatile Serializer<T> @default;

    [DebuggerStepThrough]
    public static Serializer<T> Default {
      get {
        if (@default==null) lock (_lock) if (@default==null) {
          try {
            @default = SerializerProvider.Default.GetObjectSerializer<T>();
          }
          catch {
          }
        }
        return @default;
      }
    }

    /// <inheritdoc/>
    object ISerializer.CreateObject(SerializationData data, SerializationContext context)
    {
      return CreateObject(data, context);
    }

    /// <inheritdoc/>
    void ISerializer.GetObjectData(object obj, SerializationData data, SerializationContext context)
    {
      GetObjectData((T)obj, data, context);
    }

    /// <inheritdoc/>
    void ISerializer.SetObjectData(object obj, SerializationData data, SerializationContext context)
    {
      SetObjectData((T)obj, data, context);
    }

    /// <summary>
    /// Gets <see cref="ISerializer{T}.GetObjectData"/> method delegate.
    /// </summary>
    public new Action<T, SerializationData, SerializationContext> GetObjectData;

    /// <summary>
    /// Gets <see cref="ISerializer{T}.SetObjectData"/> method delegate.
    /// </summary>
    public Action<T, SerializationData, SerializationContext> SetObjectData;

    /// <summary>
    /// Gets <see cref="ISerializer{T}.CreateObject"/> method delegate.
    /// </summary>
    public Func<SerializationData, SerializationContext, T> CreateObject;

    /// <summary>
    /// <see cref="Serializer{T}"/> provider.
    /// </summary>
    public readonly ISerializerProvider Provider;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation">Serializer to provide the delegates for.</param>
    public Serializer(ISerializer<T> implementation)
      : base(implementation)
    {
      GetObjectData = Implementation.GetObjectData;
      SetObjectData = Implementation.SetObjectData;
      CreateObject = Implementation.CreateObject;
      Provider = Implementation.Provider;
    }

    private Serializer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      GetObjectData = Implementation.GetObjectData;
      SetObjectData = Implementation.SetObjectData;
      CreateObject = Implementation.CreateObject;
      Provider = Implementation.Provider;
    }
  }
}