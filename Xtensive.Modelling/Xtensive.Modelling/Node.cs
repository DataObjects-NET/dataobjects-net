// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Notifications;
using Xtensive.Modelling.Resources;

namespace Xtensive.Modelling
{
  /// <summary>
  /// An abstract base class for model node.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Name}")]
  public abstract class Node : LockableBase,
    IPathNode
  {
    [NonSerialized]
    private Node parent;
    [NonSerialized]
    private Model model;
    private string name;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public Node Parent {
      [DebuggerStepThrough]
      get { return parent; }
      [DebuggerStepThrough]
      internal set { parent = value; }
    }

    /// <inheritdoc/>
    public Model Model {
      get {
        Node node = this;
        while (true) {
          Node next = node.Parent;
          if (next==null)
            return node as Model;
          node = next;
        }
      }
    }

    /// <inheritdoc/>
    public string Path {
      get {
        // TODO: Escape "."
        if (Parent==null)
          return Name;
        else {
          return Parent.Path + "." + Name;
        }
      }
    }

    public IPathNode GetChild(string path)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public abstract INodeCollection Collection { get; }

    /// <summary>
    /// Performs validation before setting new <see cref="Name"/> of this instance.
    /// </summary>
    /// <param name="newName">The new <see cref="Name"/> value to validate.</param>
    /// <exception cref="ArgumentException">Item with specified name already exists 
    /// in parent node collection.</exception>
    protected virtual void ValidateName(string newName)
    {
      var collection = Collection;
      if (collection==null)
        return;
      Node node;
      if (!collection.TryGetValue(newName, out node))
        return;
      if (node==this)
        return;
      throw new ArgumentException(String.Format(
        Strings.ExItemWithNameXAlreadyExists, newName), newName);
    }

    #region IChangeNotifier & related members

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

    /// <inheritdoc/>
    [field: NonSerialized]
    public event EventHandler<ChangeNotifierEventArgs> Changing;

    /// <inheritdoc/>
    [field: NonSerialized]
    public event EventHandler<ChangeNotifierEventArgs> Changed;

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
  }
}