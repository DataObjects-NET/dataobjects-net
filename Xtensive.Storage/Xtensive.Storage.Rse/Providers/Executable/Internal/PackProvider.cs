// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.08.26

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  public class PackProvider : UnaryExecutableProvider<Compilable.PackProvider>
  {
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      long rowNumber = 1;
      using (var enumerator = Source.Enumerate(context).GetEnumerator()) {
        Tuple keyTuple = null;
        Tuple groupTuple = null;
        List<Tuple> pack = null;
        while (enumerator.MoveNext()) {
          var tuple = enumerator.Current;
          var currentKeyTuple = Origin.KeyTransform.Apply(TupleTransformType.Tuple, tuple);
          if (currentKeyTuple.Equals(keyTuple)) {
            pack.Add(Origin.PackTransform.Apply(TupleTransformType.Tuple, tuple));
          }
          else {
            if (groupTuple!=null) {
              groupTuple.SetValue(Origin.PackedColumn.Index, pack.AsReadOnly());
              yield return groupTuple;
            }
            keyTuple = currentKeyTuple;
            pack = new List<Tuple>();
            groupTuple = Origin.GroupTransform.Apply(TupleTransformType.Tuple, tuple);
          }
        }
        if (groupTuple!=null) {
          groupTuple.SetValue(Origin.PackedColumn.Index, pack.AsReadOnly());
          yield return groupTuple;
        }
      }
    }


    // Constructors

    
    public PackProvider(Compilable.PackProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}