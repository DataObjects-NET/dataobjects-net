// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.08

using System;
using System.Data.Common;
using Xtensive.Aspects;
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
  [Infrastructure]
  public sealed class DirectSqlAccessor : SessionBound,
    ISessionService
  {
    private readonly IDirectSqlService service;

    /// <summary>
    /// Gets a value indicating whether direct SQL capabilities are available.
    /// Returns <see langword="true" />, if underlying storage provider 
    /// supports SQL.
    /// </summary>
    public bool IsAvailable {
      get {
        return service!=null;
      }
    }

    /// <see cref="IDirectSqlService.Connection" copy="true" />
    public DbConnection Connection {
      get {
        EnsureIsAvailable();
        return service.Connection;
      }
    }

    /// <see cref="IDirectSqlService.Transaction" copy="true" />
    public DbTransaction Transaction {
      get {
        EnsureIsAvailable();
        TryStartTransaction();
        return service.Transaction;
      }
    }

    /// <see cref="IDirectSqlService.CreateCommand" copy="true" />
    public DbCommand CreateCommand()
    {
      EnsureIsAvailable();
      TryStartTransaction();
      return service.CreateCommand();
    }

    private void TryStartTransaction()
    {
      if (Session.Transaction!=null)
        Session.EnsureTransactionIsStarted();
    }

    /// <exception cref="NotSupportedException">Underlying storage provider 
    /// does not support SQL.</exception>
    private void EnsureIsAvailable()
    {
      if (service==null)
        throw new NotSupportedException(Strings.ExUnderlyingStorageProviderDoesNotSupportSQL);
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
      service = session.Services.Demand<SessionHandler>().GetService<IDirectSqlService>();
    }
  }
}