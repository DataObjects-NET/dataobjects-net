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
  /// Suppresses the activation of context of specified <see cref="ContextType"/> 
  /// for the method it is applied on.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
  public sealed class SuppressActivationAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the type of the context to suppress activation for.
    /// </summary>
    /// <remarks>If <see cref="ContextType"/> is <see langword="null"/>
    ///  then context activation for all contexts must be suppressed.
    /// </remarks>
    public Type ContextType { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="contextType">Type of the context to suppress activation for.</param>
    public SuppressActivationAttribute(Type contextType)
    {
      ContextType = contextType;
    }
  }
}