// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.08.13

using System.IO;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Base class for any binary value serializer.
  /// A version of <see cref="ValueSerializerBase{TStream,T}"/> for binary serialization.
  /// </summary>
  public abstract class BinaryValueSerializerBase<T> : ValueSerializerBase<Stream, T>
  {
    // Constructors

    /// <inheritdoc/>
    protected BinaryValueSerializerBase(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
    }

    // IDeserializationCallback methods

    /// <inheritdoc/>
    public override void OnDeserialization(object sender)
    {
      if (Provider==null || Provider.GetType()==typeof (BinaryValueSerializerProvider))
        Provider = BinaryValueSerializerProvider.Default;
    }
  }
}