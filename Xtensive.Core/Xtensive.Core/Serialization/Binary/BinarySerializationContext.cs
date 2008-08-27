// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// A context specific to binary serialization.
  /// </summary>
  [Serializable]
  public class BinarySerializationContext : SerializationContext
  {
    /// <summary>
    /// Gets current <see cref="Serializer"/>.
    /// </summary>
    public new BinarySerializer Serializer { get; private set; }

    /// <inheritdoc/>
    protected override SerializationDataReader CreateReader()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    protected override SerializationDataWriter CreateWriter()
    {
      throw new NotImplementedException();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="serializer">The serializer this context belongs to.</param>
    public BinarySerializationContext(BinarySerializer serializer)
      : base(serializer)
    {
      Serializer = serializer;
    }
  }
}