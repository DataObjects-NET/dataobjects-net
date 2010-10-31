// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.11

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Internals.DocTemplates;
using S=Xtensive.SizeCalculators;

namespace Xtensive.SizeCalculators
{
  /// <summary>
  /// A struct providing faster access for key <see cref="S.SizeCalculator{T}"/> delegates.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="ISizeCalculator{T}"/> generic argument.</typeparam>
  [Serializable]
  public struct SizeCalculatorStruct<T> : ISerializable
  {
    /// <summary>
    /// Gets <see cref="SizeCalculatorStruct{T}"/> for <see cref="S.SizeCalculator{T}.Default"/> hasher.
    /// </summary>
    public static readonly SizeCalculatorStruct<T> Default = new SizeCalculatorStruct<T>(SizeCalculator<T>.Default);


    /// <summary>
    /// Gets the underlying size calculator for this cache.
    /// </summary>
    public readonly SizeCalculator<T> SizeCalculator;

    /// <summary>
    /// Gets <see cref="ISizeCalculatorBase.GetDefaultSize"/> method delegate.
    /// </summary>
    public readonly Func<int> GetSize;

    /// <summary>
    /// Gets <see cref="ISizeCalculator{T}.GetValueSize"/> method delegate.
    /// </summary>
    public readonly Func<T, int> GetValueSize;

    /// <summary>
    /// Implicit conversion of <see cref="S.SizeCalculator{T}"/> to <see cref="SizeCalculatorStruct{T}"/>.
    /// </summary>
    /// <param name="sizeCalculator">Size calculator to provide the struct for.</param>
    public static implicit operator SizeCalculatorStruct<T>(SizeCalculator<T> sizeCalculator)
    {
      return new SizeCalculatorStruct<T>(sizeCalculator);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="sizeCalculator">Size calculator to provide the delegates for.</param>
    private SizeCalculatorStruct(SizeCalculator<T> sizeCalculator)
    {
      SizeCalculator = sizeCalculator;
      GetSize = SizeCalculator==null ? null : SizeCalculator.GetDefaultSize;
      GetValueSize = SizeCalculator==null ? null : SizeCalculator.GetValueSize;
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    private SizeCalculatorStruct(SerializationInfo info, StreamingContext context)
    {
      SizeCalculator = (SizeCalculator<T>) info.GetValue("SizeCalculator", typeof (SizeCalculator<T>));
      GetSize = SizeCalculator==null ? null : SizeCalculator.GetDefaultSize;
      GetValueSize = SizeCalculator==null ? null : SizeCalculator.GetValueSize;
    }

    /// <inheritdoc/>
    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("SizeCalculator", SizeCalculator);
    }
  }
}