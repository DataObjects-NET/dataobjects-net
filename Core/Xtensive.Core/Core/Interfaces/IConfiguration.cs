// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.22

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Core
{
  /// <summary>
  /// A configuration contract (for <see cref="IConfigurable{TConfiguration}"/>).
  /// </summary>
  /// <remarks>
  /// <para id="Ctor"><see cref="ParameterlessCtorClassDocTemplate"/></para>
  /// </remarks>
  public interface IConfiguration: 
    ILockable,
    ICloneable
  {
    /// <summary>
    /// Validates the configuration.
    /// Should always be invoked by <see cref="ILockable.Lock(bool)"/> method 
    /// before actually locking the configuration.
    /// </summary>
    void Validate();
  }
}