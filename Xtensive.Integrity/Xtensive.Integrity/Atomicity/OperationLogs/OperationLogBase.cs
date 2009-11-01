// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Integrity.Atomicity.Internals;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Atomicity.OperationLogs
{
  public abstract class OperationLogBase: IOperationLog, 
    IEnumerable<IRedoDescriptor>
  {
    public abstract void Append(IRedoDescriptor redoDescriptor);
    public abstract IEnumerator<IRedoDescriptor> GetEnumerator(Direction direction);

    public IEnumerator<IRedoDescriptor> GetEnumerator()
    {
      return GetEnumerator(Direction.Positive);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      IEnumerator<IRedoDescriptor> enumerator = GetEnumerator(Direction.Positive);
      while (enumerator.MoveNext()) {
        yield return enumerator.Current;
      }
    }

    protected void ValidateDescriptor(IRedoDescriptor redoDescriptor)
    {
      if (RedoScope.CurrentDescriptor!=redoDescriptor)
        throw new InvalidOperationException(Strings.ExSpecifiedRedoDescriptorCantBeLogged);
      if (redoDescriptor is BlockingRedoDescriptor)
        throw new InvalidOperationException(Strings.ExSpecifiedRedoDescriptorCantBeLogged);
    }
  }
}