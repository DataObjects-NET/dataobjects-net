// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.07.02

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Aspects
{
  /// <summary>
  /// Attribute that points that this method must not be covered
  /// with context activation wrapper.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method)]
  public sealed class SuppressContextActivationAttribute : Attribute
  {
    private Type contextType;

    /// <summary>
    /// Gets or sets the type of the context to suppress activation for.
    /// </summary>
    /// <remarks>If <see cref="ContextType"/> is <see langword="null"/>
    ///  then context activation for all contexts must be suppressed.
    /// </remarks>
    public Type ContextType
    {
      get { return contextType; }
      set { contextType = value; }
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public SuppressContextActivationAttribute()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="contextType">Type of the context to suppress activation for.</param>
    public SuppressContextActivationAttribute(Type contextType)
    {
      ContextType = contextType;
    }
  }
}