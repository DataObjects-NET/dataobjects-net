// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.03.28

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Serialization.Implementation
{
  /// <summary>
  /// Abstract base class for any record writer.
  /// </summary>
  public abstract class SerializationDataWriter : IDisposable
  {
    /// <summary>
    /// Initializes this instance.
    /// Called before starting the serialization.
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// Creates a new <see cref="SerializationData"/> instance.
    /// </summary>
    /// <param name="reference">The <see cref="SerializationData.Reference"/> property value.</param>
    /// <param name="source">The <see cref="SerializationData.Source"/> property value.</param>
    /// <param name="origin">The <see cref="SerializationData.Origin"/> property value.</param>
    /// <returns>New <see cref="SerializationData"/> instance.</returns>
    public abstract SerializationData Create(IReference reference, object source, object origin);

    /// <summary>
    /// Appends (writes) the specified <paramref name="data"/> to the end of the stream.
    /// </summary>
    /// <param name="data">Record to append.</param>
    public abstract void Append(SerializationData data);

    /// <summary>
    /// Called on successful completion of serialization.
    /// </summary>
    public virtual void Complete()
    {
    }

    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    public virtual void Dispose()
    {
    }
  }
}