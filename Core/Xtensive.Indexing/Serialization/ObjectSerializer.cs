// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.20

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Threading;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Serialization
{
  /// <summary>
  /// Provides delegates allowing to call serialization methods faster.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IObjectSerializer{T}"/> generic argument.</typeparam>
  [Serializable]
  public sealed class ObjectSerializer<T> : MethodCacheBase<IObjectSerializer<T>>
  {
    /// <summary>
    /// Gets the provider this serializer is bound to.
    /// </summary>
    public IObjectSerializerProvider Provider { get; private set; }

    /// <summary>
    /// Gets <see cref="IObjectSerializer{T}.CreateObject"/> method delegate.
    /// </summary>
    public Func<Type, T> CreateObject;

    /// <summary>
    /// Gets <see cref="IObjectSerializer{T}.GetObjectData(T,T,SerializationData)"/> method delegate.
    /// </summary>
    public new Action<T, T, SerializationData> GetObjectData;

    /// <summary>
    /// Gets <see cref="IObjectSerializer{T}.SetObjectData(T,SerializationData)"/> method delegate.
    /// </summary>
    public Func<T, SerializationData, T> SetObjectData;

    /// <inheritdoc/>
    public bool IsReferable { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation">Serializer to provide the delegates for.</param>
    public ObjectSerializer(IObjectSerializer<T> implementation)
      : base(implementation) 
    {
      Provider = Implementation.Provider;
      IsReferable = Implementation.IsReferable;
      GetObjectData = Implementation.GetObjectData;
      SetObjectData = Implementation.SetObjectData;
      CreateObject = Implementation.CreateObject;
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true"/>
    public ObjectSerializer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Provider = Implementation.Provider;
      IsReferable = Implementation.IsReferable;
      GetObjectData = Implementation.GetObjectData;
      SetObjectData = Implementation.SetObjectData;
      CreateObject = Implementation.CreateObject;
    }
  }
}