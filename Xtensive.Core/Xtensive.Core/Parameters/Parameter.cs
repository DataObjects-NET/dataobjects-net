// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// Parameter - an object identifying its value in active <see cref="ParameterContext"/>.
  /// </summary>
  public class Parameter
  {
    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>    
    public string Name { get; private set;}

    /// <summary>
    /// Gets or sets the value of the parameter.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="ParameterContext"/> is not activated.</exception>
    /// <exception cref="InvalidOperationException">Value for the parameter is not set.</exception>
    public object Value {
      [DebuggerStepThrough]
      get { return GetValue(); }
      [DebuggerStepThrough]
      set { SetValue(value); }
    }

    /// <summary>
    /// Gets a value indicating whether this instance has value.
    /// </summary>
    public bool HasValue {
      get { return GetCurrentScope().HasValue(this); }
    }

    /// <summary>
    /// Gets the value of the parameter.
    /// </summary>
    /// <returns>Parameter value.</returns>
    /// <exception cref="InvalidOperationException"><see cref="ParameterContext"/> is not activated.</exception>
    /// <exception cref="InvalidOperationException">Value for the parameter is not set.</exception>
    public object GetValue()
    {
      return GetCurrentScope().GetValue(this);
    }

    /// <summary>
    /// Sets the value of the parameter.
    /// </summary>
    /// <param name="value">The new value.</param>
    /// <exception cref="InvalidOperationException"><see cref="ParameterContext"/> is not activated.</exception>    
    public void SetValue(object value)
    {
      GetCurrentScope().SetValue(this, value);
    }

    /// <summary>
    /// Clears parameter's value.
    /// </summary>
    public void Clear()
    {
      GetCurrentScope().Clear(this);
    }

    /// <summary>
    /// Called on leaving the scope.
    /// </summary>
    /// <param name="parameterScopeValue">The parameter scope value.</param>
    internal virtual void OnScopeDisposed(object parameterScopeValue)
    {
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }

    #region Private methods

    /// <exception cref="Exception"><see cref="ParameterContext"/> is required.</exception>
    private static ParameterScope GetCurrentScope()
    {
      var currentScope = ParameterScope.CurrentScope;
      if (currentScope == null)
        throw Exceptions.ContextRequired(typeof (ParameterContext), typeof (ParameterScope));
      return currentScope;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    [DebuggerStepThrough]
    public Parameter(string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(name, "name");
      Name = name;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public Parameter()
      : this(string.Empty)
    {
    }
  }
}