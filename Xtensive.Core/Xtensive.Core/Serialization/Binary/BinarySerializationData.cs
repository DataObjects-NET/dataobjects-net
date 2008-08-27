// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using System.Diagnostics;
using System.IO;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// A <see cref="SerializationData"/> specific to binary serialization.
  /// </summary>
  [Serializable]
  public class BinarySerializationData : SerializationData<Stream>
  {
    /// <summary>
    /// Gets the <see cref="SerializationContext"/> this instance belongs to.
    /// </summary>
    public new BinarySerializationContext Context {
      [DebuggerStepThrough]
      get { return (BinarySerializationContext) base.Context; }
    }

    /// <summary>
    /// Gets the underlying stream for this instance.
    /// </summary>
    public Stream Stream { get; private set; }

    /// <inheritdoc/>
    public override void AddValue<T>(string name, T value)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override T GetValue<T>(string name)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    protected override void EnsureIsRead()
    {
      throw new NotImplementedException();
    }


    // Constructors

    /// <inheritdoc/>
    public BinarySerializationData()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> property value.</param>
    public BinarySerializationData(Stream stream)
    {
      Stream = stream;
    }

    /// <inheritdoc/>
    public BinarySerializationData(IReference reference, object source, object origin, bool preferNesting)
      : base(reference, source, origin, preferNesting)
    {
    }
  }
}