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
    public int Index { get; private set; }

    /// <summary>
    /// Gets the column type.
    /// </summary>
    public Type Type { get; private set; }

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
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

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    public static bool operator ==(Column left, Column right)
    {
      return Equals(left, right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    public static bool operator !=(Column left, Column right)
    {
      return !Equals(left, right);
    }

    #endregion


    // Constructors

    #region Basic constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name"><see cref="Name"/> property value.</param>
    /// <param name="index"><see cref="Index"/> property value.</param>
    /// <param name="type"><see cref="Type"/> property value.</param>    
    public Column(string name, int index, Type type)
      : this(null, name, index, type)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="columnInfoRef"><see cref="ColumnInfoRef"/> property value.</param>
    /// <param name="index"><see cref="Index"/> property value.</param>
    /// <param name="type"><see cref="Type"/> property value.</param>    
    public Column(ColumnInfoRef columnInfoRef, int index, Type type)
      : this(columnInfoRef, columnInfoRef.ColumnName, index, type)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="columnInfoRef"><see cref="ColumnInfoRef"/> property value.</param>
    /// <param name="name"><see cref="Name"/> property value.</param>
    /// <param name="index"><see cref="Index"/> property value.</param>
    /// <param name="type"><see cref="Type"/> property value.</param>    
    public Column(ColumnInfoRef columnInfoRef, string name, int index, Type type)
    {
      ColumnInfoRef = columnInfoRef;
      Name = name;
      Index = index;
      Type = type;
    }

    #endregion

    #region Origin-based constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="column">The original <see cref="Column"/>.</param>
    /// <param name="alias">The alias to add.</param>
    public Column(Column column, string alias)
    {
      ColumnInfoRef = column.ColumnInfoRef;
      Name = alias.IsNullOrEmpty() 
        ? column.Name 
        : string.Concat(alias, ".", column.Name);
      Type = column.Type;
      Index = column.Index;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="column">The original <see cref="Column"/>.</param>
    /// <param name="index"><see cref="Index"/> property value.</param>
    public Column(Column column, int index)
    {
      ColumnInfoRef = column.ColumnInfoRef;
      Name = column.Name;
      Type = column.Type;
      Index = index;
    }

    #endregion
  }
}