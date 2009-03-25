// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes the type of table column.
  /// </summary>
  [Serializable]
  public class TypeInfo
  {
    /// <summary>
    /// Gets or sets the type of the data.
    /// </summary>
    public Type DataType { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether a column allow <see langword="null"/> values.
    /// </summary>
    public bool AllowNulls { get; private set; }
    
    /// <summary>
    /// Gets or sets the collation.
    /// </summary>
    public string Collation { get; private set; }
    
    /// <summary>
    /// Gets or sets the length.
    /// </summary>
    public int Length { get; private set; }

    #region Equals, GetHashCode methods

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      var typeInfo = obj as TypeInfo;
      if (typeInfo==null)
        return false;

      return
        DataType.Equals(typeInfo.DataType) &&
          AllowNulls==typeInfo.AllowNulls &&
            Collation==typeInfo.Collation &&
              Length==typeInfo.Length;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return DataType != null ? DataType.GetHashCode() : base.GetHashCode();
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("DataType = {0}, AllowNulls = {1}", DataType, AllowNulls);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    public TypeInfo(Type dataType)
    {
      DataType = dataType;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="allowNulls">Does column allow null values.</param>
    public TypeInfo(Type dataType, bool allowNulls)
    {
      DataType = dataType;
      AllowNulls = allowNulls;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="allowNulls">Does column allow null values.</param>
    /// <param name="collation">The collation.</param>
    /// <param name="length">The length.</param>
    public TypeInfo(Type dataType, bool allowNulls, string collation, int length)
    {
      DataType = dataType;
      AllowNulls = allowNulls;
      Collation = collation;
      Length = length;
    }
  }
}