// Copyright (C) 2007 Xtensive LLC.
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
  /// Represents collection of <see cref="TypeInfo"/> instances and is indexed by <see cref="Type"/> and name.
  /// </summary>
  [Serializable]
  public sealed class TypeInfoCollection
    : NodeCollection<TypeInfo>,
      IFilterable<TypeAttributes, TypeInfo>
  {
    private readonly Type baseType = typeof (object);
    private readonly Dictionary<Type, TypeInfo> typeIndex = new Dictionary<Type, TypeInfo>();
    private readonly Dictionary<TypeInfo, TypeInfo> ancestors = new Dictionary<TypeInfo, TypeInfo>();
    private readonly Dictionary<TypeInfo, HashSet<TypeInfo>> descendants = new Dictionary<TypeInfo, HashSet<TypeInfo>>();
    private readonly Dictionary<TypeInfo, HashSet<TypeInfo>> interfaces = new Dictionary<TypeInfo, HashSet<TypeInfo>>();
    private readonly Dictionary<TypeInfo, HashSet<TypeInfo>> implementors = new Dictionary<TypeInfo, HashSet<TypeInfo>>();
    private Dictionary<int, TypeInfo> typeIdIndex;
    
    /// <summary>
    /// Determines whether this instance contains an item with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if this instance contains the specified key; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Type key)
    {
      return typeIndex.ContainsKey(key);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value"><see cref="TypeInfo"/> if it was found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if value is found by specified <paramref name="key"/>; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(Type key, out TypeInfo value)
    {
      return typeIndex.TryGetValue(key, out value);
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
      return ancestors.TryGetValue(item, out result) ? result : null;
    }

    /// <summary>
    /// Finds the set of direct descendants of the specified <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The type to search descendants for.</param>
    /// <returns><see cref="IList{T}"/> of <see cref="TypeInfo"/> instance that are descendants of specified <paramref name="item"/>.</returns>
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
    ///   <see cref="IList{T}"/> of <see cref="TypeInfo"/> instance that are descendants of specified <paramref name="item"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public IEnumerable<TypeInfo> FindDescendants(TypeInfo item, bool recursive)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var result = new HashSet<TypeInfo>(descendants[item]);
      if (!recursive)
        return result;
      foreach (TypeInfo descendant in descendants[item])
        result.UnionWith(FindDescendants(descendant, true));

      return result;
    }

    /// <summary>
    /// Find the <see cref="IList{T}"/> of interfaces that specified <paramref name="item"/> implements.
    /// </summary>
    /// <param name="item">The type to search interfaces for.</param>
    /// <returns><see cref="IList{T}"/> of <see cref="TypeInfo"/> instance that are implemented by specified <paramref name="item"/>.</returns>
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
    /// <returns><see cref="IList{T}"/> of <see cref="TypeInfo"/> instance that are implemented by specified <paramref name="item"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public IEnumerable<TypeInfo> FindInterfaces(TypeInfo item, bool recursive)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var result = new HashSet<TypeInfo>(interfaces[item]);
      if (!recursive)
        return result;
      foreach (TypeInfo @interface in interfaces[item])
        result.UnionWith(FindInterfaces(@interface, true));
      if (item.IsEntity) {
        TypeInfo ancestor = FindAncestor(item);
        while (ancestor != null) {
          foreach (TypeInfo @interface in FindInterfaces(ancestor, true))
            result.Add(@interface);
          ancestor = FindAncestor(ancestor);
        }
      }
      return result;
    }

    /// <summary>
    /// Finds the set of direct implementors of the specified <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The type to search implementors for.</param>
    /// <returns><see cref="IList{T}"/> of <see cref="TypeInfo"/> instance that are implementors of specified <paramref name="item"/>.</returns>
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
    ///   <see cref="IList{T}"/> of <see cref="TypeInfo"/> instance that are implementors of specified <paramref name="item"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public IEnumerable<TypeInfo> FindImplementors(TypeInfo item, bool recursive)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "it   em");
      var result = new HashSet<TypeInfo>(implementors[item]);
      if (recursive)
        foreach (TypeInfo implementor in implementors[item])
          result.UnionWith(FindDescendants(implementor, true));
      foreach (TypeInfo descendant in descendants[item])
        result.UnionWith(FindImplementors(descendant, recursive));
      return result;
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

      TypeInfo candidate = item;
      while (true) {
        TypeInfo ancestor = FindAncestor(candidate);
        if (ancestor == null)
          return candidate;
        candidate = ancestor;
      }
    }

    private void RegisterDescendant(TypeInfo descendant)
    {
      TypeInfo ancestor = FindAncestor(descendant.UnderlyingType);
      if (ancestor==null)
        return;
      ancestors[descendant] = ancestor;
      descendants[ancestor].Add(descendant);
    }

    private void RegisterInterface(TypeInfo @interface)
    {
      var all = new HashSet<TypeInfo>(FindInterfaces(@interface.UnderlyingType));
      var inherited = new HashSet<TypeInfo>(FindInterfaces(@interface, true));
      interfaces[@interface].UnionWith(all.Except(inherited));
    }

    public void RegisterBaseInterface(TypeInfo @base, TypeInfo @interface)
    {
      this.EnsureNotLocked();
      // Check all interfaces (direct and inherited)
      if (FindDescendants(@base, true).Contains(@interface))
        return;
      descendants[@base].Add(@interface);
//      interfaces[@interface].Add(@base);
    }

    public void RegisterImplementor(TypeInfo @interface, TypeInfo implementor)
    {
      this.EnsureNotLocked();
      // Check all interfaces (direct and inherited)
      if (FindInterfaces(implementor, true).Contains(@interface))
        return;
      interfaces[implementor].Add(@interface);
      implementors[@interface].Add(implementor);
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

    /// <summary>
    /// Find the <see cref="IList{T}"/> of interfaces that specified <paramref name="type"/> implements.
    /// </summary>
    /// <param name="type">The type to search interfaces for.</param>
    /// <returns><see cref="IList{T}"/> of <see name="TypeDef"/> instance that are implemented by the specified <paramref name="type"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="type"/> is <see langword="null"/>.</exception>
    private IEnumerable<TypeInfo> FindInterfaces(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      Type[] interfaces = type.GetInterfaces();
      for (int index = 0; index < interfaces.Length; index++) {
        TypeInfo result;
        if (TryGetValue(interfaces[index], out result))
          yield return result;
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
      typeIndex.Add(value.UnderlyingType, value);

      if (value.IsEntity)
      {
        descendants[value] = new HashSet<TypeInfo>();
        interfaces[value] = new HashSet<TypeInfo>();
        RegisterDescendant(value);
      }
      if (value.IsStructure)
      {
        descendants[value] = new HashSet<TypeInfo>();
        RegisterDescendant(value);
      }
      if (value.IsInterface)
      {
        descendants[value] = new HashSet<TypeInfo>();
        interfaces[value] = new HashSet<TypeInfo>();
        implementors[value] = new HashSet<TypeInfo>();
        RegisterInterface(value);
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      this.EnsureNotLocked();
      
      GenerateTypeIds();

      base.Lock(recursive);
    }

    private void GenerateTypeIds()
    {
      typeIdIndex = new Dictionary<int, TypeInfo>(Count);
      int typeId = TypeInfo.MinTypeId;
      foreach (TypeInfo type in this) {
        if (type.TypeId==TypeInfo.NoTypeId)
          type.TypeId = typeId++;
        typeIdIndex[type.TypeId] = type;
      }
    }
  }
}
