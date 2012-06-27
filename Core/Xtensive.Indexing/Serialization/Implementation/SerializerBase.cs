// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.25

using System;
using System.IO;
using Xtensive.Configuration;

namespace Xtensive.Indexing.Serialization.Implementation
{
  /// <summary>
  /// Abstract base class for any serializer.
  /// </summary>
  [Serializable]
  public abstract class SerializerBase : ConfigurableBase<SerializerConfiguration>,
    ISerializer
  {
    /// <inheritdoc/>
    public void Serialize(Stream stream, object source)
    {
      Serialize(stream, source, null);
    }

    /// <inheritdoc/>
    public object Deserialize(Stream stream)
    {
      return Deserialize(stream, null);
    }

    /// <inheritdoc/>
    public object Clone(object source)
    {
      Stream stream = new MemoryStream();
      Serialize(stream, source);
      stream.Position = 0;
      return Deserialize(stream);
    }

    /// <inheritdoc/>
    public abstract void Serialize(Stream stream, object source, object origin);

    /// <inheritdoc/>
    public abstract object Deserialize(Stream stream, object origin);

    
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