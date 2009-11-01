// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.28

using Xtensive.Core;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Redo operation descriptor.
  /// </summary>
  public interface IRedoDescriptor: IOperationDescriptor, 
    IContext<RedoScope>
  {
    IUndoDescriptor OppositeDescriptor { get; set; }
    bool IsUndone { get; }
    object[] Arguments { get; set; }
  }
}