// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Collections;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// <see cref="ParameterContext"/> activation scope.
  /// </summary>
  public class ParameterScope : Scope<ParameterContext>
  {    
    internal static new ParameterScope CurrentScope 
    {
      [DebuggerStepThrough]
      get {
        return (ParameterScope) Scope<ParameterContext>.CurrentScope;
      }
    }
    
    [DebuggerStepThrough]
    internal object GetValue(object parameter)
    {
      if (Context.HasValue(parameter))
        return Context.GetValue(parameter);

      if (IsNested)
        return ((ParameterScope) OuterScope).GetValue(parameter);

      throw new InvalidOperationException(
        string.Format(Resources.Strings.ValueForParameterXIsNotSet, parameter));
    }

    [DebuggerStepThrough]
    internal void SetValue(object parameter, object value)
    {
      Context.SetValue(parameter, value);      
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
  }
}