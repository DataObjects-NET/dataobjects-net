// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Atomicity.OperationLogs
{
  public class MemoryOperationLog: OperationLogBase
  {
    private List<IRedoDescriptor> descriptors;

    public override void Append(IRedoDescriptor redoDescriptor)
    {
      ValidateDescriptor(redoDescriptor);
      descriptors.Add(redoDescriptor);
    }

    public override IEnumerator<IRedoDescriptor> GetEnumerator(Direction direction)
    {
      if (direction==Direction.Positive) {
        for (int i = 0; i < descriptors.Count; i++)
          yield return descriptors[i];
      }
      else {
        for (int i = descriptors.Count-1; i>=0; i--)
          yield return descriptors[i];
      }
    }


    // Constructors

    public MemoryOperationLog()
    {
      descriptors = new List<IRedoDescriptor>();
    }

    public MemoryOperationLog(IEnumerable<IRedoDescriptor> source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      descriptors = new List<IRedoDescriptor>(source);
    }
  }
}