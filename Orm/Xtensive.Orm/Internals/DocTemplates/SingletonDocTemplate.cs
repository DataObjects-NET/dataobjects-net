// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.13

using System;

namespace Xtensive.Internals.DocTemplates
{
  /// <summary>
  /// Singleton documentation template.
  /// </summary>
  /// <remarks>
  /// <para id="About">
  /// This class is a singleton - use its <see cref="Instance"/>
  /// property to get the only instance of it.
  /// </para>
  /// </remarks>
  public class SingletonDocTemplate
  {
    /// <summary>
    /// Gets the only instance of <see cref="SingletonDocTemplate"/>.
    /// </summary>
    public static SingletonDocTemplate Instance {
      get {
        throw new NotImplementedException();
      }
    }
  }
}