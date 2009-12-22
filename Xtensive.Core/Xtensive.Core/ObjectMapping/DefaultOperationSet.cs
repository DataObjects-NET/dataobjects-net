// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.16

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.ObjectMapping
{
  [Serializable]
  public sealed class DefaultOperationSet : LockableBase,
    IOperationSet,
    IEnumerable<OperationInfo>
  {
    private readonly List<OperationInfo> descriptors = new List<OperationInfo>();

    public bool IsEmpty { get { return descriptors.Count==0; } }

    public void Apply()
    {
      throw new NotImplementedException();
    }

    public void Add(OperationInfo descriptor)
    {
      this.EnsureNotLocked();
      descriptors.Add(descriptor);
    }

    public IEnumerator<OperationInfo> GetEnumerator()
    {
      return descriptors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}