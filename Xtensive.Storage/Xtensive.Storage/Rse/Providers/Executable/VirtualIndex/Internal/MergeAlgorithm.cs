// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.20

using System;
using System.Collections.Generic;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal
{
  internal static class MergeAlgorithm
  {
    public static IEnumerable<Tuple> Merge(
      AdvancedComparer<Tuple> comparer,
      List<Pair<IEnumerable<Tuple>, Converter<Tuple, Tuple>>> enumerators)
    {
      return Merge(comparer, 
                   enumerators.ConvertAll(input => new Pair<IEnumerator<Tuple>, Converter<Tuple, Tuple>>(
                     input.First.GetEnumerator(), 
                     input.Second))
        );
    }


    public static IEnumerable<Tuple> Merge(
      AdvancedComparer<Tuple> comparer,
      List<Pair<IEnumerator<Tuple>, Converter<Tuple, Tuple>>> enumerators)
    {
      var enums = new IEnumerator<Tuple>[enumerators.Count];
      var extractors = new Converter<Tuple, Tuple>[enumerators.Count];
      var haveValues = new bool[enumerators.Count];

      for (int i = 0; i < enumerators.Count; i++) {
        enums[i] = enumerators[i].First;
        extractors[i] = enumerators[i].Second;
        haveValues[i] = enums[i].MoveNext();
      }

      bool willContinue = false;
      for (int i = 0; i < haveValues.Length; i++)
        willContinue |= haveValues[i];

      while (willContinue) {
        Tuple lowestKey = null;
        int lowestItemIndex = 0;
        for (int i = 0; i < enums.Length; i++) {
          if (haveValues[i]) {
            Tuple key = extractors[i](enums[i].Current);
            if (lowestKey == null) {
              lowestKey = key;
              lowestItemIndex = i;
            }
            int compare = comparer.Compare(key, lowestKey);
            if (compare < 0) {
              lowestKey = key;
              lowestItemIndex = i;
            }
          }
        }
        var item = enums[lowestItemIndex].Current;
        haveValues[lowestItemIndex] = enums[lowestItemIndex].MoveNext();
        if (!haveValues[lowestItemIndex]) {
          lowestKey = null;
          willContinue = false;
          for (int i = 0; i < haveValues.Length; i++)
            willContinue |= haveValues[i];
        }
        yield return item;
      }
    }
  }
}