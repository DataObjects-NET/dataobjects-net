// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.16

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class ApplyProvider : BinaryExecutableProvider<Compilable.ApplyProvider>
  {
    private CombineTransform combineTransform;
    private Tuple rightBlank;

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var left = Left.Enumerate(context);
      switch (Origin.ApplyType) {
        case ApplyType.Cross:
          return CrossApply(context, left);
        case ApplyType.Outer:
          return OuterApply(context, left); ;
        case ApplyType.Existing:
          return ApplyExisting(context, left);
        case ApplyType.NotExisting:
          return ApplyNotExisting(context, left);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    #region Private implementation.

    private IEnumerable<Tuple> CrossApply(EnumerationContext context, IEnumerable<Tuple> left)
    {
      using (new ParameterScope())
      foreach (var tuple in left) {
        Origin.LeftItemParameter.Value = tuple;
        // Do not cache right part
        var right = Right.OnEnumerate(context);
        foreach (var rTuple in right) {
          var item = combineTransform.Apply(TupleTransformType.Auto, tuple, rTuple);
          yield return item;
        }
      }
    }

    private IEnumerable<Tuple> OuterApply(EnumerationContext context, IEnumerable<Tuple> left)
    {
      using (new ParameterScope())
      foreach (var tuple in left) {
        Origin.LeftItemParameter.Value = tuple;
        // Do not cache right part
        var right = Right.OnEnumerate(context);
        bool empty = true;
        foreach (var rTuple in right) {
          empty = false;
          var item = combineTransform.Apply(TupleTransformType.Auto, tuple, rTuple);
          yield return item;
        }
        if (empty) {
          var item = combineTransform.Apply(TupleTransformType.Auto, tuple, rightBlank);
          yield return item;
        }
      }
    }

    private IEnumerable<Tuple> ApplyExisting(EnumerationContext context, IEnumerable<Tuple> left)
    {
      using (new ParameterScope())
      foreach (var tuple in left) {
        Origin.LeftItemParameter.Value = tuple;
        // Do not cache right part
        var right = Right.OnEnumerate(context);
        if (right.Any())
          yield return tuple;
      }
    }

    private IEnumerable<Tuple> ApplyNotExisting(EnumerationContext context, IEnumerable<Tuple> left)
    {
      using (new ParameterScope())
      foreach (var tuple in left) {
        Origin.LeftItemParameter.Value = tuple;
        // Do not cache right part
        var right = Right.OnEnumerate(context);
        if (!right.Any())
          yield return tuple;
      }
    }
//
//    private IEnumerable<Tuple> ApplyExistenceColumn(EnumerationContext context, IEnumerable<Tuple> left)
//    {
//      using (new ParameterScope())
//      foreach (var tuple in left) {
//        Origin.LeftItemParameter.Value = tuple;
//        // Do not cache right part
//        var right = Right.OnEnumerate(context);
//        yield return existenceColumnTransform.Apply(TupleTransformType.Auto, tuple, right.Any());
//      }
//    }

    #endregion


    protected override void Initialize()
    {
      base.Initialize();
      combineTransform = new CombineTransform(true, Left.Header.TupleDescriptor, Right.Header.TupleDescriptor);
      rightBlank = Tuple.Create(Right.Header.TupleDescriptor);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ApplyProvider(Compilable.ApplyProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {}
  }
}