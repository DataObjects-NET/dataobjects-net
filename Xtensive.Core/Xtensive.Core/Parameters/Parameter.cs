// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.14

using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// Parameter can have values that is actual within specific <see cref="ParameterScope"/>s.
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
    public TValue Value
    {
      get { return ParameterContext.Current.GetValue(this); }
      set { ParameterContext.Current.SetValue(this, value); }
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public Parameter()
      : this(string.Empty)
    {      
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The parameter name.</param>
    public Parameter(string name)
    {
      Name = name;
    }
  }
}