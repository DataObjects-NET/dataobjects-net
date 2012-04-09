// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Describes a group of columns that belongs to the specified <see cref="TypeInfoRef"/>.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Type = {TypeInfoRef}, Keys = {Keys}, Columns = {Columns}")]
  public sealed class ColumnGroup
  {
    /// <summary>
    /// Gets the <see cref="Model.TypeInfoRef"/> pointing to <see cref="TypeInfo"/>
    /// this column group belongs to.
    /// </summary>
    public TypeInfoRef TypeInfoRef { get; private set; }

    /// <summary>
    /// Gets the indexes of key columns.
    /// </summary>
    public ReadOnlyList<int> Keys { get; private set; }

    /// <summary>
    /// Gets the indexes of all columns.
    /// </summary>
    public ReadOnlyList<int> Columns { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="keys">The keys.</param>
    /// <param name="columns">The columns.</param>
    public ColumnGroup(TypeInfoRef type, IEnumerable<int> keys, IEnumerable<int> columns)
      : this(type, new List<int>(keys), new List<int>(columns))
    {
    }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="keys">The keys.</param>
    /// <param name="columns">The columns.</param>
    public ColumnGroup(TypeInfoRef type, IList<int> keys, IList<int> columns)
    {
      TypeInfoRef = type;
      Keys = new ReadOnlyList<int>(keys);
      Columns = new ReadOnlyList<int>(columns);
    }
  }
}