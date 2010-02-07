// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.08

using System;
using System.Data.Common;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.IoC;
using Xtensive.Storage.Providers.Sql.Resources;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// <see cref="IDirectSqlHandler"/> implementation for any
  /// SQL storage provider.
  /// </summary>
  [Service(typeof(IDirectSqlHandler))]
  public class DirectSqlHandler : IDirectSqlHandler
  {
    private SessionHandler sessionHandler;

    /// <inheritdoc/>
    public virtual DbConnection Connection {
      get {
        var sqlConnection = sessionHandler.Connection;
        return sqlConnection==null ? null : sqlConnection.UnderlyingConnection;
      }
    }

    /// <inheritdoc/>
    public virtual DbTransaction Transaction {
      get {
        var sqlConnection = sessionHandler.Connection;
        return sqlConnection==null ? null : sqlConnection.ActiveTransaction;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Connection is not open.</exception>
    public virtual DbCommand CreateCommand()
    {
      var sqlConnection = sessionHandler.Connection;
      if (sqlConnection==null)
        throw new InvalidOperationException(Strings.ExConnectionIsNotOpen);
      return sqlConnection.CreateCommand();
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sessionHandler">The session handler this instance is bound to.</param>
    [ServiceConstructor]
    public DirectSqlHandler(Providers.SessionHandler sessionHandler)
    {
      this.sessionHandler = (SessionHandler) sessionHandler; // Casting to Sql.SessionHandler
    }
  }
}