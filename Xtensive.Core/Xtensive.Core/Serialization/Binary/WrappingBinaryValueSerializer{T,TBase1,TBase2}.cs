// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.IO;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Base class for any wrapping <see cref="IValueSerializer{Stream, T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to serialize.</typeparam>
  /// <typeparam name="TBase1">First base (wrapped) type.</typeparam>
  /// <typeparam name="TBase2">Second base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingBinaryValueSerializer<T, TBase1, TBase2> : BinaryValueSerializerBase<T>
  {
    /// <summary>
    /// Serializer for first base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<Stream, TBase1> BaseSerializer1;

    /// <summary>
    /// Serializer for second base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<Stream, TBase2> BaseSerializer2;


    // Constructors

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Value serializer for 
    /// <typeparamref name="TBase1"/> or <typeparamref name="TBase2"/> is not found.</exception>
    protected WrappingBinaryValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      var serializer1 = provider.GetSerializer<TBase1>();
      if (serializer1 == null)
        throw new InvalidOperationException(string.Format(
          Strings.ExCantFindAssociate,
          ValueSerializer<Stream,TBase1>.AssociateName,
          typeof(IValueSerializer<Stream, TBase1>).GetShortName(),
          typeof(TBase1).GetShortName()));
      BaseSerializer1 = new ValueSerializerStruct<Stream, TBase1>(serializer1);
      var serializer2 = provider.GetSerializer<TBase2>();
      if (serializer2 == null)
        throw new InvalidOperationException(string.Format(
          Strings.ExCantFindAssociate,
          ValueSerializer<Stream,TBase2>.AssociateName,
          typeof(IValueSerializer<Stream, TBase2>).GetShortName(),
          typeof(TBase2).GetShortName()));
      BaseSerializer2 = new ValueSerializerStruct<Stream, TBase2>(serializer2);
    }
  }
}