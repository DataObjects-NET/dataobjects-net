// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Diagnostics;
using Xtensive.Core;


namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Defines the scope for the single action.
  /// </summary>
  public class ActionScope : IDisposable
  {
    private ActionSequence sequence;
    private NodeAction action;
    private bool isCommitted;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is committed.
    /// </summary>
    public bool IsCommitted {
      get { return isCommitted; }
    }

    /// <summary>
    /// Gets a value indicating whether this scope is committable (i.e. its 
    /// <see cref="Action"/> will be added to <see cref="Sequence"/> after
    /// <see cref="Commit"/> and <see cref="Dispose"/> calls).
    /// </summary>
    public bool IsCommittable {
      get { return sequence.CurrentScope==this; }
    }

    /// <summary>
    /// Gets the sequence this instance is created for.
    /// </summary>
    public ActionSequence Sequence {
      get { return sequence; }
    }

    /// <summary>
    /// Gets or sets the action this instance commits.
    /// </summary>
    /// <exception cref="NotSupportedException"><see cref="Commit"/> method is already called.</exception>
    public NodeAction Action {
      get { return action; }
      set {
        if (IsCommitted)
          throw Exceptions.AlreadyInitialized("Action");
        action = value;
      }
    }

    /// <exception cref="InvalidOperationException"><see cref="Action"/> is not initialized.</exception>
    public void Commit()
    {
      if (Action==null)
        throw Exceptions.NotInitialized("Action");
      isCommitted = true;
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ActionScope()
    {
      this.sequence = null;
    }

    internal ActionScope(ActionSequence sequence)
    {
      this.sequence = sequence;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      if (sequence!=null)
        sequence.OnCommit(this);
    }
  }
}