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
  public class IncrementalGenerator<TFieldType> : Generator
  {
    protected TFieldType Current { get; set; }
    protected ArithmeticStruct<TFieldType> Arithmetic { get; private set; }
    protected readonly object _lock = new object();

    /// <inheritdoc/>
    public override Tuple Next()
    {
      Tuple result = Tuple.Create(Hierarchy.KeyTupleDescriptor);
      LockType.Exclusive.Execute(_lock, () => {
        Current = Arithmetic.Add(Current, Arithmetic.One);
        result.SetValue(0, Current);
      });
      return result;
    }


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