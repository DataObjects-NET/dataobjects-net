// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.05

using Xtensive.Core.Aspects;

namespace Xtensive.Storage
{
  /// <summary>
  /// An abstract base class for objects having base associated transactional <see cref="State"/>
  /// of <typeparamref name="TState"/> type.
  /// </summary>
  /// <typeparam name="TState">The type of the transactional state.</typeparam>
  public abstract class TransactionalStateContainer<TState> : TransactionalStateContainer
  {
    private bool isStateLoaded;
    private TState state;

    /// <summary>
    /// Gets the base state.
    /// </summary>
    [Infrastructure]
    protected TState State {
      get {
        if (!IsStateLoaded) {
          BindStateTransaction();
          LoadState();
          isStateLoaded = true;
        }
        return state;
      }
      set {
        if (!IsStateLoaded) {
          BindStateTransaction();
          isStateLoaded = true;
        }
        state = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether base state is loaded or not.
    /// </summary>
    [Infrastructure]
    protected bool IsStateLoaded {
      get {
        EnsureStateIsActual();
        return isStateLoaded;
      }
    }

    [Infrastructure]
    protected abstract TState LoadState();

    /// <inheritdoc/>
    protected override void ResetState()
    {
      isStateLoaded = false;
      state = default(TState);
    }


    // Constructors

    /// <inheritdoc/>
    protected TransactionalStateContainer()
    {
    }

    /// <inheritdoc/>
    protected TransactionalStateContainer(Session session)
      : base(session)
    {
    }
  }
}