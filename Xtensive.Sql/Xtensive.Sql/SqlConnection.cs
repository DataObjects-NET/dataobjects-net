// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Sql
{
  /// <summary>
  /// Represents a connection to a database.
  /// </summary>
  public sealed class SqlConnection : IDisposable
  {
    /// <summary>
    /// Gets the state of the connection.
    /// </summary>
    public ConnectionState State { get { return UnderlyingConnection.State; } }

    /// <summary>
    /// Gets the connection info.
    /// </summary>
    public UrlInfo Url { get; private set; }

    /// <summary>
    /// Gets the underlying connection.
    /// </summary>
    public DbConnection UnderlyingConnection { get; private set; }

    /// <summary>
    /// Gets a <see cref="SqlDriver">RDBMS driver</see> the connection is working through.
    /// </summary>
    public SqlDriver Driver { get; private set; }

    /// <summary>
    /// Creates and returns a <see cref="DbCommand"/> object associated with the current connection.
    /// </summary>
    /// <returns>Created command.</returns>
    public DbCommand CreateCommand()
    {
      return CreateCommandInternal();
    }

    /// <summary>
    /// Creates and returns a <see cref="DbCommand"/> object with specified <paramref name="statement"/>.
    /// Created command will be associated with the current connection.
    /// </summary>
    /// <returns>Created command.</returns>
    public DbCommand CreateCommand(ISqlCompileUnit statement)
    {
      ArgumentValidator.EnsureArgumentNotNull(statement, "statement");
      var command = CreateCommandInternal();
      command.CommandText = Driver.Compile(statement).GetCommandText();
      return command;
    }
    
    /// <summary>
    /// Creates and returns a <see cref="DbCommand"/> object with specified <paramref name="commandText"/>.
    /// Created command will be associated with the current connection.
    /// </summary>
    /// <returns>Created command.</returns>
    public DbCommand CreateCommand(string commandText)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(commandText, "commandText");
      var command = CreateCommandInternal();
      command.CommandText = commandText;
      return command;
    }

    /// <summary>
    /// Creates the parameter.
    /// </summary>
    /// <returns>Created parameter.</returns>
    public DbParameter CreateParameter()
    {
      return Driver.ConnectionHandler.CreateParameter();
    }

    /// <summary>
    /// Creates the character large object bound to this connection.
    /// Created object initially have NULL value (<see cref="ILargeObject.IsNull"/> returns <see langword="true"/>)
    /// </summary>
    /// <returns>Created CLOB.</returns>
    public ICharacterLargeObject CreateCharacterLargeObject()
    {
      return Driver.ConnectionHandler.CreateCharacterLargeObject(UnderlyingConnection);
    }

    /// <summary>
    /// Creates the binary large object bound to this connection.
    /// Created object initially have NULL value (<see cref="ILargeObject.IsNull"/> returns <see langword="true"/>)
    /// </summary>
    /// <returns>Created BLOB.</returns>
    public IBinaryLargeObject CreateBinaryLargeObject()
    {
      return Driver.ConnectionHandler.CreateBinaryLargeObject(UnderlyingConnection);
    }

    /// <summary>
    /// Opens the connection.
    /// </summary>
    public void Open()
    {
      UnderlyingConnection.Open();
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    public void Close()
    {
      UnderlyingConnection.Close();
    }

    /// <summary>
    /// Begins the transaction.
    /// </summary>
    /// <returns>Started transaction.</returns>
    public DbTransaction BeginTransaction()
    {
      return Driver.ConnectionHandler.BeginTransaction(UnderlyingConnection);
    }

    /// <summary>
    /// Begins the transaction with the specified <see cref="IsolationLevel"/>.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>Started transaction.</returns>
    public DbTransaction BeginTransaction(IsolationLevel isolationLevel)
    {
      return Driver.ConnectionHandler.BeginTransaction(UnderlyingConnection, isolationLevel);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      UnderlyingConnection.Dispose();
    }

    private DbCommand CreateCommandInternal()
    {
      return Driver.ConnectionHandler.CreateCommand(UnderlyingConnection);
    }

    // Constructors

    internal SqlConnection(SqlDriver driver, UrlInfo url)
    {
      Driver = driver;
      UnderlyingConnection = driver.ConnectionHandler.CreateConnection(url);
      Url = url;
    }
  }
}