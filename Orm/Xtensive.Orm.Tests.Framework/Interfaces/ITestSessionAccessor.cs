// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.03.22

using System;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Provides access to session and transaction.
  /// </summary>
  public interface ITestSessionAccessor : IDisposable
  {
    /// <summary>
    /// Opened <see cref="Xtensive.Orm.Session">session</see>.
    /// </summary>
    Session Session { get; }

    /// <summary>
    /// Opened <see cref="TransactionScope">transaction</see>.
    /// </summary>
    TransactionScope Transaction { get; }

    /// <summary>
    /// Gives access to query building root from <see cref="Session"/>.
    /// </summary>
    QueryEndpoint Query { get; }

    /// <summary>
    /// Marks <see cref="Transaction"/> completed.
    /// </summary>
    void Complete();
  }
}