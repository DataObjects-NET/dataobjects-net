// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.20

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Internals
{
  internal static class InheritanceMerger
  {
    public static IEnumerable<Tuple> Merge(
      AdvancedComparer<Tuple> comparer,
      List<Triplet<IEnumerable<Tuple>, Converter<Tuple, Tuple>, MapTransform>> enumerators)
    {
      return Merge(comparer, 
        enumerators.ConvertAll(input => new Triplet<IEnumerator<Tuple>, Converter<Tuple, Tuple>, MapTransform>(
          input.First.GetEnumerator(), 
          input.Second, 
          input.Third))
        );
    }


    public static IEnumerable<Tuple> Merge(
      AdvancedComparer<Tuple> comparer,
      List<Triplet<IEnumerator<Tuple>, Converter<Tuple, Tuple>, MapTransform>> enumerators)
    {
      var enums = new IEnumerator<Tuple>[enumerators.Count];
      var extractors = new Converter<Tuple, Tuple>[enumerators.Count];
      var transforms = new MapTransform[enumerators.Count];
      var haveValues = new bool[enumerators.Count];

      for (int i = 0; i < enumerators.Count; i++) {
        enums[i] = enumerators[i].First;
        extractors[i] = enumerators[i].Second;
        transforms[i] = enumerators[i].Third;
        haveValues[i] = enums[i].MoveNext();
      }

      bool willContinue = false;
      for (int i = 0; i < haveValues.Length; i++)
        willContinue |= haveValues[i];

      Tuple lowestKey = null;
      int lowestItemIndex = 0;
      while (willContinue) {
        for (int i = 0; i < enums.Length; i++) {
          if (haveValues[i]) {
            Tuple key = extractors[i](enums[i].Current);
            if (lowestKey == null) {
              lowestKey = key;
              lowestItemIndex = i;
            }
            int result = comparer.Compare(key, lowestKey);
            if (result < 0) {
              lowestKey = key;
              lowestItemIndex = i;
            }
          }
        }
        Tuple item = enums[lowestItemIndex].Current;
        haveValues[lowestItemIndex] = enums[lowestItemIndex].MoveNext();

        willContinue = false;
        for (int i = 0; i < haveValues.Length; i++)
          willContinue |= haveValues[i];

        yield return transforms[lowestItemIndex].Apply(TupleTransformType.Auto, item);
      }
    }
  }
}