// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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

    private readonly SessionScope outerScope;
    private readonly Session session;
    private bool isDisposed;

    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public static Session CurrentSession => currentScopeAsync.Value?.Session;

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public Session Session => !isDisposed ? session : outerScope?.Session;

    public void Dispose()
    {
      if (isDisposed) {
        return;
      }

      var currentScope = currentScopeAsync.Value;

      while (currentScope != null) {
        if (currentScope == this) {
          currentScopeAsync.Value = outerScope;
          isDisposed = true;
          return;
        }

        currentScope = currentScope.outerScope;
      }

      throw new InvalidOperationException(Strings.ExScopeCantBeDisposed);
    }

    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session to activate.</param>
    public SessionScope(Session session)
    {
      this.session = session;
      outerScope = currentScopeAsync.Value;
      while (outerScope!=null && outerScope.isDisposed) {
        outerScope = outerScope.outerScope;
      }
      currentScopeAsync.Value = this;
    }
  }
}