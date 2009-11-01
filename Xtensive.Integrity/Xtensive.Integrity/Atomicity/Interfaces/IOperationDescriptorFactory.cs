// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.28

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// <see cref="IOperationDescriptor"/> factory interface - 
  /// a factory used by <see cref="AtomicityContextBase"/> descendants 
  /// to create its own operation descriptors.
  /// </summary>
  public interface IOperationDescriptorFactory
  {
    IRedoDescriptor CreateRedoDescriptor();
    IUndoDescriptor CreateUndoDescriptor();
    IUndoDescriptor CreateGroupUndoDescriptor();
  }
}