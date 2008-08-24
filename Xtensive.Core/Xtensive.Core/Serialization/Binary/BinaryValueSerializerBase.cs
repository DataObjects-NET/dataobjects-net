// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.08.13

using System.IO;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Implementation of <see cref="ValueSerializerBase{TStream,T}"/> for binary (de)serializing.
  /// </summary>
  public abstract class BinaryValueSerializerBase<T> : ValueSerializerBase<Stream, T>
  {
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected BinaryValueSerializerBase(IBinaryValueSerializerProvider provider)
      : base(provider) {}
  }
}