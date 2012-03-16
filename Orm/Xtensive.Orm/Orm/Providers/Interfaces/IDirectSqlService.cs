// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.08

using System;
using System.Data.Common;
using Xtensive.Orm.Services;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A handler used by <see cref="DirectSqlAccessor"/>.
  /// If implemented by provider
  /// </summary>
  public interface IDirectSqlService
  {
    /// <summary>
    /// Gets the underlying connection that is currently in use.
    /// </summary>
    DbConnection Connection { get; }

    /// <summary>
    /// Gets the underlying transaction that is currently running.
    /// <see langword="null" />, if transaction isn't running now.
    /// </summary>
    DbTransaction Transaction { get; }

    /// <summary>
    /// Creates the <see cref="DbCommand"/> object associated with the
    /// current <see cref="Connection"/> and <see cref="Transaction"/>.
    /// </summary>
    /// <returns>Newly created <see cref="DbCommand"/> object.</returns>
    /// <exception cref="InvalidOperationException">Connection is not open.</exception>
    DbCommand CreateCommand();
  }
}