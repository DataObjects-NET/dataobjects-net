// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.22

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Enumerates non-transactional stages.
  /// </summary>
  public enum NonTransactionalStage
  {
    /// <summary>
    /// None.
    /// </summary>
    None = 0,
    /// <summary>
    /// Prologue.
    /// </summary>
    Prologue,
    /// <summary>
    /// Epilogue.
    /// </summary>
    Epilogue
  }
}