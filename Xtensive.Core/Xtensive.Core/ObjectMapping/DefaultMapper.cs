// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.10

using System;

namespace Xtensive.Core.ObjectMapping
{
  public class DefaultMapper : MapperBase
  {
    private DefaultModificationSet modificationSet;

    protected override void OnObjectModified(OperationInfo descriptor)
    {
      modificationSet.Add(descriptor);
    }

    protected override void InitializeComparison(object originalTarget, object modifiedTarget)
    {
      modificationSet = new DefaultModificationSet();
    }

    protected override IOperationSet GetComparisonResult(object originalTarget, object modifiedTarget)
    {
      modificationSet.Lock();
      return modificationSet;
    }
  }
}