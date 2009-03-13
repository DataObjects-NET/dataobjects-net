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

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class ApplyProvider : BinaryExecutableProvider<Compilable.ApplyProvider>
  {
    private CombineTransform transform;

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      using (new ParameterScope()) {
        var left = Left.Enumerate(context);
        foreach (var tuple in left) {
          Origin.LeftItemParameter.Value = tuple;
          // Do not cache right part
          var right = Right.OnEnumerate(context);
          bool empty = true;
          foreach (var rTuple in right) {
            empty = false;
            var item = transform.Apply(TupleTransformType.Auto, tuple, rTuple);
            yield return item;
          }

          if (empty && Origin.LeftJoin) {
            var item = transform.Apply(TupleTransformType.Auto, tuple, default(Tuple));
            yield return item;
          }
        }
      }
    }

    protected override void Initialize()
    {
      base.Initialize();
      transform = new CombineTransform(true, Left.Header.TupleDescriptor, Right.Header.TupleDescriptor);
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