// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.10

using Xtensive.Core;
using Xtensive.Core.Arithmetic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Generator that provides incremental sequence of integer values.
  /// </summary>
  /// <typeparam name="TFieldType">The type of the field.</typeparam>
  public class IncrementalGenerator<TFieldType> : Generator
  {
    private TFieldType current;
    private readonly object _lock = new object();

    /// <summary>
    /// Gets the <see cref="ArithmeticStruct{T}"/>.
    /// </summary>
    protected ArithmeticStruct<TFieldType> Arithmetic { get; private set; }

    /// <inheritdoc/>
    public override Tuple Next()
    {
      Tuple result = Tuple.Create(Hierarchy.KeyTupleDescriptor);
      LockType.Exclusive.Execute(_lock, () => {
        current = Arithmetic.Add(current, Arithmetic.One);
        result.SetValue(0, current);
      });
      return result;
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hierarchy">The hierarchy this instance will serve.</param>
    public IncrementalGenerator(HierarchyInfo hierarchy)
      : base(hierarchy)
    {
      Arithmetic = Arithmetic<TFieldType>.Default;
    }
  }
}