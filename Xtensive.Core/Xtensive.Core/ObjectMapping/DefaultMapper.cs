// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.10

namespace Xtensive.Core.ObjectMapping
{
  public class DefaultMapper : MapperBase
  {
    private DefaultOperationSet operationSet;

    protected override void OnObjectModified(OperationInfo descriptor)
    {
      operationSet.Add(descriptor);
    }

    protected override void InitializeComparison(object originalTarget, object modifiedTarget)
    {
      operationSet = new DefaultOperationSet();
    }

    protected override IOperationSet GetComparisonResult(object originalTarget, object modifiedTarget)
    {
      operationSet.Lock();
      return operationSet;
    }
  }
}