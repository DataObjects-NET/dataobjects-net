// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Data;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Common;

namespace Xtensive.Sql.Dom
{
  public class SqlParameter : Parameter
  {


    // Constructors

    /// <summary>
    /// Initializes a new <see cref="Parameter"/> instance.
    /// </summary>
    public SqlParameter()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    public SqlParameter(string parameterName)
      : base(parameterName)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="Parameter"/> instance and
    /// specifies its <see cref="Parameter.ParameterName">Name</see> and
    /// <see cref="DbType"/>.
    /// </summary>
    /// <param name="parameterName">A name of the <see cref="Parameter"/>.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    public SqlParameter(string parameterName, DbType dbType)
      : base(parameterName, dbType)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="Parameter"/> instance and
    /// specifies its <see cref="Parameter.ParameterName">Name</see> and
    /// <see cref="Parameter.Value"/>.
    /// </summary>
    /// <param name="parameterName">A name of the <see cref="Parameter"/>.</param>
    /// <param name="value">An <see cref="object"/> that is the 
    /// <see cref="Parameter"/> value.</param>
    public SqlParameter(string parameterName, object value)
      :  base(parameterName, value)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="Parameter"/> instance and
    /// specifies its <see cref="Parameter.ParameterName">Name</see>,
    /// <see cref="DbType"/> and <see cref="Parameter.Size"/>.
    /// </summary>
    /// <param name="parameterName">A name of the <see cref="Parameter"/>.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    /// <param name="size">The size of the parameter value.</param>
    public SqlParameter(string parameterName, DbType dbType, int size)
      : base(parameterName, dbType, size)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="Parameter"/> instance and
    /// specifies its <see cref="Parameter.ParameterName">Name</see>,
    /// <see cref="DbType"/>, <see cref="Parameter.Size"/> and 
    /// <see cref="Parameter.SourceColumn">source column name</see>.
    /// </summary>
    /// <param name="parameterName">A name of the <see cref="Parameter"/>.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    /// <param name="size">The size of the parameter value.</param>
    /// <param name="sourceColumn">The name of the source column.</param>
    public SqlParameter(string parameterName, DbType dbType, int size, string sourceColumn)
      : base(parameterName, dbType, size, sourceColumn)
    {
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
    /// <param name="value">An <see cref="object"/> that is the value of the
    /// <see cref="Parameter"/>.</param>
    public SqlParameter(
      string parameterName, DbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision,
      byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
      : base(parameterName, dbType, size, direction, isNullable, precision, scale, sourceColumn, sourceVersion, value)
    {
    }
  }
}
