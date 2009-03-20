// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Modelling;
using Xtensive.Modelling.Resources;

namespace Xtensive.Modelling
{
  /// <summary>
  /// An abstract base class for collection of nodes in model.
  /// </summary>
  [Serializable]
  public abstract class NodeCollection : LockableBase,
    INodeCollection,
    IDeserializationCallback
  {
    private Node parent;
    [NonSerialized]
    private string name;
    [NonSerialized]
    private string escapedName;
    [NonSerialized]
    private string cachedPath;
    [NonSerialized]
    private Dictionary<string, Node> nameIndex = new Dictionary<string, Node>();
    private readonly List<Node> list = new List<Node>();


    #region Properties

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Property is already initialized.</exception>
    public string Name
    {
      [DebuggerStepThrough]
      get { return name; }
      [DebuggerStepThrough]
      set {
        throw Exceptions.AlreadyInitialized("Name");
      }
    }

    /// <inheritdoc/>
    public string EscapedName
    {
      [DebuggerStepThrough]
      get { return escapedName; }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Property is already initialized.</exception>
    public Node Parent
    {
      [DebuggerStepThrough]
      get { return parent; }
      [DebuggerStepThrough]
      set {
        throw Exceptions.AlreadyInitialized("Parent");
      }
    }

    /// <inheritdoc/>
    public Node Model {
      [DebuggerStepThrough]
      get { return Parent.Model; }
    }

    /// <inheritdoc/>
    public string Path {
      [DebuggerStepThrough]
      get {
        if (cachedPath!=null)
          return cachedPath;
        var parentPath = Parent.Path;
        if (parentPath.Length==0)
          return EscapedName;
        return string.Concat(
          parentPath, Node.PathDelimiter, 
          EscapedName);
      }
    }

    /// <inheritdoc/>
    long ICountable.Count {
      [DebuggerStepThrough]
      get { return Count; }
    }

    /// <inheritdoc/>
    public int Count {
      [DebuggerStepThrough]
      get { return list.Count; }
    }

    #endregion

    /// <inheritdoc/>
    public Node this[int index] {
      [DebuggerStepThrough]
      get {
        return list[index];
      }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Item is not found.</exception>
    public Node this[string name] {
      [DebuggerStepThrough]
      get {
        Node result;
        if (!TryGetValue(name, out result))
          throw new ArgumentException(String.Format(String.Format(
            Strings.ExItemWithNameXIsNotFound, name)));
        return result;
      }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public bool TryGetValue(string name, out Node value)
    {
      return nameIndex.TryGetValue(name, out value);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public bool Contains(string name)
    {
      return nameIndex.ContainsKey(name);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public virtual string GetTemporaryName()
    {
      return Guid.NewGuid().ToString();
    }

    /// <inheritdoc/>
    public IPathNode GetChild(string path)
    {
      if (path.IsNullOrEmpty())
        return this;
      var parts = path.RevertibleSplitFirstAndTail(Node.PathEscape, Node.PathDelimiter);
      var next = this[parts.First];
      if (parts.Second==null)
        return next;
      return next.GetChild(parts.Second);
    }

    /// <inheritdoc/>
    public void Validate()
    {
      foreach (var node in list)
        node.Validate();
    }

    #region IEnumerable<...> members

    [DebuggerStepThrough]
    public IEnumerator GetEnumerator()
    {
      return list.GetEnumerator();
    }

    #endregion

    #region ILockable members

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive)
        foreach (Node node in this)
          node.Lock(recursive);
      cachedPath = Path;
    }

    #endregion

    #region Private \ internal methods

    /// <exception cref="InvalidOperationException">Wrong DoneCollection.Add arguments: node.Index!=list.Count!</exception>
    internal void Add(Node node)
    {
      if (node.Index!=list.Count)
        throw Exceptions.InternalError("Wrong NodeCollection.Add arguments: node.Index!=list.Count!", Log.Instance);
      int count = list.Count;
      try {
        list.Add(node);
        nameIndex.Add(node.Name, node);
      }
      catch {
        if (list.Count>count)
          list.RemoveAt(count);
        if (nameIndex.Count>count)
          nameIndex.Remove(node.Name);
        throw;
      }
    }

    /// <exception cref="InvalidOperationException">Wrong DoneCollection.Add arguments: node.Index!=list.Count!</exception>
    internal void Remove(Node node)
    {
      int count = list.Count;
      int index = node.Index;
      try {
        list.RemoveAt(index);
        nameIndex.Remove(node.Name);
      }
      catch {
        if (list.Count<count)
          list.Insert(index, node);
        if (nameIndex.Count<count)
          nameIndex.Add(node.Name, node);
        throw;
      }
    }

    internal void Move(Node node, int newIndex)
    {
      int count = list.Count;
      int oldIndex = node.Index;
      try {
        list.RemoveAt(oldIndex);
      }
      catch {
        if (list.Count<count)
          list.Insert(oldIndex, node);
        throw;
      }
      try {
        list.Insert(newIndex, node);
      }
      catch {
        if (list.Count==count) {
          list.RemoveAt(newIndex);
          list.Insert(oldIndex, node);
        }
        if (list.Count<count)
          list.Insert(oldIndex, node);
        throw;
      }
    }

    #endregion

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="Name"/> is not initialized yet.</exception>
    protected virtual void Initialize()
    {
      if (Name.IsNullOrEmpty())
        throw Exceptions.NotInitialized("Name");
      escapedName = new[] {Name}.RevertibleJoin(Node.PathEscape, Node.PathDelimiter);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      var m = Model;
      string fullName = Path;
      if (m!=null)
        fullName = string.Concat(m.EscapedName, Node.PathDelimiter, fullName);
      return string.Format(Strings.NodeInfoFormat, fullName, Count);
    }

    /// <inheritdoc/>
    public virtual void Dump()
    {
      if (Count==0) {
        Log.Info("None");
        return;
      }
      foreach (var node in list)
        node.Dump();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent"><see cref="Parent"/> property value.</param>
    /// <param name="name"><see cref="Name"/> property value.</param>
    protected NodeCollection(Node parent, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(parent, "parent");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      this.name = name;
      this.parent = parent;
      Initialize();
    }

    // Deserialization

    /// <inheritdoc/>
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      Initialize();
      nameIndex = new Dictionary<string, Node>(Count);
      foreach (var node in list)
        nameIndex.Add(node.Name, node);
      if (IsLocked) {
        cachedPath = Path;
      }
    }
  }
}

