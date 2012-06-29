// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm
{
  /// <summary>
  /// Attribute for specifying parameter types of compiled method.
  /// See <see cref="CompilerAttribute"/> for details.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
  public sealed class TypeAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public Type Value { get; private set; }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type.</param>
    public TypeAttribute(Type type)
    {
      Value = type;
    }
  }
}
