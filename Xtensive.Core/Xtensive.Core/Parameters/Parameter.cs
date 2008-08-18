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
  /// Parameter, which have values specific for active <see cref="ParameterContext"/>.
  /// </summary>
  /// <typeparam name="TValue">The type of parameter value.</typeparam>
  public sealed class Parameter<TValue> 
  {
    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>    
    public string Name { get; private set;}

    /// <summary>
    /// Gets or sets the parameter value.
    /// </summary>    
    /// <exception cref="InvalidOperationException"><see cref="ParameterContext"/> is not activated.</exception>
    /// <exception cref="InvalidOperationException">Value for the parameter is not set.</exception>
    public TValue Value
    {
      [DebuggerStepThrough]
      get {
        EnsureContextIsActivated();
        return (TValue) ParameterScope.CurrentScope.GetValue(this);
      }
      [DebuggerStepThrough]
      set {
        EnsureContextIsActivated();
        ParameterScope.CurrentScope.SetValue(this, value);
      }
    }

    private static void EnsureContextIsActivated()
    {
      if (ParameterScope.CurrentScope == null)
        throw new InvalidOperationException(
          string.Format(Resources.Strings.XIsNotActivated, typeof(ParameterContext).Name));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    [DebuggerStepThrough]
    public Parameter()
      : this(string.Empty)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The parameter name.</param>
    [DebuggerStepThrough]
    public Parameter(string name)
    {
      Name = name;
    }
  }
}