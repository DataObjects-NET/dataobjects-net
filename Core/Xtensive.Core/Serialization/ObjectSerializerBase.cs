// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.20

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;
using Xtensive.Serialization.Implementation;

namespace Xtensive.Serialization
{
  /// <summary>
  /// Base class for any <see cref="IObjectSerializer{T}"/>.
  /// </summary>
  /// <typeparam name="T">Type of object to serialize / deserialize.</typeparam>
  public abstract class ObjectSerializerBase<T> : IObjectSerializer<T>
  {
    /// <inheritdoc/>
    public IObjectSerializerProvider Provider { get; protected set; }

    #region Cached value serializers

    /// <summary>
    /// Gets the value serializer for <see cref="bool"/> type.
    /// </summary>
    protected ValueSerializer<bool> BooleanSerializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="byte"/> type.
    /// </summary>
    protected ValueSerializer<byte> ByteSerializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="char"/> type.
    /// </summary>
    protected ValueSerializer<char> CharSerializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="decimal"/> type.
    /// </summary>
    protected ValueSerializer<decimal> DecimalSerializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="double"/> type.
    /// </summary>
    protected ValueSerializer<double> DoubleSerializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="Guid"/> type.
    /// </summary>
    protected ValueSerializer<Guid> GuidSerializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="short"/> type.
    /// </summary>
    protected ValueSerializer<short> Int16Serializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="ushort"/> type.
    /// </summary>
    protected ValueSerializer<ushort> UInt16Serializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="int"/> type.
    /// </summary>
    protected ValueSerializer<int> Int32Serializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="uint"/> type.
    /// </summary>
    protected ValueSerializer<uint> UInt32Serializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="long"/> type.
    /// </summary>
    protected ValueSerializer<long> Int64Serializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="ulong"/> type.
    /// </summary>
    protected ValueSerializer<ulong> UInt64Serializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="float"/> type.
    /// </summary>
    protected ValueSerializer<float> SingleSerializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="string"/> type.
    /// </summary>
    protected ValueSerializer<string> StringSerializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="Type"/> type.
    /// </summary>
    protected ValueSerializer<Type> TypeSerializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="Token{T}"/> of <see cref="string"/> type.
    /// </summary>
    protected ValueSerializer<Token<string>> StringTokenSerializer { get; private set; }

    /// <summary>
    /// Gets the value serializer for <see cref="Token{T}"/> of <see cref="Type"/> type.
    /// </summary>
    protected ValueSerializer<Token<Type>> TypeTokenSerializer { get; private set; }

    #endregion

    /// <inheritdoc/>
    public virtual bool IsReferable {
      [DebuggerStepThrough]
      get { return true; }
    }

    /// <inheritdoc/>
    public abstract T CreateObject(Type type);

    /// <inheritdoc/>
    public virtual void GetObjectData(T source, T origin, SerializationData data)
    {
      GetObjectHeader(source, data);
    }

    /// <summary>
    /// Adds <paramref name="source"/> type and reference to the <paramref name="data"/>.
    /// </summary>
    /// <param name="source">The object to add the header for.</param>
    /// <param name="data">The <see cref="SerializationData"/> to update.</param>
    protected virtual void GetObjectHeader(T source, SerializationData data)
    {
      var type = GetObjectType(source);
      data.SerializedType = type;
      if (IsReferable)
        // It isn't a reference type, so it must have SerializedReference
        data.SerializedReference = data.Reference;
    }

    /// <summary>
    /// Gets the type of the object (used in <see cref="GetObjectData"/>).
    /// </summary>
    /// <param name="source">The object to get the type of.</param>
    /// <returns>The type of the object.</returns>
    public virtual Type GetObjectType(T source)
    {
      return source.GetType();
    }

    /// <inheritdoc/>
    public abstract T SetObjectData(T source, SerializationData data);

    #region IObjectSerializer Members

    /// <inheritdoc/>
    object IObjectSerializer.CreateObject(Type type) 
    {
      return CreateObject(type);
    }

    /// <inheritdoc/>
    void IObjectSerializer.GetObjectData(SerializationData data) 
    {
      GetObjectData((T) data.Source, (T) data.Origin, data);
    }

    /// <inheritdoc/>
    void IObjectSerializer.SetObjectData(SerializationData data) 
    {
      data.UpdateSource(SetObjectData((T) data.Origin, data));
    }

    #endregion

    /// <exception cref="InvalidOperationException">Requested value serializer is not found.</exception>
    protected ValueSerializer<TValue> GetValueSerializer<TValue>()
    {
      var vsp = Provider.ValueSerializerProvider;
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The provider.</param>
    protected ObjectSerializerBase(IObjectSerializerProvider provider) 
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      Provider = provider;
      BooleanSerializer = GetValueSerializer<bool>();
      ByteSerializer = GetValueSerializer<byte>();
      CharSerializer = GetValueSerializer<char>();
      DecimalSerializer = GetValueSerializer<decimal>();
      DoubleSerializer = GetValueSerializer<double>();
      GuidSerializer = GetValueSerializer<Guid>();
      Int16Serializer = GetValueSerializer<short>();
      UInt16Serializer = GetValueSerializer<ushort>();
      Int32Serializer = GetValueSerializer<int>();
      UInt32Serializer = GetValueSerializer<uint>();
      Int64Serializer = GetValueSerializer<long>();
      UInt64Serializer = GetValueSerializer<ulong>();
      SingleSerializer = GetValueSerializer<float>();
      TypeSerializer = GetValueSerializer<Type>();
      StringSerializer = GetValueSerializer<string>();
      StringTokenSerializer = GetValueSerializer<Token<string>>();
      TypeTokenSerializer = GetValueSerializer<Token<Type>>();
    }
  }
}