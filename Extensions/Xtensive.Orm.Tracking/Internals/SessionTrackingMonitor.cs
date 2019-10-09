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
    private readonly Stack<TrackingStackFrame> stack;

    private void Subscribe()
    {
      session.Events.Persisting += OnPersisting;
      session.Events.TransactionOpened += OnOpenTransaction;
      session.Events.TransactionCommitted += OnCommitTransaction;
      session.Events.TransactionRollbacked += OnRollbackTransaction;
    }

    private void OnOpenTransaction(object sender, TransactionEventArgs e)
    {
      stack.Push(new TrackingStackFrame());
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
    /// <exception cref="T:System.ArgumentNullException"><paramref name="session"/> is <see langword="null"/>.</exception>
    [ServiceConstructor]
    public SessionTrackingMonitor(Session session, DirectSessionAccessor accessor)
    {
      if (session==null)
        throw new ArgumentNullException("session");
      if (accessor==null)
        throw new ArgumentNullException("accessor");

      this.session = session;
      this.accessor = accessor;

      stack = new Stack<TrackingStackFrame>();
      stack.Push(new TrackingStackFrame());

      Subscribe();
    }
  }
}