// Copyright (C) 2019-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.IoC;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Tracking
{
  [Service(typeof (ISessionTrackingMonitor), Singleton = true)]
  internal sealed class SessionTrackingMonitor : TrackingMonitor, ISessionTrackingMonitor, ISessionService
  {
    private readonly Session session;
    private readonly DirectSessionAccessor accessor;
    private readonly Stack<TrackingStackFrame> stack = new(2);

    private void Subscribe()
    {
      var events = session.Events;
      events.Persisting += OnPersisting;
      events.TransactionOpened += OnOpenTransaction;
      events.TransactionCommitted += OnCommitTransaction;
      events.TransactionRollbacked += OnRollbackTransaction;
    }

    private void OnOpenTransaction(object sender, TransactionEventArgs e)
    {
      stack.Push(new TrackingStackFrame(false));
    }

    private void OnCommitTransaction(object sender, TransactionEventArgs e)
    {
      var source = stack.Pop();
      var target = stack.Peek();
      target.MergeWith(source);

      if (e.Transaction.IsNested)
        return;

      var items = target.Cast<ITrackingItem>().ToList().AsReadOnly();
      target.Clear();

      RaiseTrackingCompleted(new TrackingCompletedEventArgs(session, items));
    }

    private void OnRollbackTransaction(object sender, TransactionEventArgs e)
    {
      stack.Pop();
    }

    private void OnPersisting(object sender, EventArgs e)
    {
      var frame = stack.Peek();

      foreach (var state in accessor.GetChangedEntities(PersistenceState.Removed))
        frame.Register(new TrackingItem(state.Key, TrackingItemState.Deleted, state.DifferentialTuple));

      foreach (var state in accessor.GetChangedEntities(PersistenceState.New))
        frame.Register(new TrackingItem(state.Key, TrackingItemState.Created, state.DifferentialTuple));

      foreach (var state in accessor.GetChangedEntities(PersistenceState.Modified))
        frame.Register(new TrackingItem(state.Key, TrackingItemState.Changed, state.DifferentialTuple));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionTrackingMonitor"/> class.
    /// </summary>
    /// <param name="session"><see cref="T:Xtensive.Orm.Session"/>, to which current instance
    /// is bound.</param>
    /// <param name="accessor"><see cref="DirectSessionAccessor"/> instance to get access to
    /// changed entities of each kind. See <see cref="DirectSessionAccessor.GetChangedEntities"/> method
    /// for reference.</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="session"/> is <see langword="null"/>.</exception>
    [ServiceConstructor]
    public SessionTrackingMonitor(Session session, DirectSessionAccessor accessor)
    {
      ArgumentNullException.ThrowIfNull(session);
      ArgumentNullException.ThrowIfNull(accessor);
      this.session = session;
      this.accessor = accessor;

      stack.Push(new TrackingStackFrame(false));

      Subscribe();
    }
  }
}
