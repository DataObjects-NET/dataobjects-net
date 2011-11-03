// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.05

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class PredicateJoinProvider : BinaryExecutableProvider<Compilable.PredicateJoinProvider>
  {
    private readonly JoinType joinType;
    private CombineTransform transform;
    private Func<Tuple, Tuple, bool> predicate;
    private Tuple rightBlank;

    public override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var left = Left.Enumerate(context);
      var right = Right.Enumerate(context);
      foreach (var l in left) {
        bool rightTupleWasFound = false;
        foreach (var r in right)
          if (predicate(l, r)) {
            rightTupleWasFound = true;
            var item = transform.Apply(TupleTransformType.Auto, l, r);
            yield return item;
          }
        if (joinType == JoinType.LeftOuter && !rightTupleWasFound) {
          var item = transform.Apply(TupleTransformType.Auto, l, rightBlank);
          yield return item;
        }
      }
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      transform = new CombineTransform(true, Left.Header.TupleDescriptor, Right.Header.TupleDescriptor);
      predicate = Origin.Predicate.CachingCompile();
      rightBlank = Tuple.Create(Right.Header.TupleDescriptor);
      rightBlank.Initialize(new BitArray(Right.Header.Length, true));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public PredicateJoinProvider(Compilable.PredicateJoinProvider origin, ExecutableProvider left,
      ExecutableProvider right)
      : base(origin, left, right)
    {
      joinType = Origin.JoinType;
    }
  }
}