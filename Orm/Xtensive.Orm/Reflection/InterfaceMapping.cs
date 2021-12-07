// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.30

using System;
using System.Reflection;

using ReflectionInterfaceMapping=System.Reflection.InterfaceMapping;
using System.Linq;
using System.Collections.Generic;

namespace Xtensive.Reflection
{
  /// <summary>
  /// Faster <see cref="ReflectionInterfaceMapping"/> analogue.
  /// </summary>
  [Serializable]
  public sealed class InterfaceMapping
  {
    /// <summary>
    /// Gets the target type of this mapping.
    /// </summary>
    public Type TargetType { get; private set; }

    /// <summary>
    /// Gets the interface type of this mapping.
    /// </summary>
    public Type InterfaceType { get; private set; }

    /// <summary>
    /// Gets the type members of this mapping.
    /// </summary>
    public IReadOnlyList<MethodInfo> TargetMethods { get; private set; }

    /// <summary>
    /// Gets the interface members of this mapping.
    /// </summary>
    public IReadOnlyList<MethodInfo> InterfaceMethods { get; private set; }

    
    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="source">The source mapping.</param>
    public InterfaceMapping(ReflectionInterfaceMapping source)
    {
      TargetType = source.TargetType;
      InterfaceType = source.InterfaceType;
      TargetMethods = Array.AsReadOnly(source.TargetMethods);
      InterfaceMethods = Array.AsReadOnly(source.InterfaceMethods);
    }
  }
}