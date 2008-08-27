// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.25

using System;
using System.IO;

namespace Xtensive.Core.Serialization.Implementation
{
  /// <summary>
  /// Abstract base class for <see cref="Stream"/>-based serializers.
  /// </summary>
  [Serializable]
  public abstract class SerializerBase : SerializerBase<Stream>
  {
    /// <inheritdoc/>
    protected override Stream CreateCloningStream()
    {
      return new MemoryStream();
    }

    /// <inheritdoc/>
    protected override void RewindCloningStream(ref Stream stream)
    {
      stream.Seek(0, SeekOrigin.Begin);
    }

    
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