// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// Provides storing context-specific <see cref="Parameter{TValue}"/>'s values.
  /// </summary>
  public class ParameterContext : Context<ParameterScope>
  {    
    internal readonly Dictionary<Parameter, object> values = 
      new Dictionary<Parameter, object>();

    /// <summary>
    /// Gets the current <see cref="ParameterContext"/>.
    /// </summary>        
    public static ParameterContext Current {
      [DebuggerStepThrough]
      get { return Scope<ParameterContext>.CurrentContext; }
    }

    /// <summary>
    /// Creates the <see cref="ParameterScope"/> associated with <see cref="ParameterExpectedValueContext"/>.
    /// </summary>
    public static ParameterScope CreateExpectedValueScope()
    {
      return new ParameterScope(new ParameterExpectedValueContext());
    }

    #region IContext<...> methods

    /// <inheritdoc/>
    public override bool IsActive {
      [DebuggerStepThrough]
      get { return Current == this; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override ParameterScope CreateActiveScope()
    {
      return new ParameterScope(this);
    }

    #endregion

    #region Internal methods

    [DebuggerStepThrough]
    internal virtual bool TryGetValue(Parameter parameter, out object value)
    {
      return values.TryGetValue(parameter, out value);
    }
    
    [DebuggerStepThrough]
    internal virtual void SetValue(Parameter parameter, object value)
    {
      values[parameter] = value;
    }

    [DebuggerStepThrough]
    internal void Clear(Parameter parameter)
    {
      values.Remove(parameter);
    }

    [DebuggerStepThrough]
    internal bool HasValue(Parameter parameter)
    {
      return values.ContainsKey(parameter);
    }

    #endregion
  }
}
