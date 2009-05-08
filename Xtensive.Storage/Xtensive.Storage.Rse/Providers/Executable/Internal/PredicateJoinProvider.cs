// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.05

using System;
using System.Collections.Generic;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class PredicateJoinProvider : BinaryExecutableProvider<Compilable.PredicateJoinProvider>
  {
    private readonly bool leftJoin;
    private CombineTransform transform;
    private Func<Tuple, Tuple, bool> predicate;
    private Tuple rightBlank;

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var left = Left.Enumerate(context);
      var right = Right.Enumerate(context);
      foreach (var l in left)
        foreach (var r in right)
          if (predicate(l, r))
            yield return transform.Apply(TupleTransformType.Auto, l, r);
          else if (leftJoin)
            yield return transform.Apply(TupleTransformType.Auto, l, rightBlank);
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      transform = new CombineTransform(true, Left.Header.TupleDescriptor, Right.Header.TupleDescriptor);
      predicate = Origin.Predicate.CompileCached();
      rightBlank = Tuple.Create(Right.Header.TupleDescriptor);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public PredicateJoinProvider(Compilable.PredicateJoinProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {
      leftJoin = Origin.Outer;
    }
  }
}