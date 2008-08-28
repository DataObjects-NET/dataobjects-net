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
  /// <typeparam name="TBase3">Third base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingBinaryValueSerializer<T, TBase1, TBase2, TBase3> : WrappingBinaryValueSerializer<T, TBase1, TBase2>
  {
    /// <summary>
    /// Serializer for third base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<Stream, TBase3> BaseSerializer3;


    // Constructors

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Value serializer for 
    /// <typeparamref name="TBase1"/> or <typeparamref name="TBase2"/> or <typeparamref name="TBase3"/> is not found.</exception>
    protected WrappingBinaryValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      var serializer3 = provider.GetSerializer<TBase3>();
      if (serializer3 == null)
        throw new InvalidOperationException(string.Format(
          Strings.ExCantFindAssociate,
          ValueSerializer<Stream,TBase3>.AssociateName,
          typeof(IValueSerializer<Stream, TBase3>).GetShortName(),
          typeof(TBase3).GetShortName()));
      BaseSerializer3 = new ValueSerializerStruct<Stream, TBase3>(serializer3);
    }
  }
}