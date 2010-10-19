// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.27

namespace Xtensive.Collections
{
  /// <summary>
  /// Processes <see cref="TypeRegistration"/>s in <see cref="TypeRegistry"/>.
  /// </summary>
  public interface ITypeRegistrationProcessor
  {
    /// <summary>
    /// Processes the specified type registration.
    /// </summary>
    /// <param name="registry">The type registry.</param>
    /// <param name="registration">The action to process.</param>
    void Process(TypeRegistry registry, TypeRegistration registration);
  }
}