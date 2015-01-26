// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.11.17

#if NET45
using System;
using System.Threading.Tasks;
using System.Threading;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm
{
  public partial class Session
  {
    internal AsyncQueriesManager AsyncQueriesManager { get; private set; }

    /// <summary>
    /// Unbounds information about completed asynchronous query from current <see cref="StateLifetimeToken">lifetime token</see>.
    /// </summary>
    /// <param name="task">Completed task.</param>
    internal void RemoveFinishedAsyncQuery(Task task)
    {
      AsyncQueriesManager.TryRemoveFinishedAsyncQuery(GetLifetimeToken(), task);
    }

    /// <summary>
    /// Bounds information about incompleted token to current <see cref="StateLifetimeToken">lifetime token</see>.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="cancellationTokenSource"></param>
    internal void AddNewAsyncQuery(Task task, CancellationTokenSource cancellationTokenSource)
    {
      AsyncQueriesManager.AddNewAsyncQuery(GetLifetimeToken(), task, cancellationTokenSource);
    }

    /// <summary>
    /// Cancels all incompleted asynchronous queries for specified <see cref="StateLifetimeToken">lifetime token</see>.
    /// </summary>
    /// <param name="token"><see cref="StateLifetimeToken"/></param>
    internal void CancelAllAsyncQueriesForToken(StateLifetimeToken token)
    {
      AsyncQueriesManager.TryCancelAllAsyncQueriesForToken(token);
    }

    /// <summary>
    /// Adds new blocking command for current <see cref="StateLifetimeToken">lifetime token</see>.
    /// </summary>
    /// <param name="command">Blocking command</param>
    internal void AddNewBlockingCommand(Command command)
    {
      AsyncQueriesManager.AddNewBlockingCommand(GetLifetimeToken(), command);
    }

    /// <summary>
    /// Disposes all registered blocking commands for specified <see cref="StateLifetimeToken">lifetime token</see>.
    /// </summary>
    /// <param name="token"><see cref="StateLifetimeToken">Lifetime token</see> to register.</param>
    internal void DisposeBlockingCommandsForToken(StateLifetimeToken token)
    {
      AsyncQueriesManager.DisposeBlockingCommandForToken(token);
    }

    /// <summary>
    /// Checks existance of incompleted asynchronous queries for specified <see cref="StateLifetimeToken">lifetime token</see>.
    /// </summary>
    /// <returns>Returns <see langword="true"/> if there is incompleted asyncronous queries, otherwise, returns <see langword="false"/>.</returns>
    internal bool HasIncompletedAsyncQueries()
    {
      return AsyncQueriesManager.HasAsyncQueries();
    }

    /// <summary>
    /// Checks existance of incompleted asynchronous queries for specified <see cref="StateLifetimeToken">lifetime token</see>.
    /// </summary>
    /// <param name="token"><see cref="StateLifetimeToken">Lifetime token to check.</see></param>
    /// <returns>Returns <see langword="true"/> if there is incompleted asyncronous queries, otherwise, returns <see langword="false"/>.</returns>
    internal bool HasIncompletedAsyncQueriesForToken(StateLifetimeToken token)
    {
      return AsyncQueriesManager.HasAsyncQueriesForToken(token);
    }

    /// <summary>
    /// Checks existance of blocking readers of results of asynchronous queries.
    /// </summary>
    /// <returns>Returns <see langword="true"/> if there are blocking commands, otherwise, returns <see langword="false"/>.</returns>
    internal bool HasBlockingQueries()
    {
      return AsyncQueriesManager.HasBlockingCommands();
    }

    /// <summary>
    /// Checks existance of blocking commands of results of asynchronous queries for specified <see cref="StateLifetimeToken">lifetime token</see>.
    /// </summary>
    /// <param name="token"><see cref="StateLifetimeToken">Lifetime token to check.</see></param>
    /// <returns>Returns <see langword="true"/> if there are blocking commands, otherwise, returns <see langword="false"/>.</returns>
    internal bool HasBlockingCommandsForToken(StateLifetimeToken token)
    {
      return AsyncQueriesManager.HasBlockingCommandsForToken(token);
    }

    private void CancelAllAsyncQueries()
    {
      AsyncQueriesManager.TryCancelAllAsyncQueries();
    }

    private void DisposeBlockingCommands()
    {
      AsyncQueriesManager.DisposeBlockingCommands();
    }

    internal void EnsureAllAsyncQueriesFinished(StateLifetimeToken token, string errorMessage)
    {
      if (HasIncompletedAsyncQueriesForToken(token))
        throw new InvalidOperationException(errorMessage);
    }

    internal void EnsureAllAsyncQueriesFinished(string errorMessage)
    {
      if (HasIncompletedAsyncQueries())
        throw new InvalidOperationException(errorMessage);
    }

    internal void EnsureAllCommandsDisposed(StateLifetimeToken token, string errorMessage)
    {
      if (HasBlockingCommandsForToken(token))
        throw new InvalidOperationException(errorMessage);
    }
  }
}
#endif
