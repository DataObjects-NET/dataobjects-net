// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.15

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

using C=Xtensive.Conversion;

namespace Xtensive.Conversion
{
  /// <summary>
  /// A struct providing faster access for key <see cref="C.AdvancedConverter{TFrom,TTo}"/> delegates.
  /// </summary>
  [Serializable]
  public struct AdvancedConverterStruct<TFrom, TTo> : ISerializable
  {
    /// <summary>
    /// Gets <see cref="AdvancedConverterStruct{TFrom, TTo}"/> for 
    /// <see cref="C.AdvancedConverter{TFrom,TTo}.Default"/> hasher.
    /// </summary>
    public static readonly AdvancedConverterStruct<TFrom, TTo> Default = new AdvancedConverterStruct<TFrom, TTo>(AdvancedConverter<TFrom, TTo>.Default);


    /// <summary>
    /// Gets the underlying converter for this cache.
    /// </summary>
    public readonly AdvancedConverter<TFrom, TTo> AdvancedConverter;

    /// <summary>
    /// Gets <see cref="IAdvancedConverter{TFrom, TTo}.Convert"/> method delegate.
    /// </summary>
    public readonly Converter<TFrom, TTo> Convert;

    /// <summary>
    /// Gets <see cref="IAdvancedConverter{TFrom,TTo}.IsRough"/> value.
    /// </summary>
    public readonly bool IsRough;

    /// <summary>
    /// Implicit conversion of <see cref="C.AdvancedConverter{TFrom,TTo}"/> to 
    /// <see cref="AdvancedConverterStruct{TFrom, TTo}"/>.
    /// </summary>
    /// <param name="advancedConverter">Converter to provide the struct for.</param>
    public static implicit operator AdvancedConverterStruct<TFrom, TTo>(AdvancedConverter<TFrom, TTo> advancedConverter)
    {
      return new AdvancedConverterStruct<TFrom, TTo>(advancedConverter);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="advancedConverter">Converter to provide the delegates for.</param>
    private AdvancedConverterStruct(AdvancedConverter<TFrom, TTo> advancedConverter)
    {
      AdvancedConverter = advancedConverter;
      Convert = AdvancedConverter==null ? null : AdvancedConverter.Convert;
      IsRough = AdvancedConverter==null ? true : AdvancedConverter.IsRough;
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    private AdvancedConverterStruct(SerializationInfo info, StreamingContext context)
    {
      AdvancedConverter = (AdvancedConverter<TFrom, TTo>) info.GetValue("AdvancedConverter", typeof (AdvancedConverter<TFrom, TTo>));
      Convert = AdvancedConverter==null ? null : AdvancedConverter.Convert;
      IsRough = AdvancedConverter==null ? true : AdvancedConverter.IsRough;
    }

    /// <inheritdoc/>
    [SecurityCritical]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("AdvancedConverter", AdvancedConverter);
    }
  }
}