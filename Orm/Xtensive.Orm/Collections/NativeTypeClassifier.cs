// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.07.02

using System;
using System.Collections.Generic;

using Xtensive.Reflection;
using System.Linq;


namespace Xtensive.Collections
{
  /// <summary>
  /// Native type-based classifier.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [Serializable]
  public class NativeTypeClassifier<TItem> : ClassifiedCollection<Type, TItem>
  {
    /// <summary>
    /// Gets all the items of the specified class.
    /// </summary>
    /// <typeparam name="TClass">The class of items to get.</typeparam>
    /// <returns>
    /// A sequence of items of the specified class.
    /// </returns>
    public IEnumerable<TClass> GetItems<TClass>()
    {
      return GetItems(typeof(TClass)).Cast<TClass>();
    }

    public int GetItemCount<TClass>()
    {
      return GetItemCount(typeof(TClass));
    }

    private static Func<TItem, Type[]> GetClassifier(bool exactType)
    {
      if (exactType)
        return item => new[] {item.GetType()};
      else
        return item => item.GetType().GetCompatibles();
    }

  
    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="exactType">If set to <see langword="true"/>, exact item type is used as classifier;
    /// otherwise all its base types and interfaces are used.</param>
    public NativeTypeClassifier(bool exactType)
      : this(exactType, false)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="exactType">If set to <see langword="true"/>, exact item type is used as classifier;
    /// otherwise all its base types and interfaces are used.</param>
    /// <param name="isSet">Indicates whether this instance behaves like a set.</param>
    public NativeTypeClassifier(bool exactType, bool isSet)
      : base(GetClassifier(exactType), isSet)
    {
    }
  }
}