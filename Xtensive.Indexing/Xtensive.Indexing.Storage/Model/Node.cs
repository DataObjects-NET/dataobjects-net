// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Collections;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Notifications;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  ///An abstract base class for model node.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Name}")]
  public abstract class Node
    : LockableBase,
      IChangeNotifier,
      IHasExtensions
  {
    private string name;
    private ExtensionCollection extensions;

    /// <summary>
    /// Gets the name of this instance.
    /// </summary>
    public string Name
    {
      [DebuggerStepThrough]
      get { return name; }
      set {
        this.EnsureNotLocked();
        ValidateName(value);
        ChangeState("Name", delegate { name = value; });
      }
    }

    /// <summary>
    /// Gets the parent node.
    /// </summary>
    public Node Parent
    { 
      [DebuggerStepThrough]
      get; 
      [DebuggerStepThrough]
      private set;
    }

    /// <summary>
    /// Changes the state of this instance.
    /// </summary>
    /// <param name="property">The property to change.</param>
    /// <param name="onChangeStateDelegate">Delegate that changes the state of this instance.</param>
    protected void ChangeState(string property, Action onChangeStateDelegate)
    {
      if (Changing != null)
        Changing(this, new ChangeNotifierEventArgs(property));
      onChangeStateDelegate();
      if (Changed != null)
        Changed(this, new ChangeNotifierEventArgs(property));
    }

    /// <summary>
    /// Performs additional custom processes before setting new name to this instance.
    /// </summary>
    /// <param name="newName">The new name of this instance.</param>
    protected virtual void ValidateName(string newName)
    {
    }

    #region IChangeNotifier Members

    /// <inheritdoc/>
    public event EventHandler<ChangeNotifierEventArgs> Changing;

    /// <inheritdoc/>
    public event EventHandler<ChangeNotifierEventArgs> Changed;

    #endregion

    #region IExtensionCollection Members

    public IExtensionCollection Extensions {
      get {
        if (extensions != null)
          return extensions;

        lock (this) {
          if (extensions == null)
            extensions = new ExtensionCollection();
        }

        return extensions;
      }
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return name;
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="parent"><see cref="Parent"/> property value.</param>
    /// <param name="name">Initial <see cref="Name"/> property value.</param>
    protected Node(Node parent, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(parent, "parent");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Parent = parent;
      Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="name">Initial <see cref="Name"/> property value.</param>
    protected Node(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Parent = null;
      Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    protected Node()
    {
    }
  }
}