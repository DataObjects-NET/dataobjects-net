// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Notifications;
using Xtensive.Modelling;
using Xtensive.Modelling.Resources;

namespace Xtensive.Modelling
{
  /// <summary>
  /// A base class for collection of nodes in model.
  /// </summary>
  /// <typeparam name="TNode">The type of the node.</typeparam>
  /// <typeparam name="TParent">The type of the parent.</typeparam>
  /// <typeparam name="TModel">The type of the model.</typeparam>
  [Serializable]
  public abstract class NodeCollection<TNode, TParent, TModel> : CollectionBase<TNode>,
    INodeCollection<TNode>,
    IDeserializationCallback
    where TNode: Node
    where TParent: Node
    where TModel: Model
  {
    [NonSerialized]
    private Dictionary<string, TNode> nameIndex = new Dictionary<string, TNode>();

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public TParent Parent {
      [DebuggerStepThrough]
      get; 
      [DebuggerStepThrough]
      private set;
    }

    /// <inheritdoc/>
    Node IPathNode.Parent {
      get { return Parent; }
    }

    /// <inheritdoc/>
    public Model Model {
      get { return Parent.Model; }
    }

    /// <inheritdoc/>
    public string Path {
      get {
        // TODO: Escape "."
        return Parent.Path + "." + Name;
      }
    }

    /// <inheritdoc/>
    public IPathNode GetChild(string path)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public string GetTemporaryName()
    {
      return Guid.NewGuid().ToString();
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Item is not found.</exception>
    public TNode this[string name] {
      [DebuggerStepThrough]
      get {
        TNode result;
        if (!TryGetValue(name, out result))
          throw new ArgumentException(String.Format(String.Format(
            Strings.ExItemWithNameXIsNotFound, name)));
        return result;
      }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public bool TryGetValue(string name, out TNode value)
    {
      return nameIndex.TryGetValue(name, out value);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Item already exists.</exception>
    /// <exception cref="InstanceIsLockedException">Collection is locked.</exception>
    [DebuggerStepThrough]
    public override void Add(TNode item)
    {
      try {
        base.Add(item);
      }
      catch (InstanceIsLockedException) {
        throw;
      }
      catch (ArgumentException e) {
        throw new InvalidOperationException(
          string.Format(CultureInfo.InvariantCulture, Strings.ExItemWithNameXAlreadyExists, item.Name), e);
      }
      catch (InvalidOperationException e) {
        throw new InvalidOperationException(
          string.Format(CultureInfo.InvariantCulture, Strings.ExItemWithNameXAlreadyExists, item.Name), e);
      }
    }

    #region INodeCollection members

    /// <inheritdoc/>
    Node INodeCollection.this[string name]
    {
      [DebuggerStepThrough]
      get { return this[name]; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    bool INodeCollection.TryGetValue(string name, out Node value)
    {
      TNode node;
      bool result = TryGetValue(name, out node);
      value = node;
      return result;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    bool INodeCollection.Contains(string name)
    {
      return nameIndex.ContainsKey(name);
    }

    #endregion

    #region ICollection<Node> members

    /// <inheritdoc/>
    void ICollection<Node>.Add(Node item)
    {
      Add((TNode) item);
    }

    /// <inheritdoc/>
    bool ICollection<Node>.Contains(Node item)
    {
      return Contains((TNode) item);
    }

    /// <inheritdoc/>
    void ICollection<Node>.CopyTo(Node[] array, int arrayIndex)
    {
      base.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    bool ICollection<Node>.Remove(Node item)
    {
      return Remove((TNode) item);
    }

    #endregion

    #region IEnumerable<...> members

    /// <inheritdoc/>
    IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
    {
      foreach (TNode node in this)
        yield return node;
    }

    #endregion

    #region ILockable members

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive)
        foreach (TNode node in this)
          node.Lock(recursive);
    }

    #endregion

    #region OnXxx handlers

    /// <inheritdoc/>
    protected override void OnInserted(TNode value, int index)
    {
      base.OnInserted(value, index);
      nameIndex.Add(value.Name, value);
    }

    /// <inheritdoc/>
    protected override void OnRemoved(TNode value, int index)
    {
      base.OnRemoved(value, index);
      nameIndex.Remove(value.Name);
    }

    /// <inheritdoc/>
    protected override void OnCleared()
    {
      base.OnCleared();
      nameIndex.Clear();
    }

    /// <inheritdoc/>
    protected override void OnItemChanging(object sender, ChangeNotifierEventArgs e)
    {
      base.OnItemChanging(sender, e);
      nameIndex.Remove(((TNode) sender).Name);
    }

    /// <inheritdoc/>
    protected override void OnItemChanged(object sender, ChangeNotifierEventArgs e)
    {
      base.OnItemChanged(sender, e);
      var tNode = (TNode)sender;
      nameIndex.Add(tNode.Name, tNode);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Invalid <paramref name="value"/>.Parent value.</exception>
    protected override void OnValidate(TNode value)
    {
      base.OnValidate(value);
      if (Parent!=null && value.Parent!=Parent)
        throw new ArgumentException(Strings.ExInvalidParentValue, "value.Parent");
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent"><see cref="Parent"/> property value.</param>
    protected NodeCollection(TParent parent)
    {
      ArgumentValidator.EnsureArgumentNotNull(parent, "parent");
      Parent = parent;
    }

    // Deserialization

    /// <inheritdoc/>
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      nameIndex = new Dictionary<string, TNode>(Count);
      foreach (var node in this) {
        nameIndex.Add(node.Name, node);
        node.parent = Parent;
        TrySubscribe(node);
      }
    }
  }
}

