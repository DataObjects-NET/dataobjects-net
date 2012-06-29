// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;

namespace Xtensive.Modelling
{
  /// <summary>
  /// <see cref="Node"/> cloning context.
  /// </summary>
  [Serializable]
  public class CloningContext : IContext<CloningScope>
  {
    private List<Action> fixups;

    /// <summary>
    /// Gets all the added fixups.
    /// </summary>
    public IEnumerable<Action> Fixups {
      get { return fixups ?? EnumerableUtils<Action>.Empty; }
    }

    /// <summary>
    /// Adds the new fixup to fixups sequence.
    /// </summary>
    /// <param name="fixup">The fixup to add.</param>
    public void AddFixup(Action fixup)
    {
      if (fixups==null)
        fixups = new List<Action>();
      fixups.Add(fixup);
    }

    /// <summary>
    /// Clears all the fixups.
    /// </summary>
    public void ClearFixups()
    {
      fixups = null;
    }

    /// <summary>
    /// Applies all the fixups.
    /// </summary>
    public void ApplyFixups()
    {
      if (fixups==null)
        return;
      foreach (var fixup in fixups)
        fixup.Invoke();
      fixups = null;
    }

    /// <summary>
    /// Gets the current validation context.
    /// </summary>
    public static CloningContext Current {
      get {
        return CloningScope.CurrentContext;
      }
    }

    #region IContext<...> methods

    /// <inheritdoc/>
    public CloningScope Activate()
    {
      return new CloningScope(this);
    }

    /// <inheritdoc/>
    public bool IsActive
    {
      get { return Current==this; }
    }

    /// <inheritdoc/>
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public CloningContext()
    {
    }
  }
}