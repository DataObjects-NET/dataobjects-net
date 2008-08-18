// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// 
  /// </summary>
  public class ParameterContext : Context<ParameterScope>
  {
    public static ParameterContext Current
    {
      get {
        return ParameterScope.CurrentContext;
      }
    }

    /// <summary>
    /// Gets the value of  the specified parameter in the current scope.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="parameter">The parameter to get value.</param>
    /// <returns>Value of the parameter.</returns>
    /// <exception cref="InvalidOperationException">Value for parameter is not set.</exception>
    public T GetValue<T>(Parameter<T> parameter)
    {
      return ParameterScope.CurrentScope.GetValue(parameter);
    }

    /// <summary>
    /// Sets the value of the specified parameter in the current scope.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="parameter">The parameter to set value.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue<T>(Parameter<T> parameter, T value)
    {
      ParameterScope.CurrentScope.SetValue(parameter, value);
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { 
        return ParameterScope.CurrentContext == this; 
      }
    }

    /// <inheritdoc/>
    protected override ParameterScope CreateActiveScope()
    {
      return new ParameterScope(this);
    }
  }
}