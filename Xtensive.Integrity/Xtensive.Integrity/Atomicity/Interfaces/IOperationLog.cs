// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System.Collections.Generic;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Operation log -
  /// a log used by <see cref="AtomicityContextBase"/> descendants 
  /// to log created operation descriptors.
  /// </summary>
  public interface IOperationLog
  {
    void Append(IRedoDescriptor redoDescriptor);
  }
}