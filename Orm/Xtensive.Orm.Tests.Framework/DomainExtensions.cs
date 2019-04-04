// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.03.22

using System.Transactions;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Contains extensions for <see cref="Domain"/>.
  /// </summary>
  public static class DomainExtensions
  {
    #region Session and Transaction openers

    /// <summary>
    /// Opens session and transaction.
    /// </summary>
    /// <param name="domain">Domain to open session and transaction for.</param>
    /// <returns>Accessor to opened session and transaction.</returns>
    public static ITestSessionAccessor OpenSessionAndTransaction(this Domain domain)
    {
      var session = domain.OpenSession();
      var transaction = session.OpenTransaction();
      return new TestSessionAccessor(session, transaction);
    }

    /// <summary>
    /// Opens session and transaction.
    /// </summary>
    /// <param name="domain">Domain to open session and transaction for.</param>
    /// <param name="activate"><see langword="true"/> if opening session should be activated, otherwise, <see langword="false"/>.</param>
    /// <returns>Accessor to opened session and transaction.</returns>
    public static ITestSessionAccessor OpenSessionAndTransaction(this Domain domain, bool activate)
    {
      var options = SessionOptions.Default;
      if (activate)
        options |= SessionOptions.AutoActivation;

      var session = domain.OpenSession(new SessionConfiguration(options));
      var transaction = session.OpenTransaction();
      return new TestSessionAccessor(session, transaction);
    }

    /// <summary>
    /// Opens session with configuration with specified name and transaction.
    /// </summary>
    /// <param name="domain">Domain to open session and transaction for.</param>
    /// <param name="sessionName">Name of configuration in in <see cref="DomainConfiguration.Sessions"/>.</param>
    /// <returns>Accessor to opened session and transaction.</returns>
    public static ITestSessionAccessor OpenSessionAndTransaction(this Domain domain, string sessionName)
    {
      var session = domain.OpenSession(domain.Configuration.Sessions[sessionName]);
      var transaction = session.OpenTransaction();
      return new TestSessionAccessor(session, transaction);
    }

    /// <summary>
    /// Opens session with configuration with specified name and transaction.
    /// </summary>
    /// <param name="domain">Domain to open session and transaction for.</param>
    /// <param name="sessionName">Name of configuration in in <see cref="DomainConfiguration.Sessions"/>.</param>
    /// <param name="activate"><see langword="true"/> if opening session should be activated, otherwise, <see langword="false"/></param>
    /// <returns>Accessor to opened session and transaction.</returns>
    public static ITestSessionAccessor OpenSessionAndTransaction(this Domain domain, string sessionName, bool activate)
    {
      var configuration = domain.Configuration.Sessions[sessionName].Clone();
      if (activate)
        configuration.Options |= SessionOptions.AutoActivation;

      var session = domain.OpenSession(configuration);
      var transaction = session.OpenTransaction();
      return new TestSessionAccessor(session, transaction);
    }

    /// <summary>
    /// Opens session with specified options and transaction.
    /// </summary>
    /// <param name="domain">Domain to open session and transaction for.</param>
    /// <param name="options">Session options.</param>
    /// <returns>Accessor to opened session and transaction.</returns>
    public static ITestSessionAccessor OpenSessionAndTransaction(this Domain domain, SessionOptions options)
    {
      var session = domain.OpenSession(new SessionConfiguration(options));
      var transaction = session.OpenTransaction();
      return new TestSessionAccessor(session, transaction);
    }

    /// <summary>
    /// Opens session with specified configuration and transaction.
    /// </summary>
    /// <param name="domain">Domain to open session and transaction for.</param>
    /// <param name="sessionConfiguration">Session configuration.</param>
    /// <returns>Accessor to opened session and transaction.</returns>
    public static ITestSessionAccessor OpenSessionAndTransaction(this Domain domain,
      SessionConfiguration sessionConfiguration, bool activate)
    {
      var configuration = sessionConfiguration.Clone();
      if (activate)
        configuration.Options |= SessionOptions.AutoActivation;

      var session = domain.OpenSession(configuration);
      var transaction = session.OpenTransaction();
      return new TestSessionAccessor(session, transaction);
    }

    /// <summary>
    /// Opens session and transaction with specified open mode.
    /// </summary>
    /// <param name="domain">Domain to open session and transaction for.</param>
    /// <param name="transactionOpenMode">Open mode for transaction.</param>
    /// <returns>Accessor to opened session and transaction.</returns>
    public static ITestSessionAccessor OpenSessionAndTransaction(this Domain domain, TransactionOpenMode transactionOpenMode)
    {
      var session = domain.OpenSession();
      var transaction = session.OpenTransaction();
      return new TestSessionAccessor(session, transaction);
    }

    /// <summary>
    /// Opens session and transaction with specified isolation level
    /// </summary>
    /// <param name="domain">Domain to open session and transaction for.</param>
    /// <param name="isolationLevel">Isolation level for transaction.</param>
    /// <returns>Accessor to opened session and transaction.</returns>
    public static ITestSessionAccessor OpenSessionAndTransaction(this Domain domain, IsolationLevel isolationLevel)
    {
      var session = domain.OpenSession();
      var transaction = session.OpenTransaction(isolationLevel);
      return new TestSessionAccessor(session, transaction);
    }

    /// <summary>
    /// Opens session with specified configuration and transaction with specified open mode.
    /// </summary>
    /// <param name="domain">Domain to open session and transaction for.</param>
    /// <param name="sessionConfiguration">Session configuration.</param>
    /// <param name="transactionOpenMode">Open mode for transaction.</param>
    /// <returns>Accessor to opened session and transaction.</returns>
    public static ITestSessionAccessor OpenSessionAndTransaction(this Domain domain,
      SessionConfiguration sessionConfiguration, TransactionOpenMode transactionOpenMode)
    {
      var session = domain.OpenSession(sessionConfiguration);
      var transaction = session.OpenTransaction(transactionOpenMode);
      return new TestSessionAccessor(session, transaction);
    }

    /// <summary>
    /// Opens session with specified configuration and transaction with specified isolation level.
    /// </summary>
    /// <param name="domain">Domain to open session and transaction for.</param>
    /// <param name="sessionConfiguration">Session configuration.</param>
    /// <param name="isolationLevel">Isolation level for transaction.</param>
    /// <returns>Accessor to opened session and transaction.</returns>
    public static ITestSessionAccessor OpenSessionAndTransaction(this Domain domain,
      SessionConfiguration sessionConfiguration, IsolationLevel isolationLevel)
    {
      var session = domain.OpenSession(sessionConfiguration);
      var transaction = session.OpenTransaction(isolationLevel);
      return new TestSessionAccessor(session, transaction);
    }

    /// <summary>
    /// Opens session with specified configuration and transaction with specified open mode and isolation level.
    /// </summary>
    /// <param name="domain">Domain to open session and transaction for.</param>
    /// <param name="sessionConfiguration">Session configuration.</param>
    /// <param name="transactionOpenMode">Open mode for transaction.</param>
    /// <param name="isolationLevel">Isolation level for transaction.</param>
    /// <returns>Accessor to opened session and transaction.</returns>
    public static ITestSessionAccessor OpenSessionAndTransaction(this Domain domain,
      SessionConfiguration sessionConfiguration, TransactionOpenMode transactionOpenMode, IsolationLevel isolationLevel)
    {
      var session = domain.OpenSession(sessionConfiguration);
      var transaction = session.OpenTransaction(transactionOpenMode, isolationLevel);
      return new TestSessionAccessor(session, transaction);
    }

    #endregion
  }
}