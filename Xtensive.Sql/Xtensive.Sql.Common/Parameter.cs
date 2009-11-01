// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Common.Resources;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Represents a parameter to a <see cref="Command"/> and optionally,
  /// its mapping to a <see cref="DataSet"/> column. 
  /// </summary>
  public class Parameter
    : DbParameter,
      IDbDataParameter,
      IDataParameter,
      ICloneable
  {
    private const DbType cDefaultDbType = DbType.String;
    private DbType dbType = cDefaultDbType;
    private ParameterDirection direction = ParameterDirection.Input;
    private bool isNullable;
    private string parameterName = String.Empty;
    private int size;
    private string sourceColumn = String.Empty;
    private bool sourceColumnNullMapping;
    private DataRowVersion sourceVersion = DataRowVersion.Current;
    private object value;
    private byte precision;
    private byte scale;
    private bool forcedDbType;

    /// <summary>
    /// Resets the <see cref="DbType"/> property to its original settings.
    /// </summary>
    public override void ResetDbType()
    {
      dbType = DetectDbType(value);
      forcedDbType = false;
    }

    /// <summary>
    /// Gets or sets the <see cref="T:System.Data.DbType"/> of the <see cref="Parameter"/>.
    /// </summary>
    /// <value>
    /// <para>One of the <see cref="T:System.Data.DbType"/> values.</para>
    /// <para>The default value is <see cref="F:System.Data.DbType.String"/>.</para>
    /// </value>
    /// <exception cref="System.ArgumentException">
    /// The property is not set to a valid <see cref="T:System.Data.DbType"/>.</exception>
    public override DbType DbType
    {
      get { return dbType; }
      set { ForceDbType(value); }
    }

    private void ForceDbType(DbType dbType)
    {
        this.dbType = dbType;
        forcedDbType = true;
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the parameter is input-only,
    /// output-only, bidirectional, or a stored procedure return value parameter.
    /// </summary>
    /// <value>
    /// <para>One of the <see cref="ParameterDirection"/> values.</para>
    /// <para>The default is <see cref="ParameterDirection.Input"/>.</para>
    /// </value>
    /// <exception cref="T:System.ArgumentException">
    /// The property is not set to one of the valid 
    /// <see cref="ParameterDirection"/> values.</exception>
    public override ParameterDirection Direction
    {
      get { return direction; }
      set { direction = value; }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="Parameter"/>
    /// accepts <see langword="null"/> values.
    /// </summary>
    /// <value>
    /// <para><see langword="true"/> if null values are accepted;
    /// otherwise <see langword="false"/>.</para>
    /// <para>The default is <see langword="false"/>.</para>
    /// </value>
    public override bool IsNullable
    {
      get { return isNullable; }
      set { isNullable = value; }
    }

    /// <summary>
    /// Gets or sets the name of the <see cref="Parameter"/>.
    /// </summary>
    /// <value>
    /// <para>The name of the <see cref="Parameter"/>.</para>
    /// <para>The default is an empty string ("").</para>
    /// </value>
    public override string ParameterName
    {
      get { return parameterName; }
      set { parameterName = value; }
    }

    /// <summary>
    /// Gets or sets the maximum size, in bytes, of the data within the column.
    /// </summary>
    /// <value>
    /// <para>The maximum size, in bytes, of the data within the column.</para>
    /// <para>The default value is inferred from the parameter value.</para>
    /// </value>
    public override int Size
    {
      get { return size; }
      set { size = value; }
    }

    /// <summary>
    /// Gets or sets the name of the source column mapped to the <see cref="DataSet"/>
    /// and used for loading or returning the <see cref="Parameter.Value"/>.
    /// </summary>
    /// <value>
    /// <para>The name of the source column mapped to the <see cref="DataSet"/>.</para>
    /// <para>The default is an empty string.</para>
    /// </value>
    public override string SourceColumn
    {
      get { return sourceColumn; }
      set { sourceColumn = value; }
    }

    /// <summary>
    /// Gets or sets a value which indicates whether the source column is nullable.
    /// This allows <see cref="DbCommandBuilder"/> to correctly generate 
    /// Update statements for nullable columns.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the source column is nullable; othewise <see langword="false"/>.
    /// </value>
    public override bool SourceColumnNullMapping
    {
      get { return sourceColumnNullMapping; }
      set { sourceColumnNullMapping = value; }
    }

    /// <summary>
    /// Gets or sets the <see cref="DataRowVersion"/> to use when you load
    /// <see cref="Parameter.Value"/>.
    /// </summary>
    /// <value>
    /// <para>One of the <see cref="DataRowVersion"/> values.</para>
    /// <para>The default is <see cref="DataRowVersion.Current"/>.</para>
    /// </value>
    /// <exception cref="T:System.ArgumentException">
    /// The property is not set to one of the <see cref="DataRowVersion"/> values.</exception>
    public override DataRowVersion SourceVersion
    {
      get { return sourceVersion; }
      set { sourceVersion = value; }
    }

    /// <summary>
    /// Gets or sets the maximum number of digits used to represent the
    /// <see cref="Parameter.Value"/> property
    /// </summary>
    /// <value>
    /// <para>The maximum number of digits used to represent the
    /// <see cref="Parameter.Value"/> property</para>
    /// <para>The default value is 0. This indicates that the data provider sets
    /// the precision for <see cref="Parameter.Value"/>.</para>
    /// </value>
    public byte Precision
    {
      get { return precision; }
      set { precision = value; }
    }

    /// <summary>
    /// Gets or sets the number of decimal places to which <see cref="Parameter.Value"/>
    /// is resolved.
    /// </summary>
    /// <value>
    /// <para>The number of decimal places to which 
    /// <see cref="Parameter.Value"/> is resolved.</para>
    /// <para>The default is 0.</para>
    /// </value>
    public byte Scale
    {
      get { return scale; }
      set { scale = value; }
    }

    /// <summary>
    /// Gets or sets the value of the parameter.
    /// </summary>
    /// <value>
    /// <para>An <see cref="T:System.Object"/> that is the value of the parameter.</para>
    /// <para>The default value is <see langword="null"/>.</para>
    /// </value>
    public override object Value
    {
      get { return value; }
      set
      {
        this.value = value;
        if (!forcedDbType)
          dbType = DetectDbType(value);
        size = DetectSize(value);
      }
    }

    /// <summary>
    /// Creates a new <see cref="Parameter"/> that is a copy of the current one.
    /// </summary>
    /// <returns>
    /// A new object that is a copy of this instance.
    /// </returns>
    public object Clone()
    {
      return new Parameter(this);
    }

    private void CloneHelper(Parameter target)
    {
      target.value = value;
      target.direction = direction;
      target.size = size;
      target.sourceColumn = sourceColumn;
      target.sourceVersion = sourceVersion;
      target.sourceColumnNullMapping = sourceColumnNullMapping;
      target.isNullable = isNullable;
      target.parameterName = parameterName;
      target.precision = precision;
      target.scale = scale;
      target.dbType = dbType;
      target.forcedDbType = forcedDbType;
    }

    private static DbType DetectDbType(Object value)
    {
      if (value==null)
        return cDefaultDbType;

      Type type = value.GetType();
      switch (Type.GetTypeCode(type)) {
        case TypeCode.Empty:
        case TypeCode.DBNull:
        case TypeCode.SByte:
          throw new ArgumentOutOfRangeException(Strings.ExInvalidDataType);

        case TypeCode.Char:
          return DbType.Int16;
        case TypeCode.Boolean:
          return DbType.Boolean;
        case TypeCode.Byte:
          return DbType.Byte;
        case TypeCode.DateTime:
          return DbType.DateTime;
        case TypeCode.Decimal:
          return DbType.Decimal;
        case TypeCode.Double:
          return DbType.Double;
        case TypeCode.Int16:
          return DbType.Int16;
        case TypeCode.Int32:
          return DbType.Int32;
        case TypeCode.Int64:
          return DbType.Int64;
        case TypeCode.Object:
          if (type == typeof(byte[]))
            return DbType.Binary;
          else if (type == typeof(Guid))
            return DbType.Guid;
          else if (type == typeof(TimeSpan))
            return DbType.DateTime;
          return DbType.Object;
        case TypeCode.Single:
          return DbType.Single;
        case TypeCode.String:
          return DbType.String;
        case TypeCode.UInt16:
          return DbType.UInt16;
        case TypeCode.UInt32:
          return DbType.UInt32;
        case TypeCode.UInt64:
          return DbType.UInt64;
        default:
          throw new ArgumentOutOfRangeException(Strings.ExValueIsOfUnknownDataType);
      }
    }

    private static int DetectSize(object value)
    {
      if (!IsNull(value)) {
        string text = value as string;
        if (text!=null)
          return text.Length;
        byte[] buffer = value as byte[];
        if (buffer!=null)
          return buffer.Length;
        char[] chArray = value as char[];
        if (chArray!=null)
          return chArray.Length;
        if ((value is byte) || (value is char))
          return 1;
      }
      else 
        return 0;
      return 1;
    }

    private static bool IsNull(object value)
    {
      if (value==null || DBNull.Value==value)
        return true;
      INullable nullable = value as INullable;
      return nullable!=null ? nullable.IsNull : false;
    }


    // Constructors

    /// <summary>
    /// Initializes a new <see cref="Parameter"/> instance.
    /// </summary>
    public Parameter()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    public Parameter(string parameterName)
    {
      this.parameterName = parameterName;
    }

    /// <summary>
    /// Initializes a new <see cref="Parameter"/> instance and
    /// specifies its <see cref="ParameterName">Name</see> and
    /// <see cref="DbType"/>.
    /// </summary>
    /// <param name="parameterName">A name of the <see cref="Parameter"/>.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    public Parameter(string parameterName, DbType dbType)
    {
      this.parameterName = parameterName;
      ForceDbType(dbType);
    }

    /// <summary>
    /// Initializes a new <see cref="Parameter"/> instance and
    /// specifies its <see cref="ParameterName">Name</see> and
    /// <see cref="Value"/>.
    /// </summary>
    /// <param name="parameterName">A name of the <see cref="Parameter"/>.</param>
    /// <param name="value">An <see cref="Object"/> that is the 
    /// <see cref="Parameter"/> value.</param>
    public Parameter(string parameterName, object value)
    {
      this.parameterName = parameterName;
      this.value = value;
      dbType = DetectDbType(value);
    }

    /// <summary>
    /// Initializes a new <see cref="Parameter"/> instance and
    /// specifies its <see cref="ParameterName">Name</see>,
    /// <see cref="DbType"/> and <see cref="Size"/>.
    /// </summary>
    /// <param name="parameterName">A name of the <see cref="Parameter"/>.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    /// <param name="size">The size of the parameter value.</param>
    public Parameter(string parameterName, DbType dbType, int size)
    {
      this.parameterName = parameterName;
      ForceDbType(dbType);
      this.size = size;
    }

    /// <summary>
    /// Initializes a new <see cref="Parameter"/> instance and
    /// specifies its <see cref="ParameterName">Name</see>,
    /// <see cref="DbType"/>, <see cref="Size"/> and 
    /// <see cref="SourceColumn">source column name</see>.
    /// </summary>
    /// <param name="parameterName">A name of the <see cref="Parameter"/>.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    /// <param name="size">The size of the parameter value.</param>
    /// <param name="sourceColumn">The name of the source column.</param>
    public Parameter(string parameterName, DbType dbType, int size, string sourceColumn)
    {
      this.parameterName = parameterName;
      ForceDbType(dbType);
      this.size = size;
      this.sourceColumn = sourceColumn;
    }

    /// <summary>
    /// Initializes a new <see cref="Parameter"/> instance and
    /// specifies all its properties.
    /// </summary>
    /// <param name="parameterName">A name of the <see cref="Parameter"/>.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    /// <param name="size">The size of the parameter value.</param>
    /// <param name="direction">One of the <see cref="ParameterDirection"/> values.</param>
    /// <param name="isNullable"><see langword="true"/> if the <see cref="Parameter"/> value of the field 
    /// can be <see langword="null"/>;  otherwise <see langword="false"/>.</param>
    /// <param name="precision">The total number of digits to the left and right
    /// of the decimal point to which <see cref="Parameter.Value"/>is resolved.</param>
    /// <param name="scale">The total number of decimal places to which 
    /// <see cref="Parameter.Value"/> is resolved. </param>
    /// <param name="sourceColumn">The name of the source column.</param>
    /// <param name="sourceVersion">One of the <see cref="DataRowVersion"/> values.</param>
    /// <param name="value">An <see cref="Object"/> that is the value of the
    /// <see cref="Parameter"/>.</param>
    public Parameter(
      string parameterName, DbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision,
      byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
    {
      this.parameterName = parameterName;
      ForceDbType(dbType);
      this.size = size;
      this.direction = direction;
      this.isNullable = isNullable;
      this.precision = precision;
      this.scale = scale;
      this.sourceColumn = sourceColumn;
      this.sourceVersion = sourceVersion;
      this.value = value;
    }

    private Parameter(Parameter source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      source.CloneHelper(this);
      ICloneable cloneable = value as ICloneable;
      if (cloneable!=null) {
        value = cloneable.Clone();
      }
    }
  }
}