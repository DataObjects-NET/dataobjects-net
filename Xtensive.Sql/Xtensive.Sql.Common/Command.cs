// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Represents some statement to execute against a data source.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="Command"/> wraps actual <see cref="DbCommand">command</see>
  /// (e.g. <see cref="System.Data.SqlClient.SqlCommand"/>)
  /// with the aim to prepare it to be executed at the currently connected RDBMS server.
  /// </para>
  /// <para>
  /// This class provides only base functionality related to <see cref="Connection"/>
  /// interaction and <see cref="Parameter"/>s conversion. If you need to do 
  /// something special you have to inherit from it.
  /// </para>
  /// </remarks>
  /// <threadsafety static="true" instance="false"/>
  /// <seealso cref="Connection"/>
  /// <seealso cref="Parameter"/>
  public class Command: DbCommand
  {
    private DbCommand realCommand;
    private Connection connection;
    private ParameterCollection<Parameter> parameters = new ParameterCollection<Parameter>();

    /// <summary>
    /// Gets real command that will perform execution.
    /// </summary>
    public DbCommand RealCommand
    {
      get { return realCommand; }
    }

    /// <summary>
    /// Creates and adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name and the data type.
    /// </summary>
    /// <returns>
    /// The <see cref="Parameter"/> object that added to the collection.
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    public Parameter CreateParameter(string parameterName, DbType dbType)
    {
      return parameters.Add(parameterName, dbType);
    }

    /// <summary>
    /// Creates and adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name and the value.
    /// </summary>
    /// <returns>
    /// The <see cref="Parameter"/> object that added to the collection.
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="value">A value of the parameter.</param>
    public Parameter CreateParameter(string parameterName, object value)
    {
      return parameters.Add(parameterName, value);
    }

    /// <summary>
    /// Creates and adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name, the data type and the column length.
    /// </summary>
    /// <returns>
    /// The <see cref="Parameter"/> object that added to the collection.
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    /// <param name="size">A column length.</param>
    public Parameter CreateParameter(string parameterName, DbType dbType, int size)
    {
      return parameters.Add(parameterName, dbType, size);
    }

    /// <summary>
    /// Creates and adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name, the data type, the column length and the source
    /// column name.
    /// </summary>
    /// <returns>
    /// The <see cref="Parameter"/> object that added to the collection.
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    /// <param name="size">A column length.</param>
    /// <param name="sourceColumn">A source column name.</param>
    public Parameter CreateParameter(string parameterName, DbType dbType, int size, string sourceColumn)
    {
      return parameters.Add(parameterName, dbType, size, sourceColumn);
    }

    /// <inheritdoc/>
    public override void Cancel()
    {
      realCommand.Cancel();
    }

    /// <inheritdoc/>
    protected override DbParameter CreateDbParameter()
    {
      return new Parameter();
    }

    /// <inheritdoc/>
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      return realCommand.ExecuteReader(behavior);
    }

    /// <inheritdoc/>
    public override int ExecuteNonQuery()
    {
      return realCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public override object ExecuteScalar()
    {
      return realCommand.ExecuteScalar();
    }

    /// <inheritdoc/>
    public override void Prepare()
    {
      realCommand.Parameters.Clear();
      if (parameters.Count <= 0)
        return;
      foreach (Parameter parameter in parameters)
        realCommand.Parameters.Add(CreateDbParameter(parameter));
      realCommand.Prepare();
    }

    /// <summary>
    /// Creates <see cref="DbParameter"/> instance using <paramref name="parameter"/>
    /// as a prototype.
    /// </summary>
    /// <remarks>
    /// <para>You can override this method to overcome RDBMS limitations if necessary.</para>
    /// </remarks>
    /// <param name="parameter">A <see cref="Parameter"/> instance to be used 
    /// as prototype for <see cref="DbParameter"/> that will be really
    /// passed to execution.</param>
    /// <returns>Newly created and configured <see cref="DbParameter"/> instance.
    /// </returns>
    protected DbParameter CreateDbParameter(Parameter parameter)
    {
      DbParameter dbParameter = realCommand.CreateParameter();
      dbParameter.DbType = parameter.DbType;
      dbParameter.Direction = parameter.Direction;
      dbParameter.ParameterName = parameter.ParameterName;
      dbParameter.Size = parameter.Size;
      dbParameter.Value = parameter.Value;
      return dbParameter;
    }

    /// <inheritdoc/>
    public override string CommandText
    {
      get { return realCommand.CommandText; }
      set { realCommand.CommandText = value; }
    }

    /// <inheritdoc/>
    public override int CommandTimeout
    {
      get { return realCommand.CommandTimeout; }
      set { realCommand.CommandTimeout = value; }
    }

    /// <inheritdoc/>
    public override CommandType CommandType
    {
      get { return realCommand.CommandType; }
      set { realCommand.CommandType = value; }
    }

    /// <inheritdoc/>
    protected override DbConnection DbConnection
    {
      get { return connection; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        Connection conn = value as Connection;
        if (conn == null)
          throw new NotSupportedException();
        if (realCommand == null || (realCommand != null && realCommand.Connection != conn.RealConnection))
          realCommand = conn.RealConnection.CreateCommand();
        connection = conn;
      }
    }

    /// <inheritdoc/>
    protected override DbParameterCollection DbParameterCollection
    {
      get { return parameters; }
    }

    /// <inheritdoc/>
    protected override DbTransaction DbTransaction
    {
      get { return realCommand.Transaction; }
      set { realCommand.Transaction = value; }
    }

    /// <inheritdoc/>
    public override bool DesignTimeVisible
    {
      get { return realCommand.DesignTimeVisible; }
      set { realCommand.DesignTimeVisible = value; }
    }

    /// <inheritdoc/>
    public override UpdateRowSource UpdatedRowSource
    {
      get { return realCommand.UpdatedRowSource; }
      set { realCommand.UpdatedRowSource = value; }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      realCommand.Dispose();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Command"/> class.
    /// </summary>
    /// <param name="connection">A <see cref="Connection"/> to underlying RDBMS to be used.</param>
    public Command(Connection connection)
    {
      Connection = connection;
    }
  }
}