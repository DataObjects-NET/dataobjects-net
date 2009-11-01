// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.15

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Represents an object that describes <see cref="Attribute"/>.
  /// </summary>
  [Serializable]
  public class AttributeInfo: IAttributeInfo
  {
    private readonly ConstructorInfo constructor;
    private readonly IList<CustomAttributeTypedArgument> constructorArguments;
    private readonly IList<CustomAttributeNamedArgument> namedArguments;

    /// <summary>
    /// Returns a <see cref="ConstructorInfo"/> object representing the constructor that would have initialized the custom attribute.
    /// </summary>
    /// <value>The constructor.</value>
    public ConstructorInfo Constructor
    {
      get { return constructor; }
    }

    /// <summary>
    /// Gets the list of positional arguments specified for the constructor that would have initialized the custom attribute.
    /// </summary>
    /// <value>The constructor arguments.</value>
    public IList<CustomAttributeTypedArgument> ConstructorArguments
    {
      get { return constructorArguments; }
    }

    /// <summary>
    /// Gets the list of named arguments specified for the attribute instance representes by <see cref="IAttributeInfo"/> object.
    /// </summary>
    /// <value>The named arguments.</value>
    public IList<CustomAttributeNamedArgument> NamedArguments
    {
      get { return namedArguments; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeInfo"/> class.
    /// </summary>
    /// <param name="data">The <see cref="CustomAttributeData"/> instance.</param>
    public AttributeInfo(CustomAttributeData data)
    {
      constructor = data.Constructor;
      constructorArguments = new ReadOnlyCollection<CustomAttributeTypedArgument>(data.ConstructorArguments);
      namedArguments = new ReadOnlyCollection<CustomAttributeNamedArgument>(data.NamedArguments);
    }
  }
}