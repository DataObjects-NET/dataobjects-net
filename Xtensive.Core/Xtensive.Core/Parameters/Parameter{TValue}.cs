// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Diagnostics;

namespace Xtensive.Core.Parameters
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
    public new TValue Value
    {
      [DebuggerStepThrough]
      get {
        return (TValue) GetValue();
      }
      [DebuggerStepThrough]
      set {
        SetValue(value);
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
      : base(name)
    {
    }
  }
}