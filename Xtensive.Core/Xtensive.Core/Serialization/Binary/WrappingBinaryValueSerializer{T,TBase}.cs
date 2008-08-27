// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.IO;
using Xtensive.Core.Resources;
using Xtensive.Core.Reflection;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Base class for any wrapping <see cref="IValueSerializer{Stream, T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to serialize.</typeparam>
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingBinaryValueSerializer<T, TBase> : BinaryValueSerializerBase<T>
  {
    /// <summary>
    /// Serializer for base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<Stream, TBase> BaseSerializer;

    // Constructors

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Value serializer for <typeparamref name="TBase"/> is not found.</exception>
    protected WrappingBinaryValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      var serializer = provider.GetSerializer<TBase>();
      if (serializer == null)
        throw new InvalidOperationException(string.Format(
          Strings.ExCantFindAssociate,
          ValueSerializer<Stream,TBase>.AssociateName,
          typeof(IValueSerializer<Stream, TBase>).GetShortName(),
          typeof(TBase).GetShortName()));
      BaseSerializer = new ValueSerializerStruct<Stream, TBase>(serializer);
    }
  }
}