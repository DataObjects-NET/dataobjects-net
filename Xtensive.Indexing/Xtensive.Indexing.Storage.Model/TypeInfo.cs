// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes the type of table column.
  /// </summary>
  [Serializable]
  public class TypeInfo
  {
    /// <summary>
    /// Gets the type of the data.
    /// </summary>
    public Type DataType
    {
      [DebuggerStepThrough]
      get; 
      [DebuggerStepThrough]
      private set;
    }

    /// <summary>
    /// Gets the collation.
    /// </summary>
    public string Collation
    {
      [DebuggerStepThrough]
      get;
      [DebuggerStepThrough]
      private set;
    }
    
    /// <summary>
    /// Gets the length.
    /// </summary>
    public int Length
    {
      [DebuggerStepThrough]
      get;
      [DebuggerStepThrough]
      private set;
    }

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
          Collation==typeInfo.Collation &&
            Length==typeInfo.Length;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return DataType.GetHashCode();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    public TypeInfo(Type dataType)
    {
      ArgumentValidator.EnsureArgumentNotNull(dataType, "dataType");

      DataType = dataType;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="collation">The collation.</param>
    /// <param name="length">The length.</param>
    public TypeInfo(Type dataType, string collation, int length)
      :this(dataType)
    {
      Collation = collation;
      Length = length;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected TypeInfo()
    {
    }
  }
}