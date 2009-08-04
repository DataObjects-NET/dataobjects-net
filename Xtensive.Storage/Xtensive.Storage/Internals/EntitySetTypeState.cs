// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.04

using System;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class EntitySetTypeState
  {
    public readonly RecordSet SeekRecordSet;

    public readonly RecordSet CountRecordSet;

    public readonly CombineTransform SeekTransform;

    public readonly Func<Tuple, Entity> ItemCtor;

    public EntitySetTypeState(RecordSet seekRecordSet, RecordSet countRecordSet, CombineTransform seekTransform,
      Func<Tuple, Entity> itemCtor)
    {
      SeekRecordSet = seekRecordSet;
      CountRecordSet = countRecordSet;
      SeekTransform = seekTransform;
      ItemCtor = itemCtor;
    }
  }
}