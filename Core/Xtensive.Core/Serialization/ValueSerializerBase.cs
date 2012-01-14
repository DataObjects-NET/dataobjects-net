// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Serialization
{
  /// <summary>
  /// Base class for any <see cref="IValueSerializer{T}"/>.
  /// </summary>
  /// <typeparam name="T">Type of value to serialize or deserialize.</typeparam>
  [Serializable]
  public abstract class ValueSerializerBase<T> : IValueSerializer<T>,
    IDeserializationCallback
  {
    /// <inheritdoc/>
    public IValueSerializerProvider Provider { get; protected set; }

    /// <summary>
    /// Gets the length of the output produced by this serializer.
    /// <see langword="-1" /> means it may vary.
    /// </summary>
    public int OutputLength { get; protected set; }

    /// <inheritdoc/>
    public abstract T Deserialize(Stream stream);

    /// <inheritdoc/>
    public abstract void Serialize(Stream stream, T value);

    #region IValueSerializer methods

    void IValueSerializer.Serialize(Stream stream, object value) 
    {
      Serialize(stream, (T) value);
    }

    object IValueSerializer.Deserialize(Stream stream) 
    {
      return Deserialize(stream);
    }

    #endregion

    /// <exception cref="InvalidOperationException">Requested value serializer is not found.</exception>
    protected ValueSerializer<TValue> GetValueSerializer<TValue>()
    {
      var vsp = Provider;
      var valueSerializer = vsp.GetSerializer<TValue>();
      if (valueSerializer==null) 
        throw new InvalidOperationException(string.Format(
          Strings.ExCantFindAssociate,
          ValueSerializer<TValue>.AssociateName,
          typeof(IValueSerializer<TValue>).GetShortName(),
          typeof(TValue).GetShortName()));
      return valueSerializer;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Serializer provider this serializer is bound to.</param>
    protected ValueSerializerBase(IValueSerializerProvider provider) 
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      Provider = provider;
    }

    // IDeserializationCallback methods

    /// <see cref="SerializableDocTemplate.OnDeserialization" copy="true" />
    public virtual void OnDeserialization(object sender)
    {
      if (Provider==null || Provider.GetType()==typeof (ValueSerializerProvider))
        Provider = ValueSerializerProvider.Default;
    }
  }
}