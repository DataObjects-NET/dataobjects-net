// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.Diagnostics;
using System.IO;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Resources;
using Xtensive.Serialization.Implementation;

namespace Xtensive.Serialization
{
  /// <summary>
  /// Serializes and deserializes an object, or an entire graph of connected objects.
  /// </summary>
  public abstract class WorkingSerializerBase : SerializerBase
  {
    private IObjectSerializerProvider objectSerializerProvider;
    private IValueSerializerProvider  valueSerializerProvider;

    /// <summary>
    /// Gets or sets the object serializer provider.
    /// </summary>
    public IObjectSerializerProvider ObjectSerializerProvider
    {
      get { return objectSerializerProvider; }
      protected set {
        objectSerializerProvider = value;
        valueSerializerProvider = value!=null ? value.ValueSerializerProvider : null;
      }
    }

    /// <summary>
    /// Gets the value serializer provider.
    /// </summary>
    public IValueSerializerProvider ValueSerializerProvider
    {
      get { return valueSerializerProvider; }
    }

    /// <summary>
    /// Creates a new <see cref="SerializationContext"/> for serialization or deserialization.
    /// </summary>
    /// <param name="processType">Type of the process to prepare for.</param>
    /// <param name="stream">The stream to use.</param>
    /// <returns>Newly created <see cref="SerializationContext"/> instance.</returns>
    protected abstract SerializationContext CreateContext(Stream stream, SerializerProcessType processType);

    #region ISerializer methods

    /// <inheritdoc/>
    public override void Serialize(Stream stream, object source, object origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");

      using (var context = CreateContext(stream, SerializerProcessType.Serialization))
      using (context.Activate()) {
        var writer = context.Writer;
        writer.Initialize();
        writer.Append(GetObjectData(source, origin, true));
        var queue = context.SerializationQueue;
        while (queue.Count > 0) {
          var nextSource = queue.BottomKey;
          var nextPair = queue.PopBottom();
          queue.Remove(nextSource);
          writer.Append(GetObjectData(nextSource, nextPair.Second, true));
        }
        writer.Complete();
      }
    }

    /// <inheritdoc/>
    public override object Deserialize(Stream stream, object origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");

      bool bFirst = true;
      using (var context = CreateContext(stream, SerializerProcessType.Deserialization))
      using (context.Activate()) {
        var reader = context.Reader;
        foreach (var data in reader) {
          if (bFirst) {
            bFirst = false;
            origin = SetObjectData(origin, data);
          }
          else {
            SetObjectData(null, data);
          }
        }
        context.FixupManager.Execute();
      }
      return origin;
    }

    #endregion

    #region HasXxx, EnsureXxx methods

    /// <summary>
    /// Indicates if type <typeparamref name="T"/> has associated <see cref="IValueSerializer{T}"/>;
    /// otherwise is must have associated <see cref="IObjectSerializer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns><see langword="True"/> if <typeparamref name="T"/> has associated 
    /// <see cref="IValueSerializer{T}"/>;
    /// otherwise is must have associated <see cref="IObjectSerializer{T}"/>.</returns>
    public bool HasValueSerializer<T>() 
    {
      return ValueSerializerProvider.GetSerializer<T>()!=null;
    }

    /// <summary>
    /// Ensures the <typeparamref name="T"/> type is associated 
    /// with <see cref="IValueSerializer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="T"/> is associated 
    /// with <see cref="IObjectSerializer"/>.</exception>
    public void EnsureHasValueSerializer<T>() 
    {
      if (!HasValueSerializer<T>())
        throw new InvalidOperationException(string.Format(
          Strings.ExInvalidSerializerType,
          typeof(IObjectSerializer).GetShortName(),
          typeof(IValueSerializer<>).GetShortName()));
    }

    /// <summary>
    /// Ensures the <typeparamref name="T"/> type is associated 
    /// with <see cref="IObjectSerializer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="T"/> is associated 
    /// with <see cref="IValueSerializer{T}"/>.</exception>
    public void EnsureHasObjectSerializer<T>() 
    {
      if (HasValueSerializer<T>())
        throw new InvalidOperationException(string.Format(
          Strings.ExInvalidSerializerType,
          typeof(IValueSerializer<>).GetShortName(),
          typeof(IObjectSerializer).GetShortName()));
    }

    /// <summary>
    /// Ensures the object serializer is found.
    /// </summary>
    /// <param name="serializer">The serializer to check.</param>
    /// <param name="type">The type it was acquired for.</param>
    /// <exception cref="InvalidOperationException"><paramref name="serializer"/> is <see langword="null"/>.</exception>
    public void EnsureObjectSerializerIsFound(object serializer, Type type)
    {
      if (serializer==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExCantFindAssociate,
          "Serializer",
          typeof(IObjectSerializer).GetShortName(),
          type.GetShortName()));
    }

    /// <summary>
    /// Ensures the value serializer is found.
    /// </summary>
    /// <param name="serializer">The serializer to check.</param>
    /// <param name="type">The type it was acquired for.</param>
    /// <exception cref="InvalidOperationException"><paramref name="serializer"/> is <see langword="null"/>.</exception>
    public void EnsureValueSerializerIsFound(object serializer, Type type)
    {
      if (serializer==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExCantFindAssociate,
          "ValueSerializer",
          typeof(IValueSerializer).GetShortName(),
          type.GetShortName()));
    }

    /// <summary>
    /// Ensures the value serializer is found.
    /// </summary>
    /// <typeparam name="T">The type it was acquired for.</typeparam>
    /// <param name="serializer">The serializer to check.</param>
    /// <exception cref="InvalidOperationException"><paramref name="serializer"/> is <see langword="null"/>.</exception>
    public void EnsureValueSerializerIsFound<T>(object serializer)
    {
      if (serializer==null)
        EnsureValueSerializerIsFound(serializer, typeof(T));
    }

    #endregion

    #region Private \ internal methods

    /// <exception cref="InvalidOperationException">Wrong behavior of some component used in serialization.</exception>
    internal SerializationData GetObjectData(object source, object origin, bool immediately)
    {
      var context = SerializationContext.Current;
      var referenceManager = context.ReferenceManager;

      IReference reference;
      if (source==null) {
        // Source is null
        reference = referenceManager.GetReference(source);
        return GetObjectData(reference, null, true);
      }

      var writer = context.Writer;
      reference = source as IReference;
      if (reference!=null) {
        // Source is IReference, so it must be serialized as-is and right now (with nesting)
        var os = ObjectSerializerProvider.GetSerializerByInstance(reference);
        EnsureObjectSerializerIsFound(os, reference.GetType());
        if (os.IsReferable)
          throw Exceptions.InternalError(string.Format(
            Strings.ExInvalidSerializerBehaviorMustNotBeReferable, 
            os.GetType().GetShortName()), 
            Log.Instance);
        origin = os.CreateObject(reference.GetType());
        var data = writer.Create(null, reference, origin);
        InnerGetObjectData(os, data);
        return data;
      }
      else {
        // Source is some object
        var os = ObjectSerializerProvider.GetSerializerByInstance(source);
        EnsureObjectSerializerIsFound(os, source.GetType());
        immediately |= !source.GetType().IsClass; // always on for structs 
        if (os.IsReferable) {
          // Source can be referenced
          var queue = context.SerializationQueue;
          if (queue.Contains(source)) {
            // Already queued for the serialization
            reference = queue[source].First; // Getting existing reference
            if (!immediately)
              // Let's serialize just a reference to it here
              return GetObjectData(reference, null, true);
            else
              // We must serialize it now, so let's dequeue it
              queue.Remove(source);
          }
          else {
            // There is no object in queue
            bool isNew;
            reference = referenceManager.GetReference(source, out isNew);
            if (!immediately) {
              // Immediate serialization isn't required,
              // so we should enqueue it and serialize a reference to it
              if (isNew)
                queue.AddToTop(source, new Pair<IReference, object>(reference, origin));
              return GetObjectData(reference, null, true);
            }
          }
          // Ok, we must serialize the object itself here
          if (origin==null)
            origin = os.CreateObject(source.GetType());
          var data = writer.Create(reference, source, origin);
          InnerGetObjectData(os, data);
          return data;
        }
        else {
          // Source can't be referenced (e.g. struct)
          if (origin==null)
            origin = os.CreateObject(source.GetType());
          var data = writer.Create(null, source, origin);
          InnerGetObjectData(os, data);
          return data;
        }
      }
    }

    internal object SetObjectData(object origin, SerializationData data)
    {
      var context = SerializationContext.Current;
      var map = context.DeserializationMap;
      
      Type type = data.Type ?? data.SerializedType; // To avoid reading Type twice
      var os = ObjectSerializerProvider.GetSerializer(type);
      EnsureObjectSerializerIsFound(os, type);
      if (typeof(IReference).IsAssignableFrom(type)) {
        // We're deserializing an IReference, so everything is straighten forward
        data.Origin = os.CreateObject(type);
        InnerSetObjectData(os, data);
        var reference = (IReference) data.Source;
        if (origin!=null) {
          // The origin is provided, so we must try to update 
          // the existing object on its deserialization
          if (!map.ContainsKey(reference)) 
            map.Add(reference, origin);
          // Otherwise: old graph was containing two refs to the same object,
          // but these refs at the new one point to different ones.
          // In this case, we'll update just the first one of them, and assign 
          // the updated object to both all refs
        }
        return reference;
      }
      else {
        // We're deserializing an object
        IReference reference = null;
        if (os.IsReferable)
          reference = data.Reference ?? data.SerializedReference; // To avoid reading Reference twice
        if (origin==null) {
          // No origin, so we must acquire it
          object oldOrigin;
          if (reference!=null && map.TryGetValue(reference, out oldOrigin)) {
            // We've found an origin provided earlier
            origin = oldOrigin;
            map.Remove(reference);
          }
          else
            // Noting is found; using the default one
            origin = os.CreateObject(type);
        }
        data.Origin = origin;
        InnerSetObjectData(os, data);
        return data.Source;
      }
    }

    private static void InnerGetObjectData(IObjectSerializer objectSerializer, SerializationData data)
    {
      var context = SerializationContext.Current;
      var path = context.Path;
      bool oldPreferNesting = context.PreferNesting;
      path.Push(data);
      if (data.Source is IReference)
        context.PreferNesting = true; // IReferences must be always fully serialized "right now"
      try {
        objectSerializer.GetObjectData(data);
      }
      finally {
        context.PreferNesting = oldPreferNesting;
        path.Pop();
      }
    }

    private static void InnerSetObjectData(IObjectSerializer objectSerializer, SerializationData data)
    {
      var context = SerializationContext.Current;
      var path = context.Path;
      path.Push(data);
      try {
        objectSerializer.SetObjectData(data);
        data.EnsureNoSkips();
      }
      finally {
        path.Pop();
      }
    }

    #endregion


    // Constructors

    /// <inheritdoc/>
    protected WorkingSerializerBase()
    {
    }

    /// <inheritdoc/>
    protected WorkingSerializerBase(SerializerConfiguration configuration)
      : base(configuration)
    {
    }
  }
}