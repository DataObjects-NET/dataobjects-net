// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.16

using System;
using Xtensive.Core;



namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Holds current state of the <see cref="Comparer"/>.
  /// </summary>
  public class ComparisonContext
  {
    /// <summary>
    /// Gets the comparer this context is created for.
    /// </summary>
    public Comparer Comparer { get; private set; }

    /// <summary>
    /// Gets the parent comparison context.
    /// </summary>
    public ComparisonContext Parent { get; private set; }

    /// <summary>
    /// Gets the parent difference comparison context.
    /// </summary>
    public ComparisonContext ParentDifferenceContext { 
      get {
        var current = Parent;
        while (current!=null) {
          if (current.Difference!=Difference)
            return current;
          current = current.Parent;
        }
        return null;
      }
    }

    /// <summary>
    /// Gets the nearest <see cref="ParentDifferenceContext"/> where
    /// <see cref="Difference"/> is of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="Difference"/> to search for.</typeparam>
    /// <returns>
    /// The nearest <see cref="Parent"/> of type <typeparamref name="T"/>, if found;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public ComparisonContext GetParentDifferenceContext<T>()
      where T : Difference
    {
      var current = ParentDifferenceContext;
      while (current!=null) {
        var d = current.Difference as T;
        if (d!=null)
          return current;
        current = current.ParentDifferenceContext;
      }
      return null;
    }

    /// <summary>
    /// Gets or sets the currently processed difference.
    /// </summary>
    public Difference Difference { get; set; }

    /// <summary>
    /// Gets or sets the currently processed property accessor.
    /// </summary>
    public PropertyAccessor PropertyAccessor { get; set; }

    /// <summary>
    /// Activates this instance.
    /// </summary>
    /// <returns>A disposable object deactivating it.</returns>
    /// <exception cref="InvalidOperationException">Invalid context activation sequence.</exception>
    public IDisposable Activate()
    {
      return Activate(true);
    }

    /// <summary>
    /// Activates this instance.
    /// </summary>
    /// <param name="safely">If <see langword="true" />,
    /// a check that <see cref="Parent"/> is active must be performed.</param>
    /// <returns>A disposable object deactivating it.</returns>
    /// <exception cref="InvalidOperationException">Invalid context activation sequence.</exception>
    public IDisposable Activate(bool safely)
    {
      if (safely && Comparer.Context!=Parent)
        throw new InvalidOperationException(Strings.ExInvalidContextActivationSequence);
      var oldContext = Comparer.Context;
      Comparer.Context = this;
      return new Disposable<ComparisonContext, ComparisonContext>(this, oldContext,
        (isDisposing, _this, _oldContext) => {
          if (_this.Comparer.Context!=_this)
            throw new InvalidOperationException(Strings.ExInvalidContextDeactivationSequence);
          _this.Comparer.Context = _oldContext;
        });
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <exception cref="InvalidOperationException">No current Comparer.</exception>
    public ComparisonContext()
    {
      Comparer = Comparer.Current;
      if (Comparer==null)
        throw new InvalidOperationException(Strings.ExNoCurrentComparer);
      Parent = Comparer.Context;
      if (Parent==null)
        return;
      Difference = Parent.Difference;
      PropertyAccessor = Parent.PropertyAccessor;
    }
  }
}