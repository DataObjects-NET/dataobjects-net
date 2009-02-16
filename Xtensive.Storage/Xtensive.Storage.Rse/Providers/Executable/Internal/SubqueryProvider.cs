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
  internal sealed class SubqueryProvider : BinaryExecutableProvider<Compilable.SubqueryProvider>
  {
    private CombineTransform transform;

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var result = new List<Tuple>();
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
            result.Add(item);
//            yield return item;
          }

          if (empty && Origin.LeftJoin) {
            var item = transform.Apply(TupleTransformType.Auto, tuple, default(Tuple));
            result.Add(item);
//            yield return item;
          }
        }
      }
      return result;
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
    public SubqueryProvider(Compilable.SubqueryProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {}
  }
}