// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.07.02

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Collections
{
  /// <summary>
  /// Type-based classifier.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [Serializable]
  public class TypeClassifier<TItem> : ClassifiedCollection<Type, TItem>
  {
    /// <summary>
    /// Gets all the items of the specified class.
    /// </summary>
    /// <typeparam name="TClass">The class of items to get.</typeparam>
    /// <returns>
    /// A sequence of items of the specified class.
    /// </returns>
    public IEnumerable<TItem> GetItems<TClass>()
    {
      return GetItems(typeof (TClass));
    }

    // Constructors

    /// <inheritdoc/>
    public TypeClassifier(Func<TItem, Type[]> classifier)
      : base(classifier)
    {
    }

    /// <inheritdoc/>
    public TypeClassifier(Func<TItem, Type[]> classifier, bool isSet)
      : base(classifier, isSet)
    {
    }
  }
}