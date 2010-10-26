// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.05.13

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class HashJoinProvider : BinaryExecutableProvider<Compilable.JoinProvider>
  {
    private readonly JoinType joinType;
    private readonly Pair<int>[] joiningPairs;
    private CombineTransform transform;
    private MapTransform leftKeyTransform;
    private MapTransform rightKeyTransform;
    private Tuple rightBlank;

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var hashTable = new Dictionary<Tuple,List<Tuple>>();
      var left = Left.Enumerate(context);
      var right = Right.Enumerate(context);
      foreach (var item in right) {
        var key = KeyExtractorRight(item);
        List<Tuple> entry;
        if (hashTable.TryGetValue(key, out entry))
          entry.Add(item);
        else {
          entry = new List<Tuple> {item};
          hashTable.Add(key, entry);
        }
      }
      foreach (var leftItem in left) {
        var key = KeyExtractorLeft(leftItem);
        List<Tuple> entry;// = hashTable[key];
        var exists = hashTable.TryGetValue(key, out entry);
        if (joinType == JoinType.LeftOuter) {
          if (exists)
            foreach (var rightItem in entry)
              yield return transform.Apply(TupleTransformType.Auto, leftItem, rightItem);
          else
            yield return transform.Apply(TupleTransformType.Auto, leftItem, rightBlank);
        }
        else if (exists)
          foreach (var rightItem in entry)
            yield return transform.Apply(TupleTransformType.Auto, leftItem, rightItem);
      }
    }

    #region Helper methods

    private Tuple KeyExtractorLeft(Tuple input)
    {
      return leftKeyTransform.Apply(TupleTransformType.Auto, input);
    }

    private Tuple KeyExtractorRight(Tuple input)
    {
      return rightKeyTransform.Apply(TupleTransformType.Auto, input);
    }

    #endregion

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      transform = new CombineTransform(true, Left.Header.TupleDescriptor, Right.Header.TupleDescriptor);
      int[] leftColumns = joiningPairs.Select(pair => pair.First).ToArray();
      int[] rightColumns = joiningPairs.Select(pair => pair.Second).ToArray();
      TupleDescriptor leftKeyDescriptor = TupleDescriptor.Create(leftColumns
        .Select(i => Left.Header.TupleDescriptor[i]));
      TupleDescriptor rightKeyDescriptor = TupleDescriptor.Create(rightColumns
        .Select(i => Right.Header.TupleDescriptor[i]));
      leftKeyTransform = new MapTransform(true, leftKeyDescriptor, leftColumns);
      rightKeyTransform = new MapTransform(true, rightKeyDescriptor, rightColumns);
      rightBlank = Tuple.Create(Right.Header.TupleDescriptor);
      rightBlank.Initialize(new BitArray(Right.Header.Length, true));
    }


    // Constructors

    public HashJoinProvider(Compilable.JoinProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {
      joinType = origin.JoinType;
      joiningPairs = origin.EqualIndexes;
    }
  }
}