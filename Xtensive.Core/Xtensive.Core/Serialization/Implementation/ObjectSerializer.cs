// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.20

using System;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Provides delegates allowing to call serialization methods faster.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IObjectSerializer{T}"/> generic argument.</typeparam>
  [Serializable]
  public sealed class ObjectSerializer<T> :
    MethodCacheBase<IObjectSerializer<T>>,
    IObjectSerializer<T>
  {
    private static readonly object _lock = new object();
    private static volatile ObjectSerializer<T> @default;

    /// <summary>
    /// Gets default serializer for type <typeparamref name="T"/>
    /// (uses <see cref="ObjectSerializerProvider.Default"/>).
    /// </summary>
    [DebuggerHidden]
    public static ObjectSerializer<T> Default {
      get {
        if (@default == null)
          lock (_lock)
            if (@default == null)
              try {
                var serializer = new ObjectSerializer<T>(ObjectSerializerProvider.Default.GetSerializer<T>());
                Thread.MemoryBarrier();
                @default = serializer;
              }
              catch {}
        return @default;
      }
    }

    #region IObjectSerializer members

    /// <inheritdoc/>
    object IObjectSerializer.CreateObject() {
      return CreateObject();
    }

    /// <inheritdoc/>
    void IObjectSerializer.GetObjectData(object obj, SerializationData data) 
    {
      GetObjectData((T) obj, data);
    }

    /// <inheritdoc/>
    object IObjectSerializer.SetObjectData(object obj, SerializationData data) 
    {
      return SetObjectData((T) obj, data);
    }

    #endregion

    #region IObjectSerializer<T> members

    /// <inheritdoc/>
    T IObjectSerializer<T>.CreateObject() {
      return CreateObject();
    }

    /// <inheritdoc/>
    void IObjectSerializer<T>.GetObjectData(T obj, SerializationData data) {
      GetObjectData(obj, data);
    }

    /// <inheritdoc/>
    T IObjectSerializer<T>.SetObjectData(T obj, SerializationData data) {
      return SetObjectData(obj, data);
    }

    #endregion

    /// <summary>
    /// Gets <see cref="IObjectSerializer{T}.GetObjectData(T,SerializationData)"/> method delegate.
    /// </summary>
    public new Action<T, SerializationData> GetObjectData;

    /// <summary>
    /// Gets <see cref="IObjectSerializer{T}.SetObjectData(T,SerializationData)"/> method delegate.
    /// </summary>
    public Func<T, SerializationData, T> SetObjectData;

    /// <summary>
    /// Gets <see cref="IObjectSerializer{T}.CreateObject"/> method delegate.
    /// </summary>
    public Func<T> CreateObject;

    private readonly IObjectSerializerProvider provider;

    /// <inheritdoc/>
    public IObjectSerializerProvider Provider {
      get { return provider; }
    }

    /// <inheritdoc/>
    public bool IsReferable { get; private set; }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation">Serializer to provide the delegates for.</param>
    public ObjectSerializer(IObjectSerializer<T> implementation)
      : base(implementation.GetType().GetField("Implementation") != null
        ? implementation.GetType().GetField("Implementation").GetValue(implementation) as IObjectSerializer<T>
        : implementation) {
      GetObjectData = Implementation.GetObjectData;
      SetObjectData = Implementation.SetObjectData;
      CreateObject = Implementation.CreateObject;
      IsReferable = Implementation.IsReferable;
      provider = Implementation.Provider;
    }
  }
}