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
      ThreadSafeDictionary<Type, ReadOnlyDictionary<string, PropertyAccessor>>.Create(new object());
    [NonSerialized]
    private ThreadSafeCached<Model> cachedModel = ThreadSafeCached<Model>.Create(new object());
    [NonSerialized]
    private string cachedPath;
    [NonSerialized]
    private Nesting nesting;
    [NonSerialized]
    private ReadOnlyDictionary<string, PropertyAccessor> propertyAccessors;
    internal Node parent;
    private string name;
    private NodeState state;
    private int index;

    #region Properties

    /// <inheritdoc/>
    public Node Parent {
      [DebuggerStepThrough]
      get { return parent; }
      [DebuggerStepThrough]
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "newParent");
        if (value==Parent)
          return;
        NodeCollection collection = null;
        if (Nesting.IsCollectionProperty)
          collection = (NodeCollection) Nesting.PropertyValue;
        Move(value, Name, collection==null ? 0 : collection.Count);
      }
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
      [DebuggerStepThrough]
      set {
        Move(Parent, value, Index);
      }
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
      [DebuggerStepThrough]
      set {
        Move(Parent, Name, value);
      }
    }

    /// <inheritdoc/>
    public Nesting Nesting {
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

    #endregion

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid node state.</exception>
    public void Move(Node newParent, string newName, int newIndex)
    {
      if (State==NodeState.Removed)
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
      this.EnsureNotLocked();
      if (newParent==Parent && newName==Name && newIndex==Index)
        return;
      ValidateMove(newParent, newName, newIndex);
      if (State==NodeState.Initializing) {
        parent = newParent;
        name = newName;
        index = newIndex;
        PerformCreate();
      }
      else
        PerformMove(newParent, newName, newIndex);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid node state.</exception>
    public void Remove()
    {
      EnsureIsEditable();
      ValidateRemove();
      PerformRemove();
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

      if (!Nesting.IsCollectionProperty)
        // Validation parent property nesting
        ArgumentValidator.EnsureArgumentIsInRange(newIndex, 0, 0, "newIndex");
      else {
        // Validation parent collection nesting
        var collection = (NodeCollection) Nesting.PropertyGetter(newParent);
        ArgumentValidator.EnsureArgumentIsInRange(newIndex, 0, collection.Count - (newParent==Parent ? 1 : 0), "newIndex");
        Node node;
        if (!collection.TryGetValue(newName, out node))
          return;
        if (node!=this)
          throw new ArgumentException(String.Format(
            Strings.ExItemWithNameXAlreadyExists, newName), newName);
      }
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

    #region PerformXxx methods

    /// <summary>
    /// Actually performs construction operation.
    /// </summary>
    protected virtual void PerformCreate()
    {
      if (Parent!=null) {
        if (!Nesting.IsCollectionProperty)
          Nesting.PropertyValue = this;
        else
          ((NodeCollection) Nesting.PropertyValue).Add(this);
      }
      state = NodeState.Live;
    }

    /// <summary>
    /// Actually performs <see cref="Move"/> operation.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="newIndex">The new index.</param>
    protected virtual void PerformMove(Node newParent, string newName, int newIndex)
    {
      if (newParent!=parent) {
        // Parent is changed
        if (!Nesting.IsCollectionProperty) {
          Nesting.PropertySetter(parent, null);
          Nesting.PropertySetter(newParent, this);
        }
        else {
          var oldCollection = (NodeCollection) Nesting.PropertyGetter(parent);
          var newCollection = (NodeCollection) Nesting.PropertyGetter(newParent);
          for (int i = index+1; i<oldCollection.Count; i++)
            oldCollection[i].EnsureIsEditable();
          for (int i = index; i<newCollection.Count; i++)
            newCollection[i].EnsureIsEditable();
          for (int i = index+1; i<oldCollection.Count; i++)
            oldCollection[i].PerformShift(-1);
          for (int i = index; i<newCollection.Count; i++)
            newCollection[i].PerformShift(1);
          oldCollection.Remove(this);
          newCollection.Add(this);
        }
      }
      else {
        // Parent isn't changed
        if (newIndex!=index) {
          var collection = (NodeCollection) Nesting.PropertyGetter(newParent);
          int minIndex, maxIndex, shift;
          if (newIndex < index) {
            minIndex = newIndex;
            maxIndex = index - 1;
            shift = 1;
          }
          else {
            minIndex = index + 1;
            maxIndex = newIndex;
            shift = -1;
          }
          for (int i = minIndex; i <= maxIndex; i++)
            collection[i].EnsureIsEditable();
          for (int i = minIndex; i <= maxIndex; i++)
            collection[i].PerformShift(shift);
          collection.Move(this, newIndex);
        }
      }
      name = newName;
      parent = (Node) newParent;
      index = newIndex;
    }

    /// <summary>
    /// Performs "shift" operation 
    /// (induced by <see cref="Move"/> operation of another node).
    /// </summary>
    /// <param name="offset">Shift offset.</param>
    protected virtual void PerformShift(int offset)
    {
      index += offset;
    }

    /// <summary>
    /// Actually performs <see cref="Remove"/> operation.
    /// </summary>
    protected void PerformRemove()
    {
      state = NodeState.Removed;
      if (!Nesting.IsCollectionProperty)
        Nesting.PropertyValue = null;
      else
        ((NodeCollection) Nesting.PropertyValue).Remove(this);
    }

    #endregion

    #region EnsureXxxx methods

    /// <exception cref="InvalidOperationException"><see cref="State"/> is invalid.</exception>
    protected void EnsureIsLive()
    {
      if (State!=NodeState.Live)
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
    }

    /// <exception cref="InvalidOperationException"><see cref="State"/> is invalid.</exception>
    protected void EnsureIsEditable()
    {
      if (State!=NodeState.Live)
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
      this.EnsureNotLocked();
    }

    #endregion

    #region ILockable methods

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      foreach (var pair in PropertyAccessors) {
        string propertyName = pair.Key;
        var accessor = pair.Value;
        if (accessor.HasGetter) {
          var lockable = GetProperty(pair.Key) as ILockable;
          if (lockable!=null)
            lockable.Lock();
        }
      }
      cachedPath = Path;
    }

    #endregion

    #region Private \ internal methods

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

    #region To override

    protected abstract Nesting CreateNesting();

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="CreateNesting"/> has returned <see langword="null" />.</exception>
    protected virtual void Initialize()
    {
      nesting = CreateNesting();
      if (nesting==null)
        throw new InvalidOperationException(Strings.ExNoNesting);
      propertyAccessors = GetPropertyAccessors(GetType());
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
      Initialize();
      Move(parent, name, index);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="parent"><see cref="Parent"/> property value.</param>
    /// <param name="name">Initial <see cref="Name"/> property value.</param>
    protected Node(Node parent, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(parent, "parent");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Initialize();
      if (!Nesting.IsCollectionProperty)
        Move(parent, name, 0);
      else
        Move(parent, name, ((NodeCollection) Nesting.PropertyGetter(parent)).Count);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="name">Initial <see cref="Name"/> property value.</param>
    internal Node(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Initialize();
      Name = name;
    }

    // Deserialization

    /// <inheritdoc/>
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      Initialize();
      if (IsLocked) {
        cachedPath = Path;
      }
    }
  }
}