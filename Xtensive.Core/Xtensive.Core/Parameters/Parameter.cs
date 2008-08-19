// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// Parameter - an object identifying its value in active <see cref="ParameterContext"/>.
  /// </summary>
  /// <typeparam name="TValue">The type of parameter value.</typeparam>
  public sealed class Parameter<TValue> : ParameterBase
  {
    /// <summary>
    /// Gets or sets the parameter value.
    /// </summary>    
    /// <exception cref="InvalidOperationException"><see cref="ParameterContext"/> is not activated.</exception>
    /// <exception cref="InvalidOperationException">Value for the parameter is not set.</exception>
    public TValue Value
    {
      [DebuggerStepThrough]
      get {
        var currentScope = ParameterScope.CurrentScope;
        if (currentScope==null)
          throw new InvalidOperationException(
            string.Format(Strings.XIsNotActivated, typeof(ParameterContext).GetShortName()));
        return (TValue) currentScope.GetValue(this);
      }
      [DebuggerStepThrough]
      set {
        var currentScope = ParameterScope.CurrentScope;
        if (currentScope==null)
          throw new InvalidOperationException(
            string.Format(Strings.XIsNotActivated, typeof(ParameterContext).GetShortName()));
        currentScope.SetValue(this, value);
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