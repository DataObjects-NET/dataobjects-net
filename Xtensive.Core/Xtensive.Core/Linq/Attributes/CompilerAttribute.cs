﻿// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Attribute for specifying method which acts as compiler
  /// for desired <see cref="TargetMember"/> of <see cref="TargetType"/>.
  /// <see cref="MemberCompilerProvider{T}"/> scans for this attributes
  /// via <see cref="MemberCompilerProvider{T}.RegisterCompilers(Type)"/>  method and registers them.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public class CompilerAttribute: Attribute
  {
    /// <summary>
    /// Gets or sets the type of the target.
    /// The type should be either non-generic type or open generic type.
    /// </summary>
    /// <value>The type of the target.</value>
    public Type TargetType { get; private set; }

    /// <summary>
    /// Gets or sets the target member.
    /// </summary>
    /// <value>The target member.</value>
    public string TargetMember { get; private set; }

    /// <summary>
    /// Gets or sets the kind of the target.
    /// </summary>
    /// <value>The kind of the target.</value>
    public TargetKind TargetKind { get; private set; }

    /// <summary>
    /// Gets or sets the generic params count.
    /// This affects only generic methods not generic types.
    /// </summary>
    /// <value>The generic params count.</value>
    public int GenericParamsCount { get; private set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="targetType">Type of the target.</param>
    /// <param name="targetMember">The target method.</param>
    public CompilerAttribute(Type targetType, string targetMember)
    {
      TargetType = targetType;
      TargetMember = targetMember;
      TargetKind = TargetKind.Method;
      GenericParamsCount = 0;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="targetType">Type of the target.</param>
    /// <param name="targetMember">The target member.</param>
    /// <param name="targetKind">Kind of the target.</param>
    public CompilerAttribute(Type targetType, string targetMember, TargetKind targetKind)
    {
      TargetType = targetType;
      TargetMember = targetMember;
      TargetKind = targetKind;
      GenericParamsCount = 0;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="targetType">Type of the target.</param>
    /// <param name="targetMember">The target member.</param>
    /// <param name="genericParamsCount">The generic params count.</param>
    public CompilerAttribute(Type targetType, string targetMember, int genericParamsCount)
    {
      TargetType = targetType;
      TargetMember = targetMember;
      TargetKind = TargetKind.Method;
      GenericParamsCount = genericParamsCount;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="targetType">Type of the target.</param>
    /// <param name="targetMember">The target member.</param>
    /// <param name="targetKind">Kind of the target.</param>
    /// <param name="genericParamsCount">The generic params count.</param>
    public CompilerAttribute(Type targetType, string targetMember, TargetKind targetKind, int genericParamsCount)
    {
      TargetType = targetType;
      TargetMember = targetMember;
      TargetKind = targetKind;
      GenericParamsCount = genericParamsCount;
    }
  }
}
