// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.23

using System;
using System.Diagnostics;

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

    public MethodCallDescriptor CallDescriptor
    {
      [DebuggerStepThrough]
      get { return callDescriptor; }
      [DebuggerStepThrough]
      set { callDescriptor = value; }
    }

    public abstract void Invoke();
    
    
    public abstract void Finalize(bool isUndone);

    /// <inheritdoc/>
    public override string ToString()
    {
      return callDescriptor.ToString();
    }
  }
}