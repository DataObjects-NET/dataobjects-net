// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using System.Text;
using Xtensive.Core.Reflection;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Storage.Indexing.Model
{
  /// <summary>
  /// Type of table column.
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
    /// Gets the length.
    /// </summary>
    public int Length
    {
      [DebuggerStepThrough]
      get;
      [DebuggerStepThrough]
      private set;
    }
    
    /// <summary>
    /// Gets the scale.
    /// </summary>
    public int Scale
    {
      [DebuggerStepThrough]
      get;
      [DebuggerStepThrough]
      private set;
    }

    /// <summary>
    /// Gets the precision.
    /// </summary>
    public int Precision
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
          Length==typeInfo.Length &&
            Scale==typeInfo.Scale &&
              Precision==typeInfo.Precision;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return DataType.GetHashCode();
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append(string.Format("Type: {0}", DataType.GetShortName()));
      if (Length > 0)
        sb.Append(string.Format(", Length: {0}", Length));
      return sb.ToString();
    }


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
    public TypeInfo(Type dataType, int length)
      :this(dataType)
    {
      ArgumentValidator.EnsureArgumentIsInRange(length, 0, int.MaxValue, "length");

      Length = length;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="collation">The collation.</param>
    /// <param name="length">The length.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="precision">The precision.</param>
    public TypeInfo(Type dataType, int length, int scale, int precision)
      : this(dataType, length)
    {
      Scale = scale;
      Precision = precision;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected TypeInfo()
    {
    }
  }
}