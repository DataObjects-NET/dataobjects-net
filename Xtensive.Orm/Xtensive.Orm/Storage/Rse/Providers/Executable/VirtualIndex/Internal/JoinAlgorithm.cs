// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.03

using System;
using System.Collections.Generic;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal
{
  internal static class JoinAlgorithm
  {
    public static IEnumerable<Tuple> Join(IEnumerable<Tuple> left,
                                          Converter<Tuple, Tuple> keyExtractorLeft,
                                          AdvancedComparer<Tuple> comparer,
                                          MapTransform transform,
                                          List<Triplet<IEnumerable<Tuple>, Converter<Tuple, Tuple>, TupleDescriptor>> rightEnums)
    {
      return Join(
        left.GetEnumerator(),
        keyExtractorLeft, 
        comparer, 
        transform, 
        rightEnums.ConvertAll(input => new Triplet<IEnumerator<Tuple>, Converter<Tuple, Tuple>, TupleDescriptor>(
                                         input.First.GetEnumerator(), 
                                         input.Second, 
                                         input.Third
                                         ))
        );
    }

    public static IEnumerable<Tuple> Join(IEnumerator<Tuple> left,
                                          Converter<Tuple, Tuple> keyExtractorLeft,
                                          AdvancedComparer<Tuple> comparer,
                                          MapTransform transform,
                                          List<Triplet<IEnumerator<Tuple>, Converter<Tuple, Tuple>, TupleDescriptor>> rightEnums)
    {
      var rightEnumerators = new IEnumerator<Tuple>[rightEnums.Count];
      var rightKeyExtractors = new Converter<Tuple, Tuple>[rightEnums.Count];
      var rightHaveValue = new bool[rightEnums.Count];
      var rightTuples = new Tuple[rightEnums.Count];

      for (int i = 0; i < rightEnums.Count; i++) {
        rightEnumerators[i] = rightEnums[i].First;
        rightKeyExtractors[i] = rightEnums[i].Second;
        rightHaveValue[i] = rightEnumerators[i].MoveNext();
      }

      while (left.MoveNext()) {
        Tuple leftTuple = left.Current;
        Tuple leftKey = keyExtractorLeft(leftTuple);
        for (int i = 0; i < rightEnumerators.Length; i++) {
          comparisonStep:
          if (rightHaveValue[i]) {
            IEnumerator<Tuple> rEnum = rightEnumerators[i];
            Tuple rightTuple = rEnum.Current;
            Tuple rightKey = rightKeyExtractors[i](rightTuple);
            int result = comparer.Compare(leftKey, rightKey);
            if (result==0) {
              rightTuples[i] = rightTuple;
              rightHaveValue[i] = rEnum.MoveNext();
            }
            else if (result > 0) {
              rightHaveValue[i] = rEnum.MoveNext();
              goto comparisonStep;
            }
          }
        }
        for (int i = 0; i < rightTuples.Length; i++)
          if (rightTuples[i]==null)
            rightTuples[i] = Tuple.Create(rightEnums[i].Third);

        var resultArray = new Tuple[rightEnums.Count + 1];
        resultArray[0] = leftTuple;
        rightTuples.CopyTo(resultArray, 1);

        yield return transform.Apply(TupleTransformType.Auto, resultArray);

        for (int i = 0; i < rightTuples.Length; i++)
          rightTuples[i] = null;
      }
    }
  }
}