// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Attributes;

using Xtensive.Reflection;
using Xtensive.Modelling.Validation;
using System.Linq;

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
    public static readonly string PathDelimiterString = PathDelimiter.ToString();
    /// <summary>
    /// Path escape character.
    /// </summary>
    public static readonly char PathEscape = '\\';

    [NonSerialized]
    private static ConcurrentDictionary<Type, Lazy<PropertyAccessorDictionary>> cachedPropertyAccessors = 
      new ConcurrentDictionary<Type, Lazy<PropertyAccessorDictionary>>();
    [NonSerialized]
    private Node model;
    [NonSerialized]
    private string cachedPath;
    [NonSerialized]
    private Nesting nesting;
    [NonSerialized]
    private PropertyAccessorDictionary propertyAccessors;
    internal Node parent;
    private string name;
    private string escapedName;
    private NodeState state;
    private int index;

    #region Properties

    /// <inheritdoc/>
    [SystemProperty]
    public Node Parent {
      [DebuggerStepThrough]
      get { return parent; }
      [DebuggerStepThrough]
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        if (value==Parent)
          return;
        NodeCollection collection = null;
        if (Nesting.IsNestedToCollection)
          collection = (NodeCollection) Nesting.PropertyGetter(value);
        Move(value, Name, collection==null ? 0 : collection.Count);
      }
    }

    /// <inheritdoc/>
    public Node Model {
      [DebuggerStepThrough]
      get { return model; }
    }

    /// <inheritdoc/>
    [SystemProperty]
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
      get {
        if (escapedName==null)
          escapedName = new[] {Name}.RevertibleJoin(PathEscape, PathDelimiter);
        return escapedName;
      }
    }

    /// <inheritdoc/>
    public NodeState State {
      get { return state; }
    }

    /// <inheritdoc/>
    [SystemProperty]
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
    public PropertyAccessorDictionary PropertyAccessors {
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
    public IPathNode GetNestedProperty(string propertyName)
    {
      var getter = PropertyAccessors[propertyName].Getter;
      if (getter==null)
        return null;
      var value = getter.Invoke(this) as IPathNode;
      if (value==null)
        return null;
      if (value is NodeCollection)
        return value;
      var node = value as Node;
      if (node!=null && node.Parent==this)
        return value;
      return null;
    }

    /// <inheritdoc/>
    public IEnumerable<Pair<string, IPathNode>> GetPathNodes(bool nestedOnly)
    {
      foreach (var pair in propertyAccessors) {
        string propertyName = pair.Key;
        var accessor = pair.Value;
        if (accessor.PropertyInfo.GetAttribute<SystemPropertyAttribute>(
          AttributeSearchOptions.InheritNone)!=null)
          continue;
        IPathNode propertyValue;
        if (nestedOnly)
          propertyValue = GetNestedProperty(propertyName);
        else
          propertyValue = accessor.HasGetter ? GetProperty(propertyName) as IPathNode : null;
        if (propertyValue!=null)
          yield return new Pair<string, IPathNode>(propertyName, propertyValue);
      }
    }

    /// <inheritdoc/>
    public string Path {
      [DebuggerStepThrough]
      get {
        if (cachedPath!=null) {
          return cachedPath;
        }

        if (Parent==null) {
          return string.Empty;
        }

        var parentPath = Parent.Path;
        if (parentPath.Length!=0) {
          parentPath += PathDelimiterString;
        }

        return string.Concat(
          parentPath,
          Nesting.EscapedPropertyName,
          Nesting.IsNestedToCollection ? PathDelimiterString : null,
          Nesting.IsNestedToCollection ? EscapedName : null);
      }
    }

    #endregion

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid node state.</exception>
    public void Move(Node newParent, string newName, int newIndex)
    {
      if (State==NodeState.Removed) {
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
      }

      this.EnsureNotLocked();
      if (newParent == Parent && newName == Name && newIndex == Index) {
        return;
      }

      if (this is IUnnamedNode) {
        newName = newIndex.ToString();
      }

      ValidateMove(newParent, newName, newIndex);
      if (State==NodeState.Initializing) {
        parent = newParent;
        name = newName;
        escapedName = null;
        index = newIndex;
        UpdateModel();
        OnPropertyChanged("Parent");
        OnPropertyChanged("Model");
        OnPropertyChanged("Name");
        OnPropertyChanged("EscapedName");
        OnPropertyChanged("Index");
        using (var scope = LogAction()) {
          var a = new CreateNodeAction {
            Path = newParent == null ? string.Empty : newParent.Path,
            Type = GetType(),
            Name = newName
          };
          scope.Action = a;
          PerformCreate();
          scope.Commit();
        }
      }
      else {
        using (var scope = LogAction()) {
          var a = new MoveNodeAction {
            Path = Path,
            Parent = newParent == parent ? null : newParent.Path,
            Name = newName == name ? null : newName,
            Index = newIndex == index ? (int?) null : newIndex
          };
          scope.Action = a;
          PerformMove(newParent, newName, newIndex);
          a.NewPath = Path;
          scope.Commit();
        }
      }
      OnPropertyChanged("State");
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid node state.</exception>
    public void Remove()
    {
      EnsureIsEditable();
      ValidateRemove();
      using (var scope = LogAction(new RemoveNodeAction { Path = Path })) {
        PerformRemove(this);
        scope.Commit();
      }
      OnPropertyChanged("State");
    }

    /// <inheritdoc/>
    public IPathNode Resolve(string path)
    {
      if (path.IsNullOrEmpty()) {
        return this;
      }

      var parts = path.RevertibleSplitFirstAndTail(PathEscape, PathDelimiter);
      var accessor = PropertyAccessors.TryGetValue(parts.First, out var pAccessor) ? pAccessor : default;
      if (accessor == null) {
        return null;
      }

      var next = (IPathNode) accessor.Getter.Invoke(this);
      if (parts.Second == null) {
        return next;
      }

      return next.Resolve(parts.Second);
    }

    /// <inheritdoc/>
    public void Validate()
    {
      using (ValidationScope.Open()) {
        using (var ea = new ExceptionAggregator()) {
          if (ValidationContext.Current.IsValidated(this)) {
            ea.Complete();
            return;
          }
          ValidateState();
          foreach (var pair in PropertyAccessors) {
            if (!pair.Value.HasGetter) {
              continue;
            }

            var nested = GetNestedProperty(pair.Key);
            if (nested != null) {
              ea.Execute(x => x.Validate(), nested);
              continue;
            }

            var value = GetProperty(pair.Key);
            if (value is Node pathNode) {
              ea.Execute(x => x.ValidateState(), pathNode);
            }
          }
          ea.Complete();
        }
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Required constructor isn't found.</exception>
    public virtual Node Clone(Node newParent, string newName)
    {
      using (CloningScope.Open()) {
        var isModel = this is IModel;
        if (!isModel) {
          ArgumentValidator.EnsureArgumentNotNull(newParent, nameof(newParent));
        }

        ArgumentValidator.EnsureArgumentNotNull(newName, nameof(newName));
      
        // Cloning the instance
        var model = isModel ? null : (IModel) newParent.Model;
        Node node;
        if (isModel) {
          node = TryConstructor(null, newName);
        }
        else {
          node = TryConstructor(model, newParent, newName); // Regular node
          if (node == null) {
            node = TryConstructor(model, newParent); // Unnamed node
          }
        }
        if (node==null) {
          throw new InvalidOperationException(string.Format(
            Strings.ExCannotFindConstructorToExecuteX, this));
        }

        // Cloning properties
        foreach (var pair in PropertyAccessors) {
          var accessor = pair.Value;
          if (!accessor.HasGetter) {
            continue;
          }

          CopyPropertyValue(node, accessor);
        }

        return node;
      }
    }

    /// <summary>
    /// Copies the property value.
    /// </summary>
    /// <param name="target">The target node.</param>
    /// <param name="accessor">The accessor of the property to copy value of.</param>
    protected virtual void CopyPropertyValue(Node target, PropertyAccessor accessor)
    {
      var propertyName = accessor.PropertyInfo.Name;
      var nested = GetNestedProperty(propertyName);
      if (nested!=null) {
        if (nested is NodeCollection collection) {
          foreach (Node newNode in collection) {
            newNode.Clone(target, newNode.Name);
          }
        }
        else {
          var newNode = (Node) nested;
          newNode.Clone(target, newNode.Name);
        }
      }
      else if (accessor.HasSetter) {
        var value = GetProperty(propertyName);
        if (value is IPathNode pathNode) {
          CloningContext.Current.AddFixup(() => 
            accessor.Setter(target, 
              PathNodeReference.Resolve((IModel) target.Model, 
                new PathNodeReference(pathNode.Path))));
          return;
        }

        if (value is ICloneable cloneable) {
          value = cloneable.Clone();
        }

        accessor.Setter(target, value);
      }
    }

    #region ValidateXxx methods

    /// <summary>
    /// Validates the <see cref="Move"/> method arguments.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="newIndex">The new index.</param>
    /// <exception cref="ArgumentException">Item already exists.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="newIndex"/> is out of range, 
    /// or <paramref name="newParent"/> belongs to a different <see cref="Model"/>.</exception>
    /// <exception cref="InvalidOperationException">newName!=newIndex for <see cref="IUnnamedNode"/>.</exception>
    protected virtual void ValidateMove(Node newParent, string newName, int newIndex)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(newName, nameof(newName));
      if (this is IModel) {
        ArgumentValidator.EnsureArgumentIsInRange(newIndex, 0, 0, nameof(newIndex));
        return;
      }

      // Validating parent model
      ArgumentValidator.EnsureArgumentNotNull(newParent, nameof(newParent));
      ArgumentValidator.EnsureArgumentIs<Node>(newParent, nameof(newParent));
      var model = Model;
      if (model != null) {
        var newModel = newParent.Model;
        if (model != newModel) {
          throw new ArgumentOutOfRangeException("newParent.Model");
        }
      }

      if (this is IUnnamedNode) {
        // Validation for unnamed nodes
        if (newName != newIndex.ToString()) {
          throw Exceptions.InternalError("newName!=newIndex for IUnnamedNode!", CoreLog.Instance);
        }
      }
      else if (!Nesting.IsNestedToCollection) {
        // Validation parent property nesting
        ArgumentValidator.EnsureArgumentIsInRange(newIndex, 0, 0, nameof(newIndex));
      }
      else {
        // Validation parent collection nesting
        var collection = (NodeCollection) Nesting.PropertyGetter(newParent);
        ArgumentValidator.EnsureArgumentIsInRange(newIndex, 0, collection.Count - (newParent == Parent ? 1 : 0),
          nameof(newIndex));
        if (!collection.TryGetValue(newName, out var node)) {
          return;
        }

        if (node != this) {
          throw new ArgumentException(string.Format(
            Strings.ExItemWithNameXAlreadyExists, newName), newName);
        }
      }
    }

    /// <summary>
    /// Validates the <see cref="Remove"/> method call.
    /// </summary>
    /// <exception cref="InvalidOperationException">Model object cannot be removed.</exception>
    protected virtual void ValidateRemove()
    {
      if (this is IModel) {
        throw new InvalidOperationException(Strings.ExModelObjectCannotBeRemoved);
      }
    }

    /// <summary>
    /// Validates the state (i.e. checks everything except nested properties).
    /// </summary>
    protected virtual void ValidateState() => EnsureIsLive();

    #endregion

    #region PerformXxx methods

    /// <summary>
    /// Actually performs construction operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Target object already exists.</exception>
    protected virtual void PerformCreate()
    {
      if (Parent != null) {
        if (!Nesting.IsNestedToCollection) {
          if (Nesting.PropertyValue != null) {
            throw new InvalidOperationException(string.Format(
              Strings.ExTargetObjectExistsX, Nesting.PropertyValue));
          }

          Nesting.PropertyValue = this;
        }
        else {
          var collection = (NodeCollection) Nesting.PropertyValue;
          collection.Add(this);
        }
      }

      state = NodeState.Live;
    }

    /// <summary>
    /// Actually performs <see cref="Move"/> operation.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="newIndex">The new index.</param>
    /// <exception cref="InvalidOperationException">Target object already exists.</exception>
    protected virtual void PerformMove(Node newParent, string newName, int newIndex)
    {
      var nameIsChanging = this is IUnnamedNode || Name != newName;
      var propertyGetter = Nesting.PropertyGetter;
      if (newParent != parent) {
        // Parent is changed
        if (!Nesting.IsNestedToCollection) {
          Nesting.PropertySetter(parent, null);
          var existingNode = propertyGetter(newParent);
          if (existingNode != null) {
            throw new InvalidOperationException(string.Format(
              Strings.ExTargetObjectExistsX, existingNode));
          }

          Nesting.PropertySetter(newParent, this);
        }
        else {
          var oldCollection = (NodeCollection) propertyGetter(parent);
          var newCollection = (NodeCollection) propertyGetter(newParent);
          for (var i = index + 1; i < oldCollection.Count; i++) {
            oldCollection[i].EnsureIsEditable();
          }

          for (var i = newIndex; i < newCollection.Count; i++) {
            newCollection[i].EnsureIsEditable();
          }

          if (nameIsChanging) {
            oldCollection.RemoveName(this);
          }

          for (var i = index + 1; i < oldCollection.Count; i++) {
            oldCollection[i].PerformShift(-1);
          }

          for (var i = newCollection.Count - 1; i >= newIndex; i--) {
            newCollection[i].PerformShift(1);
          }

          oldCollection.Remove(this);
          index = newIndex;
          if (nameIsChanging) {
            name = newName;
            escapedName = null;
          }

          newCollection.Add(this);
        }
      }
      else if (propertyGetter == null) {
        if (!(this is IModel)) {
          throw Exceptions.InternalError(string.Format(
            Strings.ExInvalidNestingOfNodeX, this), CoreLog.Instance);
        }
      }
      else if (Nesting.IsNestedToCollection) {
        // Parent isn't changed
        var collection = (NodeCollection) propertyGetter(newParent);
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

        if (nameIsChanging) {
          collection.RemoveName(this);
        }

        for (var i = minIndex; i <= maxIndex; i++) {
          collection[i].EnsureIsEditable();
        }

        if (shift < 0) {
          for (var i = minIndex; i <= maxIndex; i++) {
            collection[i].PerformShift(shift);
          }
        }
        else {
          for (var i = maxIndex; i >= minIndex; i--) {
            collection[i].PerformShift(shift);
          }
        }

        collection.Move(this, newIndex);
        name = newName;
        escapedName = null;
        index = newIndex;
        if (nameIsChanging) {
          collection.AddName(this);
        }

        // collection.CheckIntegrity();
      }

      parent = newParent;
      name = newName;
      escapedName = null;
      index = newIndex;
      UpdateModel();
    }

    /// <summary>
    /// Performs "shift" operation 
    /// (induced by <see cref="Move"/> operation of another node).
    /// </summary>
    /// <param name="offset">Shift offset.</param>
    protected virtual void PerformShift(int offset)
    {
      var bUnnamed = this is IUnnamedNode;
      var newName = name;
      var newIndex = index + offset;
      if (bUnnamed) {
        newName = newIndex.ToString();
        if (Nesting.IsNestedToCollection) {
          var collection = (NodeCollection) Nesting.PropertyValue;
          if (collection != null) {
            collection.RemoveName(this);
            name = newName;
            escapedName = null;
            index = newIndex;
            collection.AddName(this);
          }
        }
      }
      else {
        index = newIndex;
      }
    }

    /// <summary>
    /// Actually performs <see cref="Remove"/> operation.
    /// </summary>
    protected virtual void PerformRemove(Node source)
    {
      state = NodeState.Removed;
      if (source == this) {
        // Updating parents
        if (!Nesting.IsNestedToCollection) {
          Nesting.PropertyValue = null;
        }
        else {
          var collection = (NodeCollection) Nesting.PropertyValue;
          collection.Remove(this);
          for (var i = index; i < collection.Count; i++) {
            collection[i].EnsureIsEditable();
          }

          for (var i = index; i < collection.Count; i++) {
            collection[i].PerformShift(-1);
          }
        }
      }

      // Notifying children
      foreach (var pair in GetPathNodes(true)) {
        var pathNode = pair.Second;
        if (pathNode is NodeCollection nodeCollection) {
          foreach (Node nestedNode in nodeCollection) {
            nestedNode.PerformRemove(source);
          }
        }

        if (pathNode is Node node) {
          node.PerformRemove(source);
        }
      }

      if (source == this) {
        Lock(true);
      }
    }

    #endregion

    #region LogXxx methods

    /// <summary>
    /// Logs the property change.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="propertyValue">The property value.</param>
    /// <returns></returns>
    protected ActionScope LogPropertyChange(string propertyName, object propertyValue)
    {
      var scope = LogAction();
      var action = new PropertyChangeAction {Path = Path};
      action.Properties.Add(propertyName, PathNodeReference.Get(propertyValue));
      scope.Action = action;
      return scope;
    }
    
    /// <summary>
    /// Begins registration of a new action.
    /// </summary>
    /// <param name="action">The action to register.</param>
    /// <returns>
    /// <see cref="ActionScope"/> object allowing to describe it.
    /// </returns>
    protected ActionScope LogAction(NodeAction action)
    {
      var scope = LogAction();
      scope.Action = action;
      return scope;
    }

    /// <summary>
    /// Begins registration of a new action.
    /// </summary>
    /// <returns>
    /// <see cref="ActionScope"/> object allowing to describe it.
    /// </returns>
    protected ActionScope LogAction()
    {
      var model = (IModel) Model;
      if (model == null) {
        return new ActionScope();
      }

      var actions = model.Actions;
      if (actions == null) {
        return new ActionScope();
      }

      return actions.LogAction();
    }

    #endregion

    #region EnsureXxxx methods

    /// <summary>
    /// Ensures the node <see cref="State"/> is <see cref="NodeState.Live"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="State"/> is invalid.</exception>
    protected void EnsureIsLive()
    {
      if (State != NodeState.Live) {
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
      }
    }

    /// <summary>
    /// Ensures the node <see cref="State"/> is <see cref="NodeState.Live"/> and
    /// node isn't <see cref="Lock"/>ed.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="State"/> is invalid.</exception>
    protected void EnsureIsEditable()
    {
      if (State != NodeState.Live) {
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
      }

      this.EnsureNotLocked();
    }

    #endregion

    #region INotifyPropertyChanged methods

    /// <inheritdoc/>
    [field : NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    protected virtual void OnPropertyChanged(string name)
    {
      if (PropertyChanged != null) {
        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
      }
    }

    /// <summary>
    /// Does all the dirty job to change the property of this node.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name">Name of the property.</param>
    /// <param name="value">New value of the property.</param>
    /// <param name="setter">Property setter delegate.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> belongs to a different <see cref="Model"/>.</exception>
    protected void ChangeProperty<T>(string name, T value, Action<Node,T> setter)
    {
      EnsureIsEditable();
      var pathNode = value as IPathNode;
      var model = Model;
      if (pathNode!=null && model!=null && pathNode.Model!=model) {
        throw new ArgumentOutOfRangeException(nameof(value), Strings.ExPropertyValueMustBelongToTheSameModel);
      }

      using (var scope = !PropertyAccessors.ContainsKey(name) ? null : LogPropertyChange(name, value)) {
        setter.Invoke(this, value);
        OnPropertyChanged(name);
        scope?.Commit();
      }
    }

    #endregion

    #region ILockable methods

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      foreach (var pair in PropertyAccessors) {
        var nested = GetNestedProperty(pair.Key);
        nested?.Lock();
      }
      cachedPath = Path;
    }

    #endregion

    #region Private \ internal methods

    private void UpdateModel()
    {
      var p = Parent;
      model = p == null ? (Node) (this as IModel) : p.Model;
    }

    private static PropertyAccessorDictionary GetPropertyAccessors(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, nameof(type));

      static Lazy<PropertyAccessorDictionary> PropertyAccessorExtractor(Type entityType)
      {
        return new Lazy<PropertyAccessorDictionary>(() => {
          var d = new Dictionary<string, PropertyAccessor>();
          if (entityType != WellKnownTypes.Object) {
            foreach (var pair in GetPropertyAccessors(entityType.BaseType)) {
              d.Add(pair.Key, pair.Value);
            }
          }

          const BindingFlags propertyBindingFlags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
          foreach (var p in entityType.GetProperties(propertyBindingFlags)) {
            if (p.GetAttribute<PropertyAttribute>(AttributeSearchOptions.InheritNone) != null) {
              d.Add(p.Name, new PropertyAccessor(p));
            }
          }

          return new PropertyAccessorDictionary(d);
        });
      }

      return cachedPropertyAccessors.GetOrAdd(type, PropertyAccessorExtractor).Value;
    }

    private Node TryConstructor(IModel model, params object[] args)
    {
      var argTypes = args.Select(a => a.GetType()).ToArray();
      var ci = GetType().GetConstructor(argTypes);
      return ci == null ? null : (Node) ci.Invoke(args);
    }

    #endregion

    #region To override

    /// <summary>
    /// Creates <see cref="Nesting"/> object describing how this node is nested.
    /// </summary>
    /// <returns>New <see cref="Nesting"/> object.</returns>
    protected abstract Nesting CreateNesting();

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="CreateNesting"/> has returned <see langword="null" />.</exception>
    protected virtual void Initialize()
    {
      nesting = CreateNesting();
      if (nesting == null) {
        throw new InvalidOperationException(Strings.ExNoNesting);
      }

      propertyAccessors = GetPropertyAccessors(GetType());
    }

    #endregion

    #region Dump, ToString

    /// <inheritdoc/>
    public virtual void Dump()
    {
      var prefix = string.Empty;
      if (Nesting.IsNestedToCollection && !(Nesting.PropertyValue is IUnorderedNodeCollection)) {
        prefix = $"{Index}: ";
      }

      CoreLog.Info("{0}{1} \"{2}\"", prefix, GetType().GetShortName(), this);
      using (Orm.Logging.IndentManager.IncreaseIndent()) {
        // Validation errors
        Exception error = null;
        try {
          ValidateState();
        }
        catch (Exception e) {
          error = e;
        }

        // Basic properties
        if (error != null) {
          CoreLog.Info("+Error = {0}", error);
        }

        if (State != NodeState.Live) {
          CoreLog.Info("+State = {0}", State);
        }

        // Everything else
        foreach (var pair in propertyAccessors) {
          var propertyName = pair.Key;
          var accessor = pair.Value;
          if (accessor.PropertyInfo.GetAttribute<SystemPropertyAttribute>(
            AttributeSearchOptions.InheritNone) != null) {
            continue;
          }

          var propertyValue = accessor.HasGetter ? GetProperty(propertyName) : null;
          if (Equals(propertyValue, accessor.Default)) {
            continue;
          }

          var propertyType =
            (propertyValue == null ? accessor.PropertyInfo.PropertyType : propertyValue.GetType())
            .GetShortName();
          var nested = GetNestedProperty(propertyName);
          if (nested != null) {
            if (nested is NodeCollection collection && collection.Count != 0) {
              CoreLog.Info("+{0} ({1}):", propertyName, collection.Count);
            }
            else {
              CoreLog.Info("+{0}:", propertyName);
            }

            using (Orm.Logging.IndentManager.IncreaseIndent()) {
              nested.Dump();
            }
          }
          else {
            CoreLog.Info("+{0} = {1} ({2})", propertyName, propertyValue, propertyType);
          }
        }
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      var m = Model;
      var fullName = Path;
      if (m != null) {
        fullName = string.Concat(m.EscapedName, PathDelimiterString, fullName);
      }

      if (!Nesting.IsNestedToCollection && !(this is IModel)) {
        fullName = string.Format(Strings.NodeInfoFormat, fullName, Name);
      }

      return fullName;
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="parent"><see cref="Parent"/> property value.</param>
    /// <param name="name">Initial <see cref="Name"/> property value.</param>
    protected Node(Node parent, string name)
    {
      if (!(this is IModel)) {
        ArgumentValidator.EnsureArgumentNotNull(parent, nameof(parent));
      }

      if (!(this is IUnnamedNode)) {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, nameof(name));
      }

      Initialize();
      if (!Nesting.IsNestedToCollection) {
        Move(parent, name, 0);
      }
      else {
        Move(parent, name, ((NodeCollection) Nesting.PropertyGetter(parent)).Count);
      }
    }

    // Deserialization

    /// <inheritdoc/>
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      if (nesting!=null) {
        return; // Protects from multiple calls
      }

      Initialize();
      if (Parent is IDeserializationCallback p) {
        p.OnDeserialization(sender);
      }

      UpdateModel();
      if (IsLocked) {
        cachedPath = Path;
      }
    }
  }
}