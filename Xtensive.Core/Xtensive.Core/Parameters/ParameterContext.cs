// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// Provides storing context-specific <see cref="Parameter{TValue}"/>'s values.
  /// </summary>
  public sealed class ParameterContext : Context<ParameterScope>
  {
    private readonly Dictionary<Parameter, object> values = 
      new Dictionary<Parameter, object>();
    private readonly bool isExpectedValuesContext;
    private static readonly ParameterContext expectedValues = new ParameterContext(true);

    /// <summary>
    /// Gets the current <see cref="ParameterContext"/>.
    /// </summary>        
    public static ParameterContext Current {
      [DebuggerStepThrough]
      get { return Scope<ParameterContext>.CurrentContext; }
    }

    /// <summary>
    /// Gets the special singleton <see cref="ParameterContext"/> instance 
    /// exposing <see cref="Parameter.ExpectedValue"/>s of <see cref="Parameter{TValue}"/>s.
    /// </summary>        
    public static ParameterContext ExpectedValues {
      [DebuggerStepThrough]
      get { return expectedValues; }
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

    #region Private \ internal methods

    [DebuggerStepThrough]
    internal bool TryGetValue(Parameter parameter, out object value)
    {
      if (isExpectedValuesContext) {
        value = parameter.ExpectedValue;
        return true;
      }
      return values.TryGetValue(parameter, out value);
    }
    
    [DebuggerStepThrough]
    internal void SetValue(Parameter parameter, object value)
    {
      VerifyThatOperationIsAllowed();
      values[parameter] = value;
    }

    [DebuggerStepThrough]
    internal void Clear(Parameter parameter)
    {
      VerifyThatOperationIsAllowed();
      values.Remove(parameter);
    }

    [DebuggerStepThrough]
    internal bool HasValue(Parameter parameter)
    {
      VerifyThatOperationIsAllowed();
      return values.ContainsKey(parameter);
    }

    internal void NotifyParametersAboutDisposing()
    {
      foreach (var pair in values)
        pair.Key.OnScopeDisposed(pair.Value);
    }

    private void VerifyThatOperationIsAllowed()
    {
      if (isExpectedValuesContext)
        throw new InvalidOperationException(Resources.Strings
          .ExThisOperationIsNotAllowedForParameterContextOperatingWithExpectedValuesOfParameters);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ParameterContext()
    {
    }

    private ParameterContext(bool isExpectedValuesContext)
    {
      this.isExpectedValuesContext = isExpectedValuesContext;
    }
  }
}
