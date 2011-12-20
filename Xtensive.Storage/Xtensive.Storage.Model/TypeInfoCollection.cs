// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Model.Resources;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// A collection of <see cref="TypeInfo"/> objects.
  /// </summary>
  [Serializable]
  public sealed class TypeInfoCollection
    : NodeCollection<TypeInfo>,
      IFilterable<TypeAttributes, TypeInfo>
  {
    private readonly Type baseType = typeof (object);
    private readonly Dictionary<Type, TypeInfo> typeTable = new Dictionary<Type, TypeInfo>();
    private readonly Dictionary<string, TypeInfo> fullNameTable = new Dictionary<string, TypeInfo>();
    private readonly Dictionary<TypeInfo, TypeInfo> ancestorTable = new Dictionary<TypeInfo, TypeInfo>();
    private readonly Dictionary<TypeInfo, HashSet<TypeInfo>> descendantTable = new Dictionary<TypeInfo, HashSet<TypeInfo>>();
    private readonly Dictionary<TypeInfo, HashSet<TypeInfo>> interfaceTable = new Dictionary<TypeInfo, HashSet<TypeInfo>>();
    private readonly Dictionary<TypeInfo, HashSet<TypeInfo>> implementorTable = new Dictionary<TypeInfo, HashSet<TypeInfo>>();
    private IntDictionary<TypeInfo> typeIdIndex;
    
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

    /// <summary>
    /// An indexer that provides access to collection items.
    /// </summary>
    /// <exception cref="ArgumentException">Item was not found.</exception>
    public TypeInfo this[Type key] {
      get {
        TypeInfo result;
        if (!TryGetValue(key, out result))
          throw new ArgumentException(  
            String.Format(Strings.TypeXIsNotRegistered, key.GetShortName()));
        return result;
      }
    }

    /// <summary>
    /// An indexer that provides access to collection items by their <see cref="TypeInfo.TypeId"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Item was not found.</exception>
    public new TypeInfo this[int typeId] {
      get {
        TypeInfo result = null;
        if (typeIdIndex!=null)
          typeIdIndex.TryGetValue(typeId, out result);
        else {
          foreach (var type in this) {
            if (type.TypeId==typeId)
              result = type;
          }
        }
        if (result==null)
          throw new ArgumentException(  
            String.Format(Strings.TypeIdXIsNotRegistered, typeId));
        return result;
      }
    }

    /// <summary>
    /// Gets the structures that are contained in this collection.
    /// </summary>
    public ICountable<TypeInfo> Structures
    {
      get { return Find(TypeAttributes.Structure); }
    }

    /// <summary>
    /// Gets the entities that are contained in this collection.
    /// </summary>
    public ICountable<TypeInfo> Entities
    {
      get { return Find(TypeAttributes.Entity); }
    }

    /// <summary>
    /// Gets the interfaces that are contained in this collection.
    /// </summary>
    public ICountable<TypeInfo> Interfaces
    {
      get { return Find(TypeAttributes.Interface); }
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
    /// Removes the element at the specified index of the
    /// collection instance.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="NotSupportedException">Always</exception>
    public override void RemoveAt(int index)
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
    /// Finds the ancestor of the specified <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The type to search ancestor for.</param>
    /// <returns><see cref="TypeInfo"/> instance that is ancestor of specified <paramref name="item"/> or 
    /// <see langword="null"/> if the ancestor is not found in this collection.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public TypeInfo FindAncestor(TypeInfo item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      TypeInfo result;
      return ancestorTable.TryGetValue(item, out result) ? result : null;
    }

    /// <summary>
    /// Finds the set of direct descendants of the specified <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The type to search descendants for.</param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="TypeInfo"/> instance that are descendants of specified <paramref name="item"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public IEnumerable<TypeInfo> FindDescendants(TypeInfo item)
    {
      return FindDescendants(item, false);
    }

    /// <summary>
    /// Finds the set of descendants of the specified <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The type to search descendants for.</param>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and nested descendants will be returned.</param>
    /// <returns>
    ///   <see cref="IEnumerable{T}"/> of <see cref="TypeInfo"/> instance that are descendants of specified <paramref name="item"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public IEnumerable<TypeInfo> FindDescendants(TypeInfo item, bool recursive)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      HashSet<TypeInfo> result;
      if (!descendantTable.TryGetValue(item, out result))
        result = new HashSet<TypeInfo>();

      foreach (var item1 in result) {
        yield return item1;
        if (recursive)
          foreach (var item2 in FindDescendants(item1, true))
            yield return item2;
      }
    }

    /// <summary>
    /// Find the <see cref="IList{T}"/> of interfaces that specified <paramref name="item"/> implements.
    /// </summary>
    /// <param name="item">The type to search interfaces for.</param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="TypeInfo"/> instance that are implemented by specified <paramref name="item"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public IEnumerable<TypeInfo> FindInterfaces(TypeInfo item)
    {
      return FindInterfaces(item, false);
    }

    /// <summary>
    /// Find the <see cref="IList{T}"/> of interfaces that specified <paramref name="item"/> implements.
    /// </summary>
    /// <param name="item">The type to search interfaces for.</param>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and non-direct implemented interfaces will be returned.</param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="TypeInfo"/> instance that are implemented by specified <paramref name="item"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public IEnumerable<TypeInfo> FindInterfaces(TypeInfo item, bool recursive)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      HashSet<TypeInfo> result;
      if (!interfaceTable.TryGetValue(item, out result))
        result = new HashSet<TypeInfo>();

      foreach (var item1 in result)
        yield return item1;

      if (!recursive || item.IsInterface)
        yield break;

      var ancestor = FindAncestor(item);
      while (ancestor != null) {
        foreach (var @interface in FindInterfaces(ancestor))
          yield return @interface;
        ancestor = FindAncestor(ancestor);
      }
    }

    /// <summary>
    /// Finds the set of direct implementors of the specified <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The type to search implementors for.</param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="TypeInfo"/> instance that are implementors of specified <paramref name="item"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public IEnumerable<TypeInfo> FindImplementors(TypeInfo item)
    {
      return FindImplementors(item, false);
    }

    /// <summary>
    /// Finds the set of implementors of the specified <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The type to search implementors for.</param>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and nested implementors will be returned.</param>
    /// <returns>
    ///   <see cref="IEnumerable{T}"/> of <see cref="TypeInfo"/> instance that are implementors of specified <paramref name="item"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public IEnumerable<TypeInfo> FindImplementors(TypeInfo item, bool recursive)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      HashSet<TypeInfo> result;
      if (!implementorTable.TryGetValue(item, out result))
        result = new HashSet<TypeInfo>();

      foreach (var item1 in result) {
        yield return item1;
        if (recursive && !item1.IsInterface)
          foreach (var item2 in FindDescendants(item1, true))
            yield return item2;
      }
    }

    /// <summary>
    /// Finds the root of the specified <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The type to search root for.</param>
    /// <returns><see cref="TypeInfo"/> instance that is root of specified <paramref name="item"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public TypeInfo FindRoot(TypeInfo item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      if (item.IsInterface || item.IsStructure)
        return null;

      var candidate = item;
      while (true) {
        var ancestor = FindAncestor(candidate);
        if (ancestor == null)
          return candidate;
        candidate = ancestor;
      }
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
      if (type == baseType || type.BaseType == null)
        return null;
      return Contains(type.BaseType) ? this[type.BaseType] : FindAncestor(type.BaseType);
    }

    #endregion

    #region IFilterable<TypeAttributes,TypeInfo> Members

    /// <summary>
    /// Finds all <see cref="TypeInfo"/> instances according to specified criteria.
    /// </summary>
    /// <param name="criteria">The attributes.</param>
    /// <returns><see cref="ICountable{TItem}"/> that contains all found instances.</returns>
    public ICountable<TypeInfo> Find(TypeAttributes criteria)
    {
      // We don't have any instance that has attributes == TypeAttributes.None
      if (criteria == TypeAttributes.None)
        return new EmptyCountable<TypeInfo>();

      return Find(criteria, MatchType.Partial);
    }

    public ICountable<TypeInfo> Find(TypeAttributes criteria, MatchType matchType)
    {
      if (criteria == TypeAttributes.None)
        return new EmptyCountable<TypeInfo>();

      switch (matchType) {
      case MatchType.Partial:
        return new BufferedEnumerable<TypeInfo>(this.Where(f => (f.Attributes & criteria) > 0));
      case MatchType.Full:
        return new BufferedEnumerable<TypeInfo>(this.Where(f => (f.Attributes & criteria) == criteria));
      default:
        return new BufferedEnumerable<TypeInfo>(this.Where(f => (f.Attributes & criteria) == 0));
      }

    }

    #endregion

    /// <inheritdoc/>
    protected override void OnInserted(TypeInfo value, int index)
    {
      base.OnInserted(value, index);
      typeTable.Add(value.UnderlyingType, value);
      fullNameTable.Add(value.UnderlyingType.FullName, value);
    }

    /// <summary>
    /// Generates index allowing to quickly find 
    /// the type by its <see cref="TypeInfo.TypeId"/>.
    /// </summary>
    public void RebuildTypeIdIndex()
    {
      var index = new IntDictionary<TypeInfo>();
      foreach (var type in this)
        if (type.TypeId!=TypeInfo.NoTypeId)
          index[type.TypeId] = type;
      typeIdIndex = index;
    }

    /// <summary>
    /// Registers the connection between ancestor &amp; descendant.
    /// </summary>
    /// <param name="ancestor">The ancestor.</param>
    /// <param name="descendant">The descendant.</param>
    public void RegisterInheritance(TypeInfo ancestor, TypeInfo descendant)
    {
      this.EnsureNotLocked();

      if (ancestor.IsInterface) {
        HashSet<TypeInfo> interfaces;
        if (!interfaceTable.TryGetValue(descendant, out interfaces)) {
          interfaces = new HashSet<TypeInfo>();
          interfaceTable[descendant] = interfaces;
        }
        interfaces.Add(ancestor);
      }
      else
        ancestorTable[descendant] = ancestor;

      HashSet<TypeInfo> descendants;
      if (!descendantTable.TryGetValue(ancestor, out descendants)) {
        descendants = new HashSet<TypeInfo>();
        descendantTable[ancestor] = descendants;
      }
      descendants.Add(descendant);
    }

    /// <summary>
    /// Registers the connection between interface and implementor.
    /// </summary>
    /// <param name="interface">The interface.</param>
    /// <param name="implementor">The implementor.</param>
    public void RegisterImplementation(TypeInfo @interface, TypeInfo implementor)
    {
      this.EnsureNotLocked();

      HashSet<TypeInfo> interfaces;
      if (!interfaceTable.TryGetValue(implementor, out interfaces)) {
        interfaces = new HashSet<TypeInfo>();
        interfaceTable[implementor] = interfaces;
      }
      interfaces.Add(@interface);

      HashSet<TypeInfo> implementors;
      if (!implementorTable.TryGetValue(@interface, out implementors)) {
        implementors = new HashSet<TypeInfo>();
        implementorTable[@interface] = implementors;
      }
      implementors.Add(implementor);
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
