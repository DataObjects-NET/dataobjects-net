// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.11.17

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm
{
  public partial class Session
  {
    internal AsyncQueriesManager asyncQueriesManager { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    internal void RemoveFinishedAsyncQuery(Task task)
    {
      asyncQueriesManager.TryRemoveFinishedAsyncQuery(GetLifetimeToken(), task);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="cancellationTokenSource"></param>
    internal void AddNewAsyncQuery(Task task, CancellationTokenSource cancellationTokenSource)
    {
      asyncQueriesManager.AddNewAsyncQuery(GetLifetimeToken(), task, cancellationTokenSource);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    internal void CancelAllAsyncQueriesForToken(StateLifetimeToken token)
    {
      asyncQueriesManager.TryCancelAllAsyncQueriesForToken(token);
    }

    internal void AddNewBlockingCommand(Command command)
    {
      asyncQueriesManager.AddNewBlockingCommand(GetLifetimeToken(), command);
    }

    internal void DisposeBlockingCommandsForToken(StateLifetimeToken token)
    {
      asyncQueriesManager.DisposeBlockingCommandForToken(token);
    }

    internal bool HasIncompletedAsyncQueries()
    {
      return asyncQueriesManager.HasAsyncQueries();
    }

    internal bool HasIncompletedAsyncQueriesForToken(StateLifetimeToken token)
    {
      return asyncQueriesManager.HasAsyncQueriesForToken(token);
    }

    internal bool HasBlockingQueries()
    {
      return asyncQueriesManager.HasBlockingCommands();
    }

    internal bool HasBlockingQueriesForToken(StateLifetimeToken token)
    {
      return asyncQueriesManager.HasBlockingCommandsForToken(token);
    }

    /// <summary>
    /// 
    /// </summary>
    private void CancelAllAsyncQueries()
    {
      asyncQueriesManager.TryCancelAllAsyncQueries();
    }

    private void DisposeBlockingCommands()
    {
      asyncQueriesManager.DisposeBlockingCommands();
    }
  }
}
