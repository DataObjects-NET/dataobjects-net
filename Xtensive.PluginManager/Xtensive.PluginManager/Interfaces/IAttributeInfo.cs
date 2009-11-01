// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.15

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Describes <see cref="Attribute"/>.
  /// </summary>
  public interface IAttributeInfo
  {
    /// <summary>
    /// Returns a <see cref="ConstructorInfo"/> object representing the constructor that would have initialized the custom attribute.
    /// </summary>
    /// <value>The constructor.</value>
    ConstructorInfo Constructor { get; }

    /// <summary>
    /// Gets the list of positional arguments specified for the constructor that would have initialized the custom attribute.
    /// </summary>
    /// <value>The constructor arguments.</value>
     IList<CustomAttributeTypedArgument> ConstructorArguments { get; }

     /// <summary>
     /// Gets the list of named arguments specified for the attribute instance representes by <see cref="IAttributeInfo"/> object.
     /// </summary>
     /// <value>The named arguments.</value>
    IList<CustomAttributeNamedArgument> NamedArguments { get; }
  }
}