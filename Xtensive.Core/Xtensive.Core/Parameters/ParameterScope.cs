// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// Scope where assigned parameters values are actual.
  /// </summary>
  public class ParameterScope : Scope<ParameterContext>
  {
    private readonly Hashtable values = new Hashtable();

    /// <summary>
    /// Gets the current <see cref="ParameterScope"/>.
    /// </summary>
    public static new ParameterScope CurrentScope 
    {
      get {
        return (ParameterScope) Scope<ParameterContext>.CurrentScope;
      }
    }
    
    internal static new ParameterContext CurrentContext 
    {
      get {
        return Scope<ParameterContext>.CurrentContext;
      }
    }

    internal T GetValue<T>(Parameter<T> parameter)
    {
      if (values.ContainsKey(parameter))
        return (T) values[parameter];

      if (IsNested)
        return ((ParameterScope) OuterScope).GetValue(parameter);

      throw new InvalidOperationException(
        string.Format(Resources.Strings.ValueForParameterXIsNotSet, parameter.Name));
    }

    internal void SetValue<T>(Parameter<T> parameter, T value)
    {
      values[parameter] = value;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context of this scope.</param>
    public ParameterScope(ParameterContext context)
      : base(context)
    {      
    }
  }
}