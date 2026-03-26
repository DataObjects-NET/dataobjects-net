// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
  public readonly struct InterfaceMapping
  {
    /// <summary>
    /// Gets the target type of this mapping.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Gets the interface type of this mapping.
    /// </summary>
    public Type InterfaceType { get; }

    /// <summary>
    /// Gets the type members of this mapping.
    /// </summary>
    public IReadOnlyList<MethodInfo> TargetMethods { get; }

    /// <summary>
    /// Gets the interface members of this mapping.
    /// </summary>
    public IReadOnlyList<MethodInfo> InterfaceMethods { get; }

    
    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="source">The source mapping.</param>
    public InterfaceMapping(ReflectionInterfaceMapping source)
    {
      TargetType = source.TargetType;
      InterfaceType = source.InterfaceType;
      TargetMethods = source.TargetMethods;
      InterfaceMethods = source.InterfaceMethods;
    }
  }
}
