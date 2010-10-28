// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Transactions;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Commands
{
  /// <summary>
  /// Indexing storage API.
  /// </summary>
  public interface IStorage
  {
    /// <summary>
    /// Creates a new transactional view.
    /// </summary>
    /// <param name="isolationLevel">Required isolation level.</param>
    /// <returns>New transactional view.</returns>
    IStorageView CreateView(SessionHandler sessionHandler, IsolationLevel isolationLevel);

    /// <summary>
    /// Gets the transactional view.
    /// </summary>
    /// <param name="transactionId">The transaction identifier to get the view for.</param>
    /// <returns>The transactional view;
    /// <see langword="null" />, if no view is available for the specified transaction.</returns>
    IStorageView GetView(SessionHandler sessionHandler, Guid transactionId);
  }
}