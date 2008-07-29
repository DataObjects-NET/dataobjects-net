// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.21

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Column of the record.
  /// </summary>
  [Serializable]
  public sealed class RecordColumn : IEquatable<RecordColumn> 
  {
    /// <summary>
    /// Gets the reference that describes a column.
    /// </summary>    
    public ColumnInfoRef ColumnInfoRef { get; private set; }

    /// <summary>
    /// Gets the column name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the column index.
    /// </summary>
    /// <value>The index.</value>
    public int Index { get; private set; }

    /// <summary>
    /// Gets the column type.
    /// </summary>
    /// <value>The type.</value>
    public Type Type { get; private set; }

    /// <summary>
    /// Gets the kind of the column.
    /// </summary>    
    public ColumnKind ColumnKind { get; private set; }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(RecordColumn left, RecordColumn right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(RecordColumn left, RecordColumn right)
    {
      return !Equals(left, right);
    }

    /// <summary>
    /// Indicates whether the current <see cref="RecordColumn"/> is equal to another.
    /// </summary>
    /// <param name="recordColumn">The record column to compare with.</param>
    /// <returns><see langword="true" /> if instances are equal, otherwise <see langword="false" />.</returns>    
    public bool Equals(RecordColumn recordColumn)
    {
      if (recordColumn == null)
        return false;
      if (ColumnInfoRef == null) {
        if (!Equals(Name, recordColumn.Name))
          return false;
      }
      else
        if (!ColumnInfoRef.Equals(recordColumn.ColumnInfoRef))
          return false;

      if (!Equals(Type, recordColumn.Type))
        return false;

      return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as RecordColumn);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int result = ColumnInfoRef != null ? ColumnInfoRef.GetHashCode() : 0;
      if (result == 0)
        result = 29*result + Name.GetHashCode();
      result = 29*result + Type.GetHashCode();
      return result;
    }

    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">Initial <see cref="Name"/> value.</param>
    /// <param name="index">Initial <see cref="Index"/> value.</param>
    /// <param name="type">Initial <see cref="Type"/> value.</param>
    public RecordColumn(string name, int index, Type type)
      : this(name, index, type, ColumnKind.Unbound)
    {}

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">Initial <see cref="Name"/> value.</param>
    /// <param name="index">Initial <see cref="Index"/> value.</param>
    /// <param name="type">Initial <see cref="Type"/> value.</param>    
    /// <param name="columnKind">Initial <see cref="ColumnKind"/> value.</param>
    public RecordColumn(string name, int index, Type type, ColumnKind columnKind)
    {
      Name = name;
      Index = index;
      Type = type;
      ColumnKind = columnKind;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="columnInfoRef">Initial <see cref="ColumnInfoRef"/> value.</param>
    /// <param name="index">Initial <see cref="Index"/> value.</param>
    /// <param name="type">Initial <see cref="Type"/> value.</param>    
    /// <param name="columnKind">Initial <see cref="ColumnKind"/> value.</param>
    public RecordColumn(ColumnInfoRef columnInfoRef, int index, Type type, ColumnKind columnKind)
    {
      ColumnInfoRef = columnInfoRef;
      Name = columnInfoRef.ColumnName;
      Index = index;
      Type = type;
      ColumnKind = columnKind;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="recordColumn">The base <see cref="RecordColumn"/>.</param>
    /// <param name="alias">The alias to add to the name of new column.</param>
    public RecordColumn(RecordColumn recordColumn, string alias)
    {
      Name = string.Concat(alias, ".", recordColumn.Name);
      ColumnInfoRef = recordColumn.ColumnInfoRef;
      Type = recordColumn.Type;
      ColumnKind = recordColumn.ColumnKind;
      Index = recordColumn.Index;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="recordColumn">The base <see cref="RecordColumn"/>.</param>
    /// <param name="index">The index of new column.</param>
    public RecordColumn(RecordColumn recordColumn, int index)
    {
      ColumnInfoRef = recordColumn.ColumnInfoRef;
      Name = recordColumn.Name;
      Type = recordColumn.Type;
      ColumnKind = recordColumn.ColumnKind;
      Index = index;
    }
  }
}