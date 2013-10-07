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
    private readonly Action<TValue> onOutOfScope;

    /// <summary>
    /// Gets or sets the parameter value.
    /// </summary>    
    /// <exception cref="InvalidOperationException"><see cref="ParameterContext"/> is not activated.</exception>
    /// <exception cref="InvalidOperationException">Value for the parameter is not set.</exception>
    public new TValue Value {
      [DebuggerStepThrough]
      get {
        return (TValue) GetValue();
      }
      [DebuggerStepThrough]
      set {
        SetValue(value);
      }
    }

    internal override void OnScopeDisposed(object parameterScopeValue)
    {
      if (onOutOfScope != null) {
        var value = (TValue)parameterScopeValue;
        onOutOfScope(value);
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
      : this(name, null)
    {}

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="expectedValue">The expected value of this parameter.</param>
    [DebuggerStepThrough]
    public Parameter(TValue expectedValue)
      : this(string.Empty, null, expectedValue)
    {}

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public Parameter(string name, TValue expectedValue)
      : base(name, expectedValue)
    {}


    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="onOutOfScope">Out of scope action. 
    /// Action argument is parameter's value within disposed scope.</param>
    public Parameter(Action<TValue> onOutOfScope)
      : this(string.Empty, onOutOfScope)
    {}

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="onOutOfScope">Out of scope action. 
    /// Action argument is parameter's value within disposed scope.</param>
    /// <param name="expectedValue">The expected value of this parameter.</param>
    public Parameter(Action<TValue> onOutOfScope, TValue expectedValue)
      : this(string.Empty, onOutOfScope, expectedValue)
    {}

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="name">The <see cref="Parameter.Name"/> property value.</param>
    /// <param name="onOutOfScope">Out of scope action. 
    /// Action argument is parameter's value within disposed scope.</param>
    public Parameter(string name, Action<TValue> onOutOfScope)
      : base(name)
    {
      this.onOutOfScope = onOutOfScope;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="name">The <see cref="Parameter.Name"/> property value.</param>
    /// <param name="onOutOfScope">Out of scope action. 
    /// Action argument is parameter's value within disposed scope.</param>
    /// <param name="expectedValue">The expected value of this parameter.</param>
    public Parameter(string name, Action<TValue> onOutOfScope, TValue expectedValue)
      : base(name, expectedValue)
    {
      this.onOutOfScope = onOutOfScope;
    }
  }
}