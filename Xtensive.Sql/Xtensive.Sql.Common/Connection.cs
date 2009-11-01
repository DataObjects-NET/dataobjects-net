// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data;
using System.Data.Common;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Represents a connection to a database.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="Connection"/> object represents a unique session to an
  /// underlying data source. With a client/server database system,
  /// it is equivalent to a network connection to the server.
  /// </para>
  /// <para>
  /// A <see cref="Connection"/> actually is just a wrapper of a real connection
  /// to data source of some Data Provider.
  /// </para>
  /// <para>
  /// If the <see cref="Connection"/> goes out of scope, it is not closed. 
  /// Therefore, you must explicitly close the connection by calling 
  /// <see cref="Close"/> or <see cref="Dispose"/>.
  /// They are functionally equivalent. 
  /// You can also open the connection inside of a <see langword="using"/> block, 
  /// which ensures that the connection is closed when the code exits it.
  //  ///   <code source="..." lang="cs" region="UsingConnection"/>
  /// </para>
  /// </remarks>
  /// <threadsafety static="true" instance="false"/>
  /// <seealso cref="Command"/>
  /// <seealso cref="Driver"/>
  public class Connection : DbConnection
  {
    private DbConnection realConnection;
    private Driver driver;
    private ConnectionInfo connectionInfo;

    /// <summary>
    /// Gets the connection info.
    /// </summary>
    /// <value>The connection info.</value>
    public ConnectionInfo ConnectionInfo
    {
      get { return connectionInfo; }
    }

    /// <summary>
    /// Gets a <see cref="Driver">RDBMS driver</see> the connection is working through.
    /// </summary>
    /// <seealso cref="Driver"/>
    public Driver Driver
    {
      get { return driver; }
    }

    /// <summary>
    /// Gets a <see cref="DbConnection"/> object which will really handle
    /// all requests to data source.
    /// </summary>
    /// <remarks>
    /// <see cref="Connection"/> internally uses some another connection 
    /// that is obtained from Data Provider for some RDBMS.
    /// </remarks>
    public DbConnection RealConnection
    {
      get { return realConnection; }
    }

    /// <summary>
    /// Starts a database transaction.
    /// </summary>
    /// <returns>
    /// An object representing the new transaction.
    /// </returns>
    /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      return realConnection.BeginTransaction(isolationLevel);
    }

    /// <summary>
    /// Changes the current database for an open connection.
    /// </summary>
    /// <param name="databaseName">Specifies the name of the database for the connection to use.</param>
    public override void ChangeDatabase(string databaseName)
    {
      realConnection.ChangeDatabase(databaseName);
    }

    /// <summary>
    /// Creates and returns a <see cref="Command"/> object associated with the current connection.
    /// </summary>
    /// <returns>
    /// New <see cref="Command"/> instance associated with the <see cref="Connection"/>.
    /// </returns>
    protected override DbCommand CreateDbCommand()
    {
      return new Command(this);
    }

    /// <summary>
    /// Opens a database connection with the settings specified by the 
    /// <see cref="ConnectionString"/>.
    /// </summary>
    public override void Open()
    {
      realConnection.Open();
    }

    /// <summary>
    /// Closes the connection to the database. This is the preferred method of closing any open connection.
    /// </summary>
    /// <exception cref="DbException">The connection-level error that occurred while opening the connection.</exception>
    public override void Close()
    {
      realConnection.Close();
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="Connection"/> and optionally
    /// releases the managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      try {
        realConnection.Dispose();
      }catch {}
    }

    /// <summary>
    /// Gets or sets the string used to open the connection.
    /// </summary>
    /// <value>
    /// <para>The connection string used to establish the initial connection.</para>
    /// <para>The exact contents of the connection string depend on the specific 
    /// data source for this connection.</para>
    /// <para>The default value is an empty string.</para>
    /// </value>
    public override string ConnectionString
    {
      get { return realConnection.ConnectionString; }
      set { realConnection.ConnectionString = value; }
    }

    /// <summary>
    /// Gets the name of the current database after a connection is opened,
    /// or the database name specified in the connection string before the
    /// connection is opened.
    /// </summary>
    /// <value>
    /// <para>The name of the current database or the name of the database 
    /// to be used after a connection is opened.</para>
    /// <para>The default value is an empty string.</para>
    /// </value>
    public override string Database
    {
      get { return realConnection.Database; }
    }

    /// <summary>
    /// Gets the name of the database server to which to connect.
    /// </summary>
    /// <value>
    /// <para>The name of the database server to which to connect.</para>
    /// <para>The default value is an empty string.</para>
    /// </value>
    public override string DataSource
    {
      get { return realConnection.DataSource; }
    }

    /// <summary>
    /// Gets a string that represents the version of the server to which the object is connected.
    /// </summary>
    /// <returns>
    /// <para>The version of the database.</para>
    /// <para>The format of the string returned depends on the specific 
    /// type of connection you are using.</para>
    /// </returns>
    public override string ServerVersion
    {
      get { return realConnection.ServerVersion; }
    }

    /// <summary>
    /// Gets a string that describes the state of the connection.
    /// </summary>
    /// <returns>
    /// <para>The state of the connection.</para>
    /// <para>The format of the string returned depends on the
    /// specific type of connection you are using.</para>
    /// </returns>
    public override ConnectionState State
    {
      get { return realConnection.State; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Connection"/> class.
    /// </summary>
    /// <param name="driver">A <see cref="Driver">driver</see> instance which
    /// will proceed RDBMS interaction.</param>
    /// <param name="realConnection">The real connection.</param>
    /// <param name="connectionInfo">The connection info.</param>
    public Connection(Driver driver, DbConnection realConnection, ConnectionInfo connectionInfo)
    {
      this.driver = driver;
      this.realConnection = realConnection;
      this.connectionInfo = connectionInfo;
    }
  }
}