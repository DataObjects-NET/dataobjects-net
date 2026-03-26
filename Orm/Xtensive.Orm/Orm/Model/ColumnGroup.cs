// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
  public readonly struct ColumnGroup
  {
    /// <summary>
    /// Gets the <see cref="Model.TypeInfoRef"/> pointing to <see cref="TypeInfo"/>
    /// this column group belongs to.
    /// </summary>
    public TypeInfoRef TypeInfoRef { get; }

    /// <summary>
    /// Gets the indexes of key columns.
    /// </summary>
    public IReadOnlyList<int> Keys { get; }

    /// <summary>
    /// Gets the indexes of all columns.
    /// </summary>
    public IReadOnlyList<int> Columns { get; }


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
    public ColumnGroup(TypeInfoRef type, IReadOnlyList<int> keys, IReadOnlyList<int> columns)
    {
      TypeInfoRef = type;
      Keys = keys;
      Columns = columns;
    }
  }
}
