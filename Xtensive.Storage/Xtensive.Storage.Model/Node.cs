// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.25

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Notifications;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Represents base storage model node.
  /// </summary>
  [Serializable]
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
      get { return name; }
      set
      {
        this.EnsureNotLocked();
        Validate(value);
        ChangeState("Name", delegate { name = value; });
      }
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
    /// <param name="nameToValidate">The new name of this instance.</param>
    protected virtual void Validate(string nameToValidate)
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

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    protected Node(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
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