// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Elena Vakhtina
// Created:    2009.03.30

using System;
using JetBrains.Annotations;


namespace Xtensive.Orm
{
  /// <summary>
  /// Attribute for specifying user defined extension type.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  [MeansImplicitUse]
  public sealed class CompilerContainerAttribute : Attribute
  {
    /// <summary>
    /// Gets the target type (i.e. type this compiler translates to).
    /// </summary>
    public Type TargetType { get; private set; }

    /// <summary>
    /// Gets the conflict handling method value.
    /// </summary>
    public ConflictHandlingMethod ConflictHandlingMethod { get; private set; }


    // Constructors

    /// <summary>
    ///   Initializes new instance of this type.
    /// </summary>
    /// <param name="targetType">Target type.</param>
    /// <param name="conflictHandlingMethod">The conflict handling method.</param>
    public CompilerContainerAttribute(Type targetType, ConflictHandlingMethod conflictHandlingMethod)
    {
      TargetType = targetType;
      ConflictHandlingMethod = conflictHandlingMethod;
    }

    /// <summary>
    ///   Initializes new instance of this type.
    /// </summary>
    /// <param name="targetType">Target type.</param>
    public CompilerContainerAttribute(Type targetType)
    {
      TargetType = targetType;
      ConflictHandlingMethod = ConflictHandlingMethod.Default;
    }
  }
}