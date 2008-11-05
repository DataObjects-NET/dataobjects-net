// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.26

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Defines priority of storage-related aspects.
  /// </summary>
  public enum StorageAspectPriority
  {
    /// <summary>
    /// Priority of <see cref="SessionBoundMethodAspect"/>
    /// </summary>
    SessionBound = -200000,

    /// <summary>
    /// Priority of <see cref="TransactionalAttribute"/>
    /// </summary>
    Transactional = -190000,

    /// <summary>
    /// Priority of <see cref="UsesTransactionalStateAttribute"/>
    /// </summary>
    UsesTransactionalState = -180000,
  }
}