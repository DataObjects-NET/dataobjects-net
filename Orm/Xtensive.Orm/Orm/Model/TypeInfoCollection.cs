// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A collection of <see cref="TypeInfo"/> objects.
  /// </summary>
  [Serializable]
  public sealed class TypeInfoCollection
    : NodeCollection<TypeInfo>,
      IFilterable<TypeAttributes, TypeInfo>
  {
    private readonly Dictionary<Type, TypeInfo> typeTable = new();
    private readonly Dictionary<string, TypeInfo> fullNameTable = new();

    private TypeIdRegistry typeIdRegistry;

    /// <summary>
    /// An indexer that provides access to collection items.
    /// </summary>
    /// <exception cref="ArgumentException">Item was not found.</exception>
    public TypeInfo this[Type key] => TryGetValue(key, out var result)
      ? result
      : throw new KeyNotFoundException(string.Format(Strings.TypeXIsNotRegistered, key.GetFullName()));

    /// <summary>
    /// An indexer that provides access to collection items by their <see cref="TypeInfo.TypeId"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Item was not found.</exception>
    public new TypeInfo this[int typeId] {
      get {
        if (TypeIdRegistry!=null)
          return TypeIdRegistry[typeId];
        foreach (var type in this)
          if (type.TypeId==typeId)
            return type;
        throw new KeyNotFoundException(string.Format(Strings.ExTypeIdXIsNotRegistered, typeId));
      }
    }

    /// <summary>
    /// Gets the structures that are contained in this collection.
    /// </summary>
    public IEnumerable<TypeInfo> Structures => Find(TypeAttributes.Structure);

    /// <summary>
    /// Gets the entities that are contained in this collection.
    /// </summary>
    public IEnumerable<TypeInfo> Entities => Find(TypeAttributes.Entity);

    /// <summary>
    /// Gets the interfaces that are contained in this collection.
    /// </summary>
    public IEnumerable<TypeInfo> Interfaces => Find(TypeAttributes.Interface);

    internal TypeIdRegistry TypeIdRegistry
    {
      get { return typeIdRegistry; }
      set
      {
        if (typeIdRegistry!=null)
          throw Exceptions.AlreadyInitialized("TypeIdRegistry");
        typeIdRegistry = value;
      }
    }

    /// <summary>
    /// Determines whether this instance contains an item with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if this instance contains the specified key; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Type key)
    {
      return typeTable.ContainsKey(key);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value"><see cref="TypeInfo"/> if it was found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if value is found by specified <paramref name="key"/>; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(Type key, out TypeInfo value)
    {
      return typeTable.TryGetValue(key, out value);
    }


    /// <inheritdoc/>
    public override void Add(TypeInfo item)
    {
      base.Add(item);
      typeTable.Add(item.UnderlyingType, item);
      fullNameTable.Add(item.UnderlyingType.FullName, item);
    }

    /// <inheritdoc/>
    public override void AddRange(IEnumerable<TypeInfo> items)
    {
      foreach (var item in items) {
        Add(item);
      }
    }

    /// <summary>
    /// Removes element from the the collection.
    /// </summary>
    /// <param name="value">Item to remove.</param>
    /// <exception cref="NotSupportedException">Always</exception>
    public override bool Remove(TypeInfo value)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Removes all objects from the
    /// collection instance.
    /// </summary>
    /// <exception cref="NotSupportedException">Always</exception>
    public override void Clear()
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Determines whether collection contains a specific item.
    /// </summary>
    /// <param name="item">Value to search for.</param>
    /// <returns>
    ///   <see langword="True"/> if the object is found; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Contains(TypeInfo item)
    {
      if (item==null)
        return false;
      TypeInfo result;
      if (!TryGetValue(item.UnderlyingType, out result))
        return false;
      return result==item;
    }

    #region FindXxx methods

    /// <summary>
    /// Finds the type by its full name.
    /// </summary>
    /// <param name="fullName">The full name of the type to find.</param>
    /// <returns>Found type, if any;
    /// <see langword="null" />, if there is no type with specified full name.</returns>
    public TypeInfo Find(string fullName)
    {
      ArgumentValidator.EnsureArgumentNotNull(fullName, "fullName");
      TypeInfo result;
      return fullNameTable.TryGetValue(fullName, out result) ? result : null;
    }

    /// <summary>
    /// Finds the ancestor of the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to search ancestor for.</param>
    /// <returns><see name="TypeDef"/> instance that is ancestor of specified <paramref name="type"/> or
    /// <see langword="null"/> if the ancestor is not found in this collection.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="type"/> is <see langword="null"/>.</exception>
    private TypeInfo FindAncestor(Type type)
    {
      if (type == WellKnownTypes.Object) {
        return null;
      }
      return type.BaseType switch {
        null => null,
        var baseType => TryGetValue(baseType, out var typeInfo) ? typeInfo : FindAncestor(baseType)
      };
    }

    #endregion

    #region IFilterable<TypeAttributes,TypeInfo> Members

    /// <summary>
    /// Finds all <see cref="TypeInfo"/> instances according to specified criteria.
    /// </summary>
    /// <param name="criteria">The attributes.</param>
    /// <returns><see cref="IEnumerable{TItem}"/> that contains all found instances.</returns>
    public IEnumerable<TypeInfo> Find(TypeAttributes criteria) => Find(criteria, MatchType.Partial);

    public IEnumerable<TypeInfo> Find(TypeAttributes criteria, MatchType matchType)
    {
      if (criteria == TypeAttributes.None)
        return Array.Empty<TypeInfo>();
      switch (matchType) {
        case MatchType.Partial:
          return this.Where(f => (f.Attributes & criteria) > 0);
        case MatchType.Full:
          return this.Where(f => (f.Attributes & criteria) == criteria);
        default:
          return this.Where(f => (f.Attributes & criteria) == 0);
      }
    }

    #endregion

    /// <summary>
    /// Registers the connection between ancestor &amp; descendant.
    /// </summary>
    /// <param name="ancestor">The ancestor.</param>
    /// <param name="descendant">The descendant.</param>
    public void RegisterInheritance(TypeInfo ancestor, TypeInfo descendant)
    {
      EnsureNotLocked();

      if (ancestor.IsInterface) {
        descendant.AddInterface(ancestor);
      }
      else {
        descendant.Ancestor = ancestor;
      }

      ancestor.AddDescendant(descendant);
    }

    /// <summary>
    /// Registers the connection between interface and implementor.
    /// </summary>
    /// <param name="interface">The interface.</param>
    /// <param name="implementor">The implementor.</param>
    public void RegisterImplementation(TypeInfo @interface, TypeInfo implementor)
    {
      EnsureNotLocked();

      implementor.AddInterface(@interface);
      @interface.AddImplementor(implementor);
    }

    protected override string GetExceptionMessage(string key)
    {
      return string.Format(Strings.ExItemWithKeyXWasNotFound
        + " You might have forgotten to register type {0} as an element of domain model.", key);
    }


    // Constructors

    /// <inheritdoc/>
    public TypeInfoCollection(Node owner, string name)
      : base(owner, name)
    {
    }
  }
}
