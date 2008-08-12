// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.23

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Integrity.Aspects;
using Xtensive.Core.Helpers;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Default implementation for <see cref="IOperationDescriptor"/>.
  /// Base class for any operation descriptor.
  /// </summary>
  [Serializable]
  public abstract class OperationDescriptor: IOperationDescriptor
  {
    private MethodCallDescriptor callDescriptor;

    [DebuggerStepThrough]
    public MethodCallDescriptor CallDescriptor
    {
      get { return callDescriptor; }
      set { callDescriptor = value; }
    }

    public abstract void Invoke();
    public abstract void Finalize(bool isUndone);

    public override string ToString()
    {
      return callDescriptor.ToString();
    }
  }
}