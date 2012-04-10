// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;


namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Base class for any column of the <see cref="RecordSetHeader"/>.
  /// </summary>
  [Serializable]
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

    
    public bool Equals(Column other)
    {
      if (other==null)
        return false;
      return Name==other.Name;
    }

    
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as Column);
    }

    
    public override int GetHashCode()
    {
      return Name.GetHashCode();
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(Column left, Column right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(Column left, Column right)
    {
      return !Equals(left, right);
    }

    #endregion

    
    public override string ToString()
    {
      return string.Format(ToStringFormat, Type.Name, Name, Index);
    }

    /// <summary>
    /// Creates clone of the column, but with another <see cref="Index"/>.
    /// </summary>
    /// <param name="newIndex">The new index value.</param>
    /// <returns>Clone of the column, but with another <see cref="Index"/>.</returns>
    public abstract Column Clone(int newIndex);

    /// <summary>
    /// Creates clone of the column, but with another <see cref="Name"/>.
    /// </summary>
    /// <param name="newName">The new name value.</param>
    /// <returns>Clone of the column, but with another <see cref="Name"/>.</returns>
    public abstract Column Clone(string newName);


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class..
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