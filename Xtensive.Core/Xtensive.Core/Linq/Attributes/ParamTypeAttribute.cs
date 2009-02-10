// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Attribute for specifying parameter types of compiled method.
  /// See <see cref="CompilerAttribute"/> for details.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
  public class ParamTypeAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the type of the param.
    /// </summary>
    /// <value>The type of the param.</value>
    public Type ParamType { get; private set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type.</param>
    public ParamTypeAttribute(Type type)
    {
      ParamType = type;
    }
  }
}
