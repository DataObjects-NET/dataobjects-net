// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.26

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Defines priority of storage-related aspects.
  /// </summary>
  public enum StorageAspectPriority
  {
    /// <summary>
    /// Priority of <see cref="TransactionalAspect"/>
    /// </summary>
    Transactional = -200000,
  }
}