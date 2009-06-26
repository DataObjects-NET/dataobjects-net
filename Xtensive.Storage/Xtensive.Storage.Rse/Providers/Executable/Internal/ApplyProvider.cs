// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.16

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;

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
      case JoinType.Inner:
        return InnerApply(context, left);
      case JoinType.LeftOuter:
        return LeftOuterApply(context, left);
      default:
        throw new ArgumentOutOfRangeException();
      }
    }

    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      Left.OnBeforeEnumerate(context);
    }

    protected internal override void OnAfterEnumerate(EnumerationContext context)
    {
      Left.OnAfterEnumerate(context);
    }

    #region Private implementation.

    private IEnumerable<Tuple> InnerApply(EnumerationContext context, IEnumerable<Tuple> left)
    {
      using (new ParameterContext().Activate())
        foreach (var tuple in left) {
          Origin.ApplyParameter.Value = tuple;
          var result = new List<Tuple>();
          var rightContext = context.CreateNew();
          using (rightContext.Activate()) {
            Right.OnBeforeEnumerate(rightContext);
            // Do not cache right part
            var right = Right.OnEnumerate(rightContext);
            foreach (var rTuple in right)
              result.Add(combineTransform.Apply(TupleTransformType.Auto, tuple, rTuple));
            Right.OnAfterEnumerate(rightContext);
          }
          foreach (var item in result)
            yield return item;
        }

//      var ctx = new ParameterContext();
//      ParameterScope scope = null;
//      var batched = left
//        .SelectMany(tuple => {
//          Origin.ApplyParameter.Value = tuple;
//          // Do not cache right part
//          var right = Right.OnEnumerate(context);
//          return right.Select(rTuple => combineTransform.Apply(TupleTransformType.Auto, tuple, rTuple));
//        })
//        .Batch(0, 1, 1)
//        .ApplyBeforeAndAfter(() => scope = ctx.Activate(), () => scope.DisposeSafely());
//      foreach (var batch in batched)
//        foreach (var tuple in batch)
//          yield return tuple;
    }

    private IEnumerable<Tuple> LeftOuterApply(EnumerationContext context, IEnumerable<Tuple> left)
    {
      using (new ParameterContext().Activate())
        foreach (var tuple in left) {
          Origin.ApplyParameter.Value = tuple;

          var result = new List<Tuple>();
          var rightContext = context.CreateNew();
          using (rightContext.Activate()) {
            Right.OnBeforeEnumerate(rightContext);
            // Do not cache right part
            var right = Right.OnEnumerate(rightContext);
            bool isEmpty = true;
            foreach (var rTuple in right) {
              if (!isEmpty) 
                if (Origin.SequenceType == ApplySequenceType.Single || Origin.SequenceType == ApplySequenceType.SingleOrDefault)
                  throw new InvalidOperationException("Sequence contains more than one element.");
              isEmpty = false;
              result.Add(combineTransform.Apply(TupleTransformType.Auto, tuple, rTuple));
            }
            if (isEmpty)
              result.Add(combineTransform.Apply(TupleTransformType.Auto, tuple, rightBlank));
            Right.OnAfterEnumerate(rightContext);
          }
          foreach (var item in result)
            yield return item;
        }

//      var ctx = new ParameterContext();
//      ParameterScope scope = null;
//      var batched = left
//        .SelectMany(tuple => {
//          Origin.ApplyParameter.Value = tuple;
//          // Do not cache right part
//          var right = Right.OnEnumerate(context);
//          bool isEmpty = true;
//          var result = right.Select(rTuple => {
//            if (!isEmpty) 
//              if (Origin.SequenceType == ApplySequenceType.Single || Origin.SequenceType == ApplySequenceType.SingleOrDefault)
//                throw new InvalidOperationException("Sequence contains more than one element.");
//            
//            isEmpty = false;
//            return combineTransform.Apply(TupleTransformType.Auto, tuple, rTuple);
//          });
//          return isEmpty 
//            ? EnumerableUtils.One(combineTransform.Apply(TupleTransformType.Auto, tuple, rightBlank)) 
//            : result;
//        })
//        .Batch(0, 1, 1)
//        .ApplyBeforeAndAfter(() => scope = ctx.Activate(),() => scope.DisposeSafely());
//      foreach (var batch in batched)
//        foreach (var tuple in batch)
//          yield return tuple;
    }

    #endregion

    protected override void Initialize()
    {
      base.Initialize();
      combineTransform = new CombineTransform(true, Left.Header.TupleDescriptor, Right.Header.TupleDescriptor);
      rightBlank = Tuple.Create(Right.Header.TupleDescriptor);
      rightBlank.Initialize(new BitArray(Right.Header.Length, true));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ApplyProvider(Compilable.ApplyProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {
    }
  }
}