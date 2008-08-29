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
  public sealed class Column : IEquatable<Column> 
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
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(Column left, Column right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(Column left, Column right)
    {
      return !Equals(left, right);
    }

    /// <summary>
    /// Indicates whether the current <see cref="Column"/> is equal to another.
    /// </summary>
    /// <param name="column">The record column to compare with.</param>
    /// <returns><see langword="true" /> if instances are equal, otherwise <see langword="false" />.</returns>    
    public bool Equals(Column column)
    {
      if (column == null)
        return false;
      if (ColumnInfoRef == null) {
        if (!Equals(Name, column.Name))
          return false;
      }
      else
        if (!ColumnInfoRef.Equals(column.ColumnInfoRef))
          return false;

      if (!Equals(Type, column.Type))
        return false;

      return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as Column);
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
    public Column(string name, int index, Type type)
    {
      Name = name;
      Index = index;
      Type = type;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="columnInfoRef">Initial <see cref="ColumnInfoRef"/> value.</param>
    /// <param name="index">Initial <see cref="Index"/> value.</param>
    /// <param name="type">Initial <see cref="Type"/> value.</param>    
    public Column(ColumnInfoRef columnInfoRef, int index, Type type)
    {
      ColumnInfoRef = columnInfoRef;
      Name = columnInfoRef.ColumnName;
      Index = index;
      Type = type;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="column">The base <see cref="Column"/>.</param>
    /// <param name="alias">The alias to add to the name of new column.</param>
    public Column(Column column, string alias)
    {
      Name = string.Concat(alias, ".", column.Name);
      ColumnInfoRef = column.ColumnInfoRef;
      Type = column.Type;
      Index = column.Index;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="column">The base <see cref="Column"/>.</param>
    /// <param name="index">The index of new column.</param>
    public Column(Column column, int index)
    {
      ColumnInfoRef = column.ColumnInfoRef;
      Name = column.Name;
      Type = column.Type;
      Index = index;
    }
  }
}