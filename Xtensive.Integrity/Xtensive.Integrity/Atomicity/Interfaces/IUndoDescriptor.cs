// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.28

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Undo operation descriptor.
  /// </summary>
  public interface IUndoDescriptor: IOperationDescriptor, 
    IContext<UndoScope>
  {
    IRedoDescriptor OppositeDescriptor { get; set; }
    IGroupUndoDescriptor Group { get; set; }
    IDictionary<string, object> Arguments { get; }
    bool IsCompleted { get; }
    void Complete();
  }
}