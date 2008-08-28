// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using Xtensive.Core.Resources;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Base class for any wrapping <see cref="IValueSerializer{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to serialize.</typeparam>
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingValueSerializer<T, TBase> : ValueSerializerBase<T>
  {
    /// <summary>
    /// Serializer for base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<TBase> BaseSerializer;


    // Constructors

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Value serializer for <typeparamref name="TBase"/> is not found.</exception>
    protected WrappingValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      var serializer = provider.GetSerializer<TBase>();
      if (serializer == null)
        throw new InvalidOperationException(string.Format(
          Strings.ExCantFindAssociate,
          ValueSerializer<TBase>.AssociateName,
          typeof(IValueSerializer<TBase>).GetShortName(),
          typeof(TBase).GetShortName()));
      BaseSerializer = new ValueSerializerStruct<TBase>(serializer);
    }
  }
}