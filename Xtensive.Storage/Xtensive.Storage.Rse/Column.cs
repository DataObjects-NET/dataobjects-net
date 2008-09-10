// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Column of the record.
  /// </summary>
  public abstract class Column : IEquatable<Column>
  {
    private const string ToStringFormat = "{0} {1} ({2})";

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
      if (column==null)
        return false;
      if (!Equals(Name, column.Name))
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
      return Name.GetHashCode();
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

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, Type.Name, Name, Index);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
    /// </summary>
    /// <param name="name"><see cref="Name"/> property value.</param>
    /// <param name="index"><see cref="Index"/> property value.</param>
    /// <param name="type"><see cref="Type"/> property value.</param>
    protected Column(string name, int index, Type type)
    {
      Name = name;
      Index = index;
      Type = type;
    }
  }
}