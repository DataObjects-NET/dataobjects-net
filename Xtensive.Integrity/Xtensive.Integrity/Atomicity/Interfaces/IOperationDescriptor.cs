// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.28

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Common methods for <see cref="IUndoDescriptor"/> and <see cref="IRedoDescriptor"/>.
  /// </summary>
  public interface IOperationDescriptor
  {
    MethodCallDescriptor CallDescriptor { get; set; }
    void Invoke();
    void Finalize(bool isUndone);
  }
}