// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2008.08.20

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm
{
  /// <summary>
  /// An implementation of transaction suitable for storage.
  /// </summary>
  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  public sealed partial class Transaction : IHasExtensions
  {
    #region Current, Demand() members (static)

    /// <summary>
    /// Gets the current <see cref="Transaction"/> object
    /// using <see cref="Session"/>.<see cref="Orm.Session.Current"/>.
    /// </summary>
    public static Transaction Current => Session.Current?.Transaction;

    /// <summary>
    /// Gets the current <see cref="Transaction"/>, 
    /// or throws <see cref="InvalidOperationException"/>, 
    /// if active <see cref="Transaction"/> is not found.
    /// </summary>
    /// <returns>Current transaction.</returns>
    /// <exception cref="InvalidOperationException">
    /// <see cref="Transaction.Current"/> <see cref="Transaction"/> is <see langword="null" />.
    /// </exception>
    public static Transaction Demand() =>
      Current ?? throw new InvalidOperationException(Strings.ExActiveTransactionIsRequiredForThisOperationUseSessionOpenTransactionToOpenIt);

    /// <summary>
    /// Checks whether a transaction exists or not in the provided session.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <exception cref="InvalidOperationException"><see cref="Transaction.Current"/> <see cref="Transaction"/> is <see langword="null" />.</exception>
    public static void Require(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, nameof(session));
      session.DemandTransaction();
    }

    #endregion

    private readonly List<StateLifetimeToken> lifetimeTokens = new(1);

    private ExtensionCollection extensions;
    private Transaction inner;

    /// <summary>
    /// Gets a value indicating whether this instance is automatic transaction.
    /// </summary>
    public bool IsAutomatic { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is 
    /// transaction running locally.
    /// </summary>
    public bool IsDisconnected { get; }

    private Guid? guid;
    /// <summary>
    /// Gets the unique identifier of this transaction.
    /// Nested transactions have the same <see cref="Guid"/> 
    /// as their outermost.
    /// </summary>
    public Guid Guid => Outer?.Guid ?? (guid ??= Guid.NewGuid());

    /// <summary>
    /// Gets the session this transaction is bound to.
    /// </summary>
    public Session Session { get; }

    /// <summary>
    /// Gets the isolation level.
    /// </summary>
    public IsolationLevel IsolationLevel { get; }

    /// <summary>
    /// Gets the state of the transaction.
    /// </summary>
    public TransactionState State { get; private set; } = TransactionState.NotActivated;

    /// <summary>
    /// Gets the outer transaction.
    /// </summary>
    public Transaction Outer { get; }

    /// <summary>
    /// Gets the outermost transaction.
    /// </summary>
    public Transaction Outermost => Outer?.Outermost ?? this;

    /// <summary>
    /// Gets the start time of this transaction.
    /// </summary>
    public DateTime TimeStamp { get; } = DateTime.UtcNow;

    private TimeSpan? timeout;
    /// <summary>
    /// Gets or sets Transaction timeout
    /// </summary>
    public TimeSpan? Timeout {
      get => timeout;
      set => timeout = IsNested
          ? throw new InvalidOperationException(Strings.ExNestedTransactionTimeout)
          : value;
    }

    /// <summary>
    /// Gets a value indicating whether this transaction is a nested transaction.
    /// </summary>
    public bool IsNested => Outer != null;

    /// <summary>
    /// Gets <see cref="StateLifetimeToken"/> associated with this transaction.
    /// </summary>
    public StateLifetimeToken LifetimeToken { get; private set; } = new();

    #region IHasExtensions Members

    /// <inheritdoc/>
    public IExtensionCollection Extensions => extensions ??= new ExtensionCollection();

    #endregion

    internal string SavepointName { get; }

    /// <summary>
    /// Indicates whether changes made in this transaction are visible "as is"
    /// in <paramref name="otherTransaction"/>. This implies <paramref name="otherTransaction"/>
    /// and this transaction at least share the same <see cref="Outermost"/> transaction.
    /// Please refer to the code of this method to clearly understand what it really does ;)
    /// </summary>
    /// <param name="otherTransaction">The other transaction.</param>
    /// <returns>
    /// <see langword="True"/> if changes made in this transaction are visible
    /// "as is" in <paramref name="otherTransaction"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool AreChangesVisibleTo(Transaction otherTransaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(otherTransaction, "otherTransaction");
      if (Outermost != otherTransaction.Outermost) {
        return false;
      }

      var t = this;
      var outermost = t.Outermost;
      while (t != outermost && t != otherTransaction && t.State == TransactionState.Committed) {
        t = t.Outer;
      }

      return t.State.IsActive();
    }

    #region Private / internal methods

    internal void Begin()
    {
      Session.BeginTransaction(this);
      if (Outer != null) {
        Outer.inner = this;
      }

      State = TransactionState.Active;
    }

    internal async ValueTask BeginAsync(CancellationToken token)
    {
      await Session.BeginTransactionAsync(this, token).ConfigureAwait(false);
      if (Outer != null) {
        Outer.inner = this;
      }

      State = TransactionState.Active;
    }

    internal async ValueTask Commit(bool isAsync)
    {
      EnsureIsActive();
      State = TransactionState.Committing;
      try {
        if (inner != null) {
          throw new InvalidOperationException(Strings.ExCanNotCompleteOuterTransactionInnerTransactionIsActive);
        }

        await Session.CommitTransaction(this, isAsync).ConfigureAwait(false);
      }
      catch {
        await Rollback(isAsync).ConfigureAwait(false);
        throw;
      }

      if (Outer != null) {
        PromoteLifetimeTokens();
      }
      else if (Session.Configuration.Supports(SessionOptions.NonTransactionalReads)) {
        ClearLifetimeTokens();
      }
      else {
        ExpireLifetimeTokens();
      }

      State = TransactionState.Committed;
      EndTransaction();
    }

    internal async ValueTask Rollback(bool isAsync)
    {
      EnsureIsActive();
      State = TransactionState.RollingBack;
      try {
        try {
          if (inner != null) {
            await inner.Rollback(isAsync).ConfigureAwait(false);
          }
        }
        finally {
          await Session.RollbackTransaction(this, isAsync).ConfigureAwait(false);
        }
      }
      finally {
        ExpireLifetimeTokens();
        State = TransactionState.RolledBack;
        EndTransaction();
      }
    }

    private void EndTransaction()
    {
      if (Outer != null) {
        Outer.inner = null;
      }

      Session.CompleteTransaction(this);
    }

    private void EnsureIsActive()
    {
      if (!State.IsActive()) {
        throw new InvalidOperationException(Strings.ExTransactionIsNotActive);
      }
    }

    private void ExpireLifetimeTokens()
    {
      foreach (var token in lifetimeTokens) {
        token.Expire();
      }

      ClearLifetimeTokens();
    }

    private void PromoteLifetimeTokens()
    {
      Outer.lifetimeTokens.AddRange(lifetimeTokens);
      ClearLifetimeTokens();
    }

    private void ClearLifetimeTokens()
    {
      lifetimeTokens.Clear();
      LifetimeToken = null;
    }

    internal void CheckForTimeout(DbCommand command)
    {
      if (Timeout is not null) {
          var remain = TimeStamp + Timeout.Value - DateTime.UtcNow;
          command.CommandTimeout = remain.Ticks > 0
            ? Math.Max(1, (int) remain.TotalSeconds)
            : throw new TimeoutException(String.Format(Strings.ExTransactionTimeout, Timeout));
      }
    }

    #endregion


    // Constructors

    internal Transaction(Session session, IsolationLevel isolationLevel, bool isAutomatic, Transaction outer = null,
      string savepointName = null)
    {
      Session = session;
      IsolationLevel = isolationLevel;
      IsAutomatic = isAutomatic;
      IsDisconnected = session.IsDisconnected;
      lifetimeTokens.Add(LifetimeToken);

      if (outer != null) {
        Outer = outer;
        SavepointName = savepointName;
      }
    }
  }
}
