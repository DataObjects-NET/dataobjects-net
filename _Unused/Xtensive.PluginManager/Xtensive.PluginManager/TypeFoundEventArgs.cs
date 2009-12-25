// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.13

using System;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Provides data for <see cref="RemoteAssemblyInspector.TypeFound"/> event in <see cref="RemoteAssemblyInspector"/>.
  /// </summary>
  [Serializable]
  public class TypeFoundEventArgs: EventArgs
  {
    private readonly ITypeInfo typeInfo;

    /// <summary>
    /// Gets the <see cref="ITypeInfo"/> object.
    /// </summary>
    /// <value>The type info.</value>
    public ITypeInfo TypeInfo
    {
      get { return typeInfo; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeFoundEventArgs"/> class.
    /// </summary>
    /// <param name="typeInfo">The type info.</param>
    public TypeFoundEventArgs(ITypeInfo typeInfo)
    {
      this.typeInfo = typeInfo;
    }
  }
}