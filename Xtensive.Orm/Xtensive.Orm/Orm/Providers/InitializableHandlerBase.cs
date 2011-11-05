// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.14

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Abstract base class for storage handlers having <see cref="Initialize"/> method.
  /// </summary>
  public abstract class InitializableHandlerBase: HandlerBase
  {
    /// <summary>
    /// Initializer. 
    /// Invoked right after creation and initial configuration of the handler.
    /// </summary>
    public abstract void Initialize();
  }
}