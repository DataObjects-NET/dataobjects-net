// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using System;
using System.Data.Common;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Resources;

namespace Xtensive.Sql
{
  public abstract class SqlConnectionHandler
  {
    public SqlDriver Driver { get; private set; }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// Creates the connection from the specified URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>Created connection.</returns>
    public abstract DbConnection CreateConnection(UrlInfo url);

    /// <summary>
    /// Creates the command bound to the specified <see cref="DbConnection"/>.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>Created command.</returns>
    public virtual DbCommand CreateCommand(DbConnection connection)
    {
      return connection.CreateCommand();
    }

    /// <summary>
    /// Creates the character large object.
    /// Created object initially have NULL value (<see cref="ILargeObject.IsNull"/> returns <see langword="true"/>)
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>Created CLOB.</returns>
    public virtual ICharacterLargeObject CreateCharacterLargeObject(DbConnection connection)
    {
      throw new NotSupportedException(Strings.ExLargeObjectsAreNotSupportedByThisServer);
    }

    /// <summary>
    /// Creates the binary large object.
    /// Created object initially have NULL value (<see cref="ILargeObject.IsNull"/> returns <see langword="true"/>)
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>Created BLOB.</returns>
    public virtual IBinaryLargeObject CreateBinaryLargeObject(DbConnection connection)
    {
      throw new NotSupportedException(Strings.ExLargeObjectsAreNotSupportedByThisServer);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected SqlConnectionHandler(SqlDriver driver)
    {
      Driver = driver;
    }
  }
}