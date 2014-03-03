// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.08

using System.Data.Common;
using Xtensive.Core;

using Xtensive.IoC;
using Xtensive.Orm.Providers;


namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Provides access to such low-level objects as 
  /// <see cref="DbCommand"/> and <see cref="DbConnection"/>.
  /// </summary>
  [Service(typeof(DirectSqlAccessor))]
  public sealed class DirectSqlAccessor : SessionBound,
    ISessionService
  {
    private readonly IDirectSqlService service;

    /// <see cref="IDirectSqlService.Connection" copy="true" />
    public DbConnection Connection {
      get {
        return service.Connection;
      }
    }

    /// <see cref="IDirectSqlService.Transaction" copy="true" />
    public DbTransaction Transaction {
      get {
        return service.Transaction;
      }
    }

    /// <see cref="IDirectSqlService.RegisterInitializationSql" copy="true" />
    public void RegisterInitializationSql(string sql)
    {
      service.RegisterInitializationSql(sql);
    }

    /// <see cref="IDirectSqlService.CreateCommand" copy="true" />
    public DbCommand CreateCommand()
    {
      return service.CreateCommand();
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session this instance is bound to.</param>
    [ServiceConstructor]
    public DirectSqlAccessor(Session session)
      : base(session)
    {
      service = session.Services.Demand<IDirectSqlService>();
    }
  }
}