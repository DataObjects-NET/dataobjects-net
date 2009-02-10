// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using System.Reflection;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Attribute for specifying method which acts as compiler
  /// for desired <see cref="TargetMethod"/> of <see cref="TargetType"/>.
  /// <see cref="MemberCompilerProvider{T}"/> scans for this attributes
  /// via <see cref="MemberCompilerProvider{T}.RegisterCompilers(Type)"/>  method and registers them.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public class CompilerAttribute: Attribute
  {
    /// <summary>
    /// Gets or sets the kind of the target.
    /// </summary>
    /// <value>The kind of the target.</value>
    public TargetKind TargetKind { get; set; }

    /// <summary>
    /// Gets or sets the target method.
    /// </summary>
    /// <value>The target method.</value>
    public string TargetMethod { get; set; }

    /// <summary>
    /// Gets or sets the type of the target.
    /// The type should be either non-generic type or open generic type.
    /// </summary>
    /// <value>The type of the target.</value>
    public Type TargetType { get; set; }

    /// <summary>
    /// Gets or sets the generic params count.
    /// This affects only generic methods not generic types.
    /// </summary>
    /// <value>The generic params count.</value>
    public int GenericParamsCount { get; set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="targetType">The type of the target.</param>
    public CompilerAttribute(Type targetType)
    {
      TargetType = targetType;
      TargetKind = TargetKind.Method;
      TargetMethod = null;
      GenericParamsCount = 0;
    }
  }
}
