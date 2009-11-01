// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.12

using System.Reflection;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Describes type.
  /// </summary>
  public interface ITypeInfo
  {
    /// <summary>
    /// Gets the type's <see cref="System.Type.FullName"/>.
    /// </summary>
    /// <value>The full name of the type.</value>
    string TypeName { get; }

    /// <summary>
    /// Gets the <see cref="System.Reflection.AssemblyName"/> for the assembly that contains this instance.
    /// </summary>
    /// <value>The <see cref="System.Reflection.AssemblyName"/>.</value>
    AssemblyName AssemblyName { get; }

    /// <summary>
    /// Gets the array of <see cref="IAttributeInfo"/> objects for the type.
    /// </summary>
    /// <value>The array of <see cref="IAttributeInfo"/> objects for the type.</value>
    IAttributeInfo[] GetAttributes();
  }
}