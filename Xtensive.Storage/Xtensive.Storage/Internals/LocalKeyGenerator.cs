// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.18

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Arithmetic;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Generator that provides nonoverlapping sequence of <see cref="IncrementalKeyGenerator{TFieldType}"/>'s sequence of numeric values.
  /// </summary>
  /// <typeparam name="TFieldType">The type of the field.</typeparam>
  public class LocalKeyGenerator<TFieldType> : KeyGenerator
  {
    private TFieldType current;
    private readonly object _lock = new object();
    private readonly Tuple tuplePrototype;

    /// <summary>
    /// Gets the <see cref="ArithmeticStruct{T}"/>.
    /// </summary>
    protected ArithmeticStruct<TFieldType> Arithmetic { get; private set; }

    /// <inheritdoc/>
    public override Tuple Next()
    {
      var result = tuplePrototype.CreateNew();
      LockType.Exclusive.Execute(_lock, () => {
        current = Arithmetic.Subtract(current, Arithmetic.One);
        result.SetValue(0, current);
      });
      return result;
    }


    // Constructors

    /// <inheritdoc/>
    public LocalKeyGenerator(KeyProviderInfo keyProviderInfo)
      : base(keyProviderInfo)
    {
      tuplePrototype = Tuple.Create(KeyProviderInfo.TupleDescriptor);
      Arithmetic = Arithmetic<TFieldType>.Default;
      current = Arithmetic.IsSigned
        ? Arithmetic.Zero
        : Arithmetic.MaxValue;
    }
  }
}