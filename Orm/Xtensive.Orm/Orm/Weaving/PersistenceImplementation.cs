// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.27

using System;

namespace Xtensive.Orm.Weaving
{
  /// <summary>
  /// Helpers routines for persistence weaver.
  /// You should not use this type manually.
  /// All required invocations are injected automatically.
  /// </summary>
  public static class PersistenceImplementation
  {
    /// <summary>
    /// Processes an attempt to call key setter by throwing <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="typeName">Persistent type name.</param>
    /// <param name="propertyName">Persistent property name.</param>
    public static void HandleKeySet(string typeName, string propertyName)
    {
      throw new NotSupportedException(string.Format(Strings.ExKeyFieldXInTypeYShouldNotHaveSetAccessor, propertyName, typeName));
    }
  }
}