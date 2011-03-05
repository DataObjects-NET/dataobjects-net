// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.13

using System;

namespace Xtensive.Internals.DocTemplates
{
  /// <summary>
  /// Class with static <see cref="Default"/> documentation template.
  /// </summary>
  /// <remarks>
  /// <para id="About">
  /// This class has default instance - use its <see cref="Default"/>
  /// property to get it.
  /// </para>
  /// </remarks>
  public class HasStaticDefaultDocTemplate
  {
    /// <summary>
    /// Gets the default instance of <see cref="HasStaticDefaultDocTemplate"/>.
    /// </summary>
    public static HasStaticDefaultDocTemplate Default {
      get {
        throw new NotImplementedException();
      }
    }
  }
}