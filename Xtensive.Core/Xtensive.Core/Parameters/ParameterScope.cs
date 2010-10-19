// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Diagnostics;
using System.Linq;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Resources;

namespace Xtensive.Parameters
{
  /// <summary>
  /// <see cref="ParameterContext"/> activation scope.
  /// </summary>
  public sealed class ParameterScope : Scope<ParameterContext>,
    IDisposable
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static ParameterContext CurrentContext {
      get {
        return Scope<ParameterContext>.CurrentContext;
      }
    }

    /// <summary>
    /// Gets the associated parameter context.
    /// </summary>
    public new ParameterContext Context {
      get {
        return base.Context;
      }
    }

    #region Internal methods

    internal static new ParameterScope CurrentScope {
      [DebuggerStepThrough]
      get { return (ParameterScope) Scope<ParameterContext>.CurrentScope; }
    }

    [DebuggerStepThrough]
    internal bool TryGetValue(Parameter parameter, out object value)
    {
      object result;
      var scope = this;
      bool found = scope.Context.TryGetValue(parameter, out result);
      while (!found && scope.IsNested) {
        scope = (ParameterScope) scope.OuterScope;
        found = scope.Context.TryGetValue(parameter, out result);
      }
      value = result;
      return found;
    }

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    [DebuggerStepThrough]
    internal object GetValue(Parameter parameter)
    {
      object result;
      if (TryGetValue(parameter, out result))
        return result;
      throw new InvalidOperationException(string.Format(
        Strings.ExValueForParameterXIsNotSet, parameter));
    }

    [DebuggerStepThrough]
    internal bool HasValue(Parameter parameter)
    {
      object dummy;
      return TryGetValue(parameter, out dummy);
    }

    [DebuggerStepThrough]
    internal void SetValue(Parameter parameter, object value)
    {
      Context.SetValue(parameter, value);
    }

    [DebuggerStepThrough]
    internal void Clear(Parameter parameter)
    {
      Context.Clear(parameter);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context of this scope.</param>
    [DebuggerStepThrough]
    internal ParameterScope(ParameterContext context)
      : base(context)
    {
    }

    // Disposal

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
      if (!IsNested) {
        Dispose();
        return;
      }
      var currentContext = Context;
      Dispose();
      currentContext.NotifyParametersOnDisposing();
    }
  }
}