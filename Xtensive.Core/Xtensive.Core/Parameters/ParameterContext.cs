// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

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

    [DebuggerStepThrough]
    internal bool TryGetValue(Parameter parameter, out object value)
    {
      return values.TryGetValue(parameter, out value);
    }
    
    [DebuggerStepThrough]
    internal void SetValue(Parameter parameter, object value)
    {
      values[parameter] = value;
    }

    [DebuggerStepThrough]
    internal bool HasValue(Parameter parameter)
    {
      return values.ContainsKey(parameter);
    }

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
  }
}
