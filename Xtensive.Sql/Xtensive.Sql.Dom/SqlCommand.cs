// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Compiler;

namespace Xtensive.Sql.Dom
{
  public class SqlCommand : Command
  {
    private SqlCompilerResults results;
    private ISqlCompileUnit statement;
    private SqlDriver driver;
    private ParameterCollection<SqlParameter> parameters = new ParameterCollection<SqlParameter>();

    /// <summary>
    /// Invalidates this instance.
    /// </summary>
    public void Invalidate()
    {
      results = null;
    }

    /// <summary>
    /// Gets or sets the statement.
    /// </summary>
    /// <value>The executable statement.</value>
    public ISqlCompileUnit Statement
    {
      get { return statement; }
      set {
        statement = value;
        Invalidate();
      }
    }

    /// <summary>
    /// Creates and adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name and the data type.
    /// </summary>
    /// <returns>
    /// The <see cref="SqlParameter"/> object that added to the collection
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    public new SqlParameter CreateParameter(string parameterName, DbType dbType)
    {
      return (SqlParameter)base.CreateParameter(parameterName, dbType);
    }

    /// <summary>
    /// Creates and adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name and the value.
    /// </summary>
    /// <returns>
    /// The <see cref="SqlParameter"/> object that added to the collection.
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="value">A value of the parameter.</param>
    public new SqlParameter CreateParameter(string parameterName, object value)
    {
      return (SqlParameter)base.CreateParameter(parameterName, value);
    }

    /// <summary>
    /// Creates and adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name, the data type and the column length.
    /// </summary>
    /// <returns>
    /// The <see cref="SqlParameter"/> object that added to the collection.
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    /// <param name="size">A column length.</param>
    public new SqlParameter CreateParameter(string parameterName, DbType dbType, int size)
    {
      return (SqlParameter)base.CreateParameter(parameterName, dbType, size);
    }

    /// <summary>
    /// Creates and adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name, the data type, the column length and the source
    /// column name.
    /// </summary>
    /// <returns>
    /// The <see cref="SqlParameter"/> object that added to the collection.
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    /// <param name="size">A column length.</param>
    /// <param name="sourceColumn">A source column name.</param>
    public new SqlParameter CreateParameter(string parameterName, DbType dbType, int size, string sourceColumn)
    {
      return (SqlParameter)base.CreateParameter(parameterName, dbType, size, sourceColumn);
    }

    /// <summary>
    /// Creates a prepared (or compiled) version of the command on the data source.
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Data.IDbCommand.Connection"></see> is not set.-or- The <see cref="P:System.Data.IDbCommand.Connection"></see> is not opened. </exception>
    public override void Prepare()
    {
      if (results!=null)
        return;
      RealCommand.Parameters.Clear();
      results = driver.Compile(statement);
      base.CommandText = results.CommandText;
      if (Parameters.Count==0)
        return;
      foreach (SqlParameter p in Parameters) {
        RealCommand.Parameters.Add(CreateDbParameter(p));
      }
    }

    ///<summary>
    ///Creates a new instance of an <see cref="T:System.Data.IDbDataParameter"></see> object.
    ///</summary>
    ///<returns>An <see cref="IDbDataParameter"/> object.</returns>
    protected override DbParameter CreateDbParameter()
    {
      return new SqlParameter();
    }

    ///<summary>
    ///Executes an SQL statement against the Connection object of a .NET Framework data provider, and returns the number of rows affected.
    ///</summary>
    ///<returns>The number of rows affected.</returns>
    ///<exception cref="T:System.InvalidOperationException">The connection does not exist.-or- The connection is not open. </exception>
    public override int ExecuteNonQuery()
    {
      Prepare();
      return base.ExecuteNonQuery();
    }

    ///<summary>
    ///Executes the <see cref="P:System.Data.IDbCommand.CommandText"></see> against the <see cref="P:System.Data.IDbCommand.Connection"></see> and builds an <see cref="T:System.Data.IDataReader"></see>.
    ///</summary>
    ///<returns>An <see cref="T:System.Data.IDataReader"></see> object.</returns>
   protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      Prepare();
      return base.ExecuteDbDataReader(behavior);
    }

    ///<summary>
    ///Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
    ///</summary>
    ///<returns>The first column of the first row in the resultset.</returns>
    public override object ExecuteScalar()
    {
      Prepare();
      return base.ExecuteScalar();
    }

    ///<summary>
    ///Gets or sets the <see cref="T:System.Data.IDbConnection"></see> used by this instance of the <see cref="T:System.Data.IDbCommand"></see>.
    ///</summary>
    ///<returns>The connection to the data source.</returns>
    protected override DbConnection DbConnection
    {
      get { return base.DbConnection; }
      set {
        SqlConnection conn = value as SqlConnection;
        if (conn==null)
          throw new NotSupportedException();
        base.DbConnection = conn;
        driver = conn.Driver as SqlDriver;
        Invalidate();
      }
    }

    ///<summary>
    ///Gets the collection of <see cref="T:System.Data.Common.DbParameter"></see> objects.
    ///</summary>
    ///
    ///<returns>
    ///The parameters of the SQL statement or stored procedure.
    ///</returns>
    ///
    protected override DbParameterCollection DbParameterCollection
    {
      get { return Parameters; }
    }

    ///<summary>
    ///Gets or sets the text command to run against the data source.
    ///</summary>
    ///<returns>The text command to execute. The default value is an empty string ("").</returns>
    public override string CommandText
    {
      get {
        Prepare();
        return results.CommandText;
      }
      set { base.CommandText = value; }
    }

    ///<summary>
    ///Indicates or specifies how the <see cref="P:System.Data.IDbCommand.CommandText"></see> property is interpreted.
    ///</summary>
    ///<returns>One of the <see cref="T:System.Data.CommandType"></see> values. The default is Text.</returns>
    public override CommandType CommandType
    {
      get { return base.CommandType; }
      set { throw new NotSupportedException(); }
    }

    /// <summary>
    /// Gets the collection of <see cref="T:System.Data.Common.DbParameter"/> objects.
    /// </summary>
    /// <value></value>
    /// <returns>The parameters of the SQL statement or stored procedure.</returns>
    public new ParameterCollection<SqlParameter> Parameters
    {
      get { return parameters; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlCommand"/> class.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public SqlCommand(SqlConnection connection) : base(connection)
    {
    }
  }
}