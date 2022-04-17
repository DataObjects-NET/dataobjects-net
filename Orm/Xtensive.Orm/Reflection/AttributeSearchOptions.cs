// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.06

using System;

namespace Xtensive.Reflection
{
  /// <summary>
  /// Search options for <see cref="AttributeHelper"/> methods.
  /// </summary>
  [Flags]
  public enum AttributeSearchOptions
  {
    /// <summary>
    /// Default options.
    /// The same as <see cref="InheritNone"/>.
    /// </summary>
    Default = InheritNone,
    /// <summary>
    /// Nothing should be inherited.
    /// </summary>
    InheritNone = 0,
    /// <summary>
    /// If no attributes are found on the specified member,
    /// attributes from its base should be inherited.
    /// </summary>
    InheritFromBase = 1,
    /// <summary>
    /// Attributes inherit recursively.
    /// </summary>
    InheritRecursively = 2,
    /// <summary>
    /// Attributes from all the bases should be inherited.
    /// </summary>
    InheritFromAllBase = InheritFromBase | InheritRecursively,
    /// <summary>
    /// If no attributes are found on the specified method,
    /// attributes from the property or event it belongs to should be inherited.
    /// </summary>
    InheritFromPropertyOrEvent = 4,
    /// <summary>
    /// All inheritance options.
    /// </summary>
    InheritAll = 7,
  }
}