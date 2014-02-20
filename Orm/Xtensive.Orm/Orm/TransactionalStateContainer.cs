// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.05

namespace Xtensive.Orm
{
  /// <summary>
  /// An abstract base class for objects having associated transactional state.
  /// </summary>
  public abstract class TransactionalStateContainer<TState> : SessionBound
  {
    private TState state;

    /// <summary>
    /// Gets lifetime token for this state.
    /// </summary>
    public StateLifetimeToken LifetimeToken { get; private set; }

    /// <summary>
    /// Gets a value indicating whether state is loaded and actual.
    /// </summary>
    public bool IsActual {
      get {
        return LifetimeToken!=null && LifetimeToken.IsActive;
      }
    }

    /// <summary>
    /// Gets the transactional state.
    /// </summary>
    protected TState State {
      get {
        if (!IsActual) {
          Invalidate();
          Refresh();
        }
        return state;
      }
      set {
        Rebind();
        state = value;
      }
    }

    /// <summary>
    /// Ensures the state is actual.
    /// If it really is now, this method does nothing.
    /// Otherwise it calls <see cref="Invalidate"/> method.
    /// </summary>
    protected void EnsureIsActual()
    {
      if (!IsActual)
        Invalidate();
    }

    /// <summary>
    /// Resets the cached transactional state.
    /// </summary>
    protected virtual void Invalidate()
    {
      state = default(TState);
      LifetimeToken = null;
    }

    /// <summary>
    /// Loads/refreshes the state.
    /// </summary>
    protected abstract void Refresh();
    
    /// <summary>
    /// Binds the the state to the current lifetime token.
    /// This method must be invoked on state update.
    /// </summary>
    protected void Rebind()
    {
      LifetimeToken = Session.GetLifetimeToken();
    }


    // Constructors

    /// <inheritdoc/>
    protected TransactionalStateContainer(Session session)
      : base(session)
    {
    }
  }
}