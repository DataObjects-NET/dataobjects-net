// Copyright (C) 2007-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.29

using System;
using System.Threading;

namespace Xtensive.Orm
{
  /// <summary>
  /// <see cref="Session"/> activation scope. 
  /// </summary>
  public sealed class SessionScope : IDisposable // : Scope<Session>
  {
    private static readonly AsyncLocal<SessionScope> currentScopeAsync = new AsyncLocal<SessionScope>();

    private enum State
    {
      New,
      Active,
      Disposed
    }

    private readonly SessionScope outerScope;
    private Session session;
    private State state;

    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public static Session CurrentSession => currentScopeAsync.Value?.Session;

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public Session Session
    {
      get => state == State.Active ? session : outerScope?.Session;
      internal set {
        if (state != State.New) {
          throw new InvalidOperationException("Can't modify Active or Disposed scope.");
        }

        state = State.Active;
        session = value;
      }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      if (state == State.Disposed) {
        return;
      }

      var currentScope = currentScopeAsync.Value;

      while (currentScope != null) {
        if (currentScope == this) {
          currentScopeAsync.Value = outerScope;
          state = State.Disposed;
          session = null;
          return;
        }

        currentScope = currentScope.outerScope;
      }

      throw new InvalidOperationException(Strings.ExScopeCantBeDisposed);
    }

    // Constructors

    internal SessionScope()
    {
      state = State.New;

      outerScope = currentScopeAsync.Value;
      while (outerScope != null && outerScope.state != State.Active) {
        outerScope = outerScope.outerScope;
      }

      currentScopeAsync.Value = this;
    }

    internal SessionScope(Session session)
      : this()
    {
      this.session = session;
      state = State.Active;
    }
  }
}