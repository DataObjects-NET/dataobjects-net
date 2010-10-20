// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.25

using System;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Notifications;
using Xtensive.Reflection;
using Xtensive.Storage.Model.Resources;

namespace Xtensive.Storage.Model
{
  /// <summary>
  ///An abstract base class for model node.
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
      set {
        this.EnsureNotLocked();
        ValidateName(value);
        ChangeState("Name", delegate { name = value; });
      }
    }

    private void ChangeState(string property, Action onChangeStateDelegate)
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

    /// <summary>
    /// Updates the internal state of this instance.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be updated as well.</param>
    public virtual void UpdateState(bool recursive)
    {
      this.EnsureNotLocked();
    }

    /// <summary>
    /// Updates the internal state of this instance.
    /// </summary>
    public void UpdateState()
    {
      UpdateState(false);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.NodeFormat, 
        name ?? Strings.UnnamedNodeDisplayName, GetType().GetShortName());
    }


    // Constructors
    
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