// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.Diagnostics;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Serialization.Implementation
{
  [Serializable]
  public abstract class SerializationData<TInnerStream> : SerializationData
  {
    /// <summary>
    /// Gets the serializer this instance is bound to.
    /// </summary>
    public Serializer<TInnerStream> Serializer {
      [DebuggerStepThrough]
      get { return Context.Serializer as Serializer<TInnerStream>; }
    }

    /// <inheritdoc/>
    public sealed override SerializationData AddObject(string name, object value, object originValue, bool preferNesting)
    {
      var data = Serializer.GetObjectData(value, originValue, preferNesting);
      AddValue(name, data);
      return data;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Method is invoked for an object, which requires use of <see cref="GetReference"/>.</exception>
    public override T GetObject<T>(string name, object originValue)
    {
      var data = GetValue<SerializationData>(name);
      var source = Serializer.SetObjectData(Origin, data);
      if (data.Reference!=null)
        throw Exceptions.InternalError(
          Strings.ExInvalidSerializerBehaviorGetObjectIsUsedInsteadOfGetReference, Log.Instance);
      return (T) source;
    }

    /// <inheritdoc/>
    public override IReference GetReference(string name, object originValue)
    {
      var data = GetValue<SerializationData>(name);
      var source = Serializer.SetObjectData(Origin, data);
      if (data.Reference!=null)
        // We've just deserialized an object with reference
        return data.Reference;
      else
        // We've just deserialized a reference or something else, which isn't referable
        // (the last case will lead to type cast failure)
        return (IReference) source;
    }


    // Constructors

    /// <inheritdoc/>
    protected SerializationData()
    {
    }

    /// <inheritdoc/>
    protected SerializationData(IReference reference, object source, object origin, bool preferNesting)
      : base(reference, source, origin)
    {
    }
  }
}