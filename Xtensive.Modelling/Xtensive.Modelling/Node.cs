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
    INode
  {
    [NonSerialized]
    internal Node parent;
    [NonSerialized]
    private Cached<Model> cachedModel;
    private string name;
    private int index;

    /// <inheritdoc/>
    public bool IsRemoved {
      get { return Parent!=null ? false : !(this is Model); }
    }

    /// <inheritdoc/>
    public string Name
    {
      [DebuggerStepThrough]
      get { return name; }
    }

    /// <inheritdoc/>
    public int Index
    {
      [DebuggerStepThrough]
      get { return index; }
    }

    /// <inheritdoc/>
    public Node Parent {
      [DebuggerStepThrough]
      get { return parent; }
    }

    /// <inheritdoc/>
    public Model Model {
      [DebuggerStepThrough]
      get {
        return cachedModel.GetValue((_this) => {
          var node = _this;
          while (true) {
            var next = node.Parent;
            if (next==null)
              return node as Model;
            node = next;
          }
        }, this);
      }
    }

    /// <inheritdoc/>
    public string Path {
      [DebuggerStepThrough]
      get {
        // TODO: Escape "."
        if (Parent==null)
          return Name;
        else {
          return Parent.Path + "." + Name;
        }
      }
    }

    /// <inheritdoc/>
    public INodeCollection NodeCollection {
      [DebuggerStepThrough]
      get {
        return Parent==null ? null : GetNodeCollection(Parent);
      }
    }

    /// <inheritdoc/>
    public abstract INodeCollection GetNodeCollection(INode parent);

    public IPathNode GetChild(string path)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public virtual void Move(INode newParent, string newName, int newIndex)
    {
      this.EnsureNotLocked();
      if (newParent==Parent && newName==Name && newIndex==Index)
        return;
      ValidateMove(newParent, newName, newIndex);
      // TODO: Change NodeCollection
      name = newName;
      parent = (Node) newParent;
      index = newIndex;
    }

    /// <inheritdoc/>
    public void Move(INode newParent)
    {
      ArgumentValidator.EnsureArgumentNotNull(newParent, "newParent");
      if (newParent==Parent)
        return;
      Move(newParent, Name, GetNodeCollection(newParent).Count);
    }

    /// <inheritdoc/>
    public void Move(int newIndex)
    {
      Move(Parent, Name, newIndex);
    }

    /// <inheritdoc/>
    public void Rename(string newName)
    {
      Move(Parent, newName, Index);
    }

    /// <inheritdoc/>
    public void Remove()
    {
      this.EnsureNotLocked();
      ValidateRemove();
      // TODO: Change NodeCollection
      parent = null;
      Lock(true);
    }

    #region ValidateXxx methods

    /// <summary>
    /// Validates the <see cref="Move"/> and <see cref="Rename"/> method arguments.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="newIndex">The new index.</param>
    /// <exception cref="ArgumentException">Item already exists.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="newIndex"/> is out of range, 
    /// or <paramref name="newParent"/> belongs to a different <see cref="Model"/>.</exception>
    protected virtual void ValidateMove(INode newParent, string newName, int newIndex)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(newName, "newName");
      if (this is Model) {
        ArgumentValidator.EnsureArgumentIsInRange(newIndex, 0, 0, "newIndex");
        return;
      }

      ArgumentValidator.EnsureArgumentNotNull(newParent, "newParent");
      ArgumentValidator.EnsureArgumentIs<Node>(newParent, "newParent");
      var model = Model;
      if (model!=null) {
        var newModel = newParent.Model;
        if (model!=newModel)
          throw new ArgumentOutOfRangeException("newParent.Model");
      }

      var collection = GetNodeCollection(newParent);
      if (collection==null)
        return;
      ArgumentValidator.EnsureArgumentIsInRange(newIndex, 0, collection.Count - (newParent==Parent ? 1 : 0), "newIndex");
      Node node;
      if (!collection.TryGetValue(newName, out node))
        return;
      if (node!=this)
        throw new ArgumentException(String.Format(
          Strings.ExItemWithNameXAlreadyExists, newName), newName);
    }

    /// <summary>
    /// Validates the <see cref="Remove"/> method call.
    /// </summary>
    /// <exception cref="InvalidOperationException">Model object cannot be removed.</exception>
    protected virtual void ValidateRemove()
    {
      if (this is Model)
        throw new InvalidOperationException(Strings.ExModelObjectCannotBeRemoved);
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
    /// <param name="index">Initial <see cref="Index"/> property value.</param>
    protected Node(Node parent, string name, int index)
    {
      ArgumentValidator.EnsureArgumentNotNull(parent, "parent");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Move(parent, name, index);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="name">Initial <see cref="Name"/> property value.</param>
    internal Node(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Rename(name);
    }
  }
}