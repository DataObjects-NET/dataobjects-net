// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Diagnostics;


namespace Xtensive.Core
{
  /// <summary>
  /// Parameter - an object identifying its value in active <see cref="ParameterContext"/>.
  /// </summary>
  /// <typeparam name="TValue">The type of parameter value.</typeparam>
  public sealed class Parameter<TValue> : Parameter
  {
    /// <summary>
    /// Gets or sets the parameter value.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="ParameterContext"/> is not activated.</exception>
    /// <exception cref="InvalidOperationException">Value for the parameter is not set.</exception>
    public new TValue Value {
      [DebuggerStepThrough]
      get {
        return (TValue) base.Value;
      }
    }

    // Constructors

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public Parameter()
      : this(string.Empty)
    {
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public Parameter(string name)
      : this(name, default)
    {}

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="expectedValue">The expected value of this parameter.</param>
    [DebuggerStepThrough]
    public Parameter(TValue expectedValue)
      : this(string.Empty, expectedValue)
    {}

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public Parameter(string name, TValue expectedValue)
      : base(name, expectedValue)
    {}
  }
}