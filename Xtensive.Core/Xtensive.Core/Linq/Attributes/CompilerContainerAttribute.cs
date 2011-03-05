// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.30

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Linq
{
  /// <summary>
  /// Attribute for specifying user defined extension type.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class CompilerContainerAttribute : Attribute
  {
    /// <summary>
    /// Gets the type value.
    /// </summary>
    public Type ExtensionType { get; private set; }

    /// <summary>
    /// Gets the conflict handling method value.
    /// </summary>
    public ConflictHandlingMethod ConflictHandlingMethod { get; private set; }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="extensionType">The type.</param>
    /// <param name="conflictHandlingMethod">The conflict handling method.</param>
    public CompilerContainerAttribute(Type extensionType, ConflictHandlingMethod conflictHandlingMethod)
    {
      ExtensionType = extensionType;
      ConflictHandlingMethod = conflictHandlingMethod;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="extensionType">The type.</param>
    public CompilerContainerAttribute(Type extensionType)
    {
      ExtensionType = extensionType;
      ConflictHandlingMethod = ConflictHandlingMethod.Default;
    }
  }
}