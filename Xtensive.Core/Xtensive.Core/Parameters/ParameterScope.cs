// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using System.Linq;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// <see cref="ParameterContext"/> activation scope.
  /// </summary>
  public class ParameterScope : Scope<ParameterContext>,
    IDisposable
  {    
    internal static new ParameterScope CurrentScope {
      [DebuggerStepThrough]
      get { return (ParameterScope) Scope<ParameterContext>.CurrentScope; }
    }

    [DebuggerStepThrough]
    internal object GetValue(Parameter parameter)
    {
      object value;
      if (Context.TryGetValue(parameter, out value))
        return value;
      if (IsNested)
        return ((ParameterScope) OuterScope).GetValue(parameter);
      throw new InvalidOperationException(
        string.Format(Strings.ValueForParameterXIsNotSet, parameter));
    }

    [DebuggerStepThrough]
    internal void SetValue(Parameter parameter, object value)
    {
      Context.SetValue(parameter, value);
    }

    [DebuggerStepThrough]
    internal bool HasValue(Parameter parameter)
    {
      return Context.HasValue(parameter);
    }

    [DebuggerStepThrough]
    internal void Clear(Parameter parameter)
    {
      Context.Clear(parameter);
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
      var wasNested = IsNested;
      var parameterValues = Context.values.ToList();
      Dispose();
      if (wasNested)
        foreach (var pair in parameterValues)
          pair.Key.OnScopeDisposed(pair.Value);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context of this scope.</param>
    [DebuggerStepThrough]
    public ParameterScope(ParameterContext context)
      : base(context)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    [DebuggerStepThrough]
    public ParameterScope()
      : this(new ParameterContext())
    {
    }
  }
}