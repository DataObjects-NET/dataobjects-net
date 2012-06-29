// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.20

using System;
using Xtensive.Core;
using Xtensive.IoC;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// An abstract base class for any <see cref="IActionHandler"/> implementor.
  /// </summary>
  [Serializable]
  public abstract class ActionHandler : IActionHandler,
    IContext<ActionHandlerScope>
  {
    /// <summary>
    /// Gets the current <see cref="ActionHandler"/>.
    /// </summary>
    public static ActionHandler Current {
      get { return ActionHandlerScope.CurrentHandler; }
    }

    /// <inheritdoc/>
    public abstract void Execute(NodeAction action);

    #region IContext<...> implementation

    /// <inheritdoc/>
    public bool IsActive {
      get { return ActionHandlerScope.CurrentHandler==this; }
    }

    /// <inheritdoc/>
    public ActionHandlerScope Activate()
    {
      return new ActionHandlerScope(this);
    }

    /// <inheritdoc/>
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    #endregion
  }
}