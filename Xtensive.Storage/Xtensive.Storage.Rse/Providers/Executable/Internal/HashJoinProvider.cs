// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.05.13

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class HashJoinProvider : BinaryExecutableProvider<Compilable.JoinProvider>
  {
    private readonly bool outerJoin;
    private readonly Pair<int>[] joiningPairs;
    private CombineTransform transform;
    private MapTransform leftKeyTransform;
    private MapTransform rightKeyTransform;
    private Hashtable hashTable;
    private Tuple rightBlank;

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      hashTable = new Hashtable();
      var left = Left.Enumerate(context);
      var right = Right.Enumerate(context);
      foreach (var item in right)
        hashTable.Add(KeyExtractorRight(item), item);
      foreach (var item in left) {
        var hashValue = hashTable[KeyExtractorLeft(item)];
        if (outerJoin)
          yield return hashValue!=null ? transform.Apply(TupleTransformType.Auto, item, (Tuple) hashValue)
            : transform.Apply(TupleTransformType.Auto, item, rightBlank);
        else if (hashValue!=null)
          yield return transform.Apply(TupleTransformType.Auto, item, (Tuple) hashValue);
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
      TupleDescriptor leftKeyDescriptor = TupleDescriptor.Create(leftColumns.Select(i => Left.Header.TupleDescriptor[i]));
      TupleDescriptor rightKeyDescriptor = TupleDescriptor.Create(rightColumns.Select(i => Right.Header.TupleDescriptor[i]));
      leftKeyTransform = new MapTransform(true, leftKeyDescriptor, leftColumns);
      rightKeyTransform = new MapTransform(true, rightKeyDescriptor, rightColumns);
      rightBlank = Tuple.Create(Right.Header.TupleDescriptor);
    }


    // Constructors

    public HashJoinProvider(Compilable.JoinProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {
      outerJoin = origin.Outer;
      joiningPairs = origin.EqualIndexes;
    }
  }
}