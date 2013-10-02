// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.20

using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// <see cref="ActionHandler"/> activation scope.
  /// </summary>
  public sealed class ActionHandlerScope : Scope<ActionHandler>
  {
    /// <summary>
    /// Gets the current <see cref="ActionHandler"/>.
    /// </summary>
    public static ActionHandler CurrentHandler {
      get {
        return CurrentContext ?? NullActionHandler.Instance;
      }
    }
    
    /// <summary>
    /// Gets the associated <see cref="ActionHandler"/>.
    /// </summary>
    public ActionHandler Handler {
      get {
        return Context;
      }
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context.</param>
    public ActionHandlerScope(ActionHandler context)
      : base(context)
    {
    }
  }
}