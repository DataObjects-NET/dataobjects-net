// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.22

namespace Xtensive.Internals.DocTemplates
{
  /// <summary>
  /// A class with required parameterless constructor documentation template.
  /// </summary>
  /// <remarks>
  /// <para id="Ctor">Any descendant of this type must have 
  /// a parameterless constructor.</para>
  /// </remarks>
  public class ParameterlessCtorClassDocTemplate: ClassDocTemplate
  {
    /// <summary>
    /// <see cref="ClassDocTemplate()"/>
    /// </summary>
    public ParameterlessCtorClassDocTemplate()
    {
    }
  }
}