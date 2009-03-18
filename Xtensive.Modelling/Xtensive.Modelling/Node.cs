// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Threading;
using Xtensive.Modelling.Attributes;
using Xtensive.Modelling.Resources;
using Xtensive.Core.Reflection;

namespace Xtensive.Modelling
{
  /// <summary>
  /// An abstract base class for model node.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Name}")]
  public abstract class Node : LockableBase,
    INode,
    IDeserializationCallback
  {
    /// <summary>
    /// Path delimiter character.
    /// </summary>
    public static readonly char PathDelimiter = '/';
    /// <summary>
    /// Path escape character.
    /// </summary>
    public static readonly char PathEscape = '\\';

    [NonSerialized]
    private static ThreadSafeDictionary<Type, ReadOnlyDictionary<string, PropertyAccessor>> cachedPropertyAccessors = 
      new ThreadSafeDictionary<Type, ReadOnlyDictionary<string, PropertyAccessor>>();
    [NonSerialized]
    private ThreadSafeCached<Model> cachedModel = new ThreadSafeCached<Model>();
    [NonSerialized]
    private string cachedPath;
    [NonSerialized]
    private INesting nesting;
    [NonSerialized]
    private ReadOnlyDictionary<string, PropertyAccessor> propertyAccessors;
    [NonSerialized]
    internal Node parent;
    private string name;
    private NodeState state;
    private int index;

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
    public string Name
    {
      [DebuggerStepThrough]
      get { return name; }
    }

    /// <inheritdoc/>
    public string EscapedName
    {
      [DebuggerStepThrough]
      get { return new[] {Name}.RevertibleJoin(PathEscape, PathDelimiter); }
    }

    /// <inheritdoc/>
    public NodeState State {
      get { return state; }
    }

    /// <inheritdoc/>
    public int Index
    {
      [DebuggerStepThrough]
      get { return index; }
    }

    /// <inheritdoc/>
    public INesting Nesting {
      [DebuggerStepThrough]
      get { return nesting; }
    }

    /// <inheritdoc/>
    public ReadOnlyDictionary<string, PropertyAccessor> PropertyAccessors {
      [DebuggerStepThrough]
      get { return propertyAccessors; }
    }

    /// <inheritdoc/>
    public object GetProperty(string propertyName)
    {
      return PropertyAccessors[propertyName].Getter.Invoke(this);
    }

    /// <inheritdoc/>
    public void SetProperty(string propertyName, object value)
    {
      PropertyAccessors[propertyName].Setter.Invoke(this, value);
    }

    /// <inheritdoc/>
    public string Path {
      [DebuggerStepThrough]
      get {
        if (cachedPath!=null)
          return cachedPath;
        if (Parent==null)
          return string.Empty;
        else {
          string parentPath = Parent.Path;
          if (parentPath.Length!=0)
            parentPath += PathDelimiter;
          return string.Concat(
            parentPath, 
            Nesting.EscapedPropertyName, PathDelimiter, 
            Nesting.IsCollectionProperty ? EscapedName : string.Empty);
        }
      }
    }

    /// <inheritdoc/>
    public IPathNode GetChild(string path)
    {
      if (path.IsNullOrEmpty())
        return this;
      var parts = path.RevertibleSplitFirstAndTail(Node.PathEscape, Node.PathDelimiter);
      var next = (IPathNode) GetProperty(parts.First);
      if (parts.Second==null)
        return next;
      return next.GetChild(parts.Second);
    }

    #region Operations

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid node state.</exception>
    public virtual void Move(Node newParent, string newName, int newIndex)
    {
      if (State==NodeState.Removed)
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
      this.EnsureNotLocked();
      if (newParent==Parent && newName==Name && newIndex==Index)
        return;
      ValidateMove(newParent, newName, newIndex);
      // TODO: Change NodeCollection
      state = NodeState.Live;
      name = newName;
      parent = (Node) newParent;
      index = newIndex;
    }

    /// <inheritdoc/>
    public void Move(Node newParent)
    {
      ArgumentValidator.EnsureArgumentNotNull(newParent, "newParent");
      if (newParent==Parent)
        return;
      INodeCollection collection = null;
      if (Nesting.IsCollectionProperty)
        collection = (INodeCollection) Nesting.PropertyAccessor(newParent);
      Move(newParent, Name, collection==null ? 0 : collection.Count);
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
    /// <exception cref="InvalidOperationException">Invalid node state.</exception>
    public void Remove()
    {
      EnsureIsEditable();
      ValidateRemove();
      // TODO: Change NodeCollection
      parent = null;
      Lock(true);
    }

    #endregion

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
    protected virtual void ValidateMove(Node newParent, string newName, int newIndex)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(newName, "newName");
      if (this is Model) {
        ArgumentValidator.EnsureArgumentIsInRange(newIndex, 0, 0, "newIndex");
        return;
      }

      // Validating parent model
      ArgumentValidator.EnsureArgumentNotNull(newParent, "newParent");
      ArgumentValidator.EnsureArgumentIs<Node>(newParent, "newParent");
      var model = Model;
      if (model!=null) {
        var newModel = newParent.Model;
        if (model!=newModel)
          throw new ArgumentOutOfRangeException("newParent.Model");
      }

      // Validation parent collection nesting
      INodeCollection collection = null;
      if (Nesting.IsCollectionProperty)
        collection = (INodeCollection) Nesting.PropertyAccessor(newParent);
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

    #region EnsureXxxx methods

    protected void EnsureIsLive()
    {
      if (State!=NodeState.Live)
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
    }

    protected void EnsureIsEditable()
    {
      if (State!=NodeState.Live)
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
      this.EnsureNotLocked();
    }

    #endregion

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      // TODO: Process all collection properties
      cachedPath = Path;
    }

    protected abstract INesting CreateNesting();

    /// <inheritdoc/>
    public override string ToString()
    {
      return name;
    }

    #region Private \ internal methods

    private void Initialize()
    {
      propertyAccessors = GetPropertyAccessors(GetType());
    }

    private static ReadOnlyDictionary<string, PropertyAccessor> GetPropertyAccessors(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      return cachedPropertyAccessors.GetValue(type,
        (_type) => {
          var d = new Dictionary<string, PropertyAccessor>();
          if (_type!=typeof(object))
            foreach (var pair in GetPropertyAccessors(_type.BaseType))
              d.Add(pair.Key, pair.Value);
          foreach (var p in _type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
            if (p.GetAttribute<NodePropertyAttribute>(AttributeSearchOptions.InheritNone)!=null)
              d.Add(p.Name, new PropertyAccessor(p));
          }
          return new ReadOnlyDictionary<string, PropertyAccessor>(d, false);
        });
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="parent"><see cref="Parent"/> property value.</param>
    /// <param name="name">Initial <see cref="Name"/> property value.</param>
    /// <param name="index">Initial <see cref="Index"/> property value.</param>
    /// <exception cref="InvalidOperationException"><see cref="CreateNesting"/> has returned <see langword="null" />.</exception>
    protected Node(Node parent, string name, int index)
    {
      ArgumentValidator.EnsureArgumentNotNull(parent, "parent");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      nesting = CreateNesting();
      if (nesting==null)
        throw new InvalidOperationException(Strings.ExNoNesting);
      Initialize();
      Move(parent, name, index);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="name">Initial <see cref="Name"/> property value.</param>
    /// <exception cref="InvalidOperationException"><see cref="CreateNesting"/> has returned <see langword="null" />.</exception>
    internal Node(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      nesting = CreateNesting();
      if (nesting==null)
        throw new InvalidOperationException(Strings.ExNoNesting);
      Initialize();
      Rename(name);
    }

    // Deserialization

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"><see cref="CreateNesting"/> has returned <see langword="null" />.</exception>
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      nesting = CreateNesting();
      if (nesting==null)
        throw new InvalidOperationException(Strings.ExNoNesting);
      Initialize();
    }
  }
}