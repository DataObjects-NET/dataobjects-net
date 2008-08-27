// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.25

using System;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.Serialization.Implementation
{
  /// <summary>
  /// Abstract base class for any serializer.
  /// </summary>
  /// <typeparam name="TStream">The type of the stream to deal with.</typeparam>
  [Serializable]
  public abstract class SerializerBase<TStream> : ConfigurableBase<SerializerConfiguration>,
    ISerializer<TStream>
  {
    /// <inheritdoc/>
    public void Serialize(TStream stream, object source)
    {
      Serialize(stream, source, null);
    }

    /// <inheritdoc/>
    public object Deserialize(TStream stream)
    {
      return Deserialize(stream, null);
    }

    /// <inheritdoc/>
    public object Clone(object source)
    {
      TStream stream = CreateCloningStream();
      Serialize(stream, source);
      RewindCloningStream(ref stream);
      return Deserialize(stream);
    }

    /// <inheritdoc/>
    public abstract void Serialize(TStream stream, object source, object origin);

    /// <inheritdoc/>
    public abstract object Deserialize(TStream stream, object origin);

    /// <summary>
    /// Creates the stream for <see cref="Clone"/> operation.
    /// </summary>
    /// <returns>Newly created stream.</returns>
    protected abstract TStream CreateCloningStream();

    /// <summary>
    /// Rewinds the cloning stream to the beginning.
    /// </summary>
    /// <param name="stream">The stream to rewind.</param>
    protected abstract void RewindCloningStream(ref TStream stream);

    
    // Constructors

    /// <inheritdoc/>
    protected SerializerBase()
    {
    }

    /// <inheritdoc/>
    protected SerializerBase(SerializerConfiguration configuration)
      : base(configuration)
    {
    }
  }
}