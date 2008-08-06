// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.06

using System;
using System.Diagnostics;

namespace Xtensive.Storage.Attributes
{
  /// <summary>
  /// Marks a method or property of <see cref="SessionBound"/> type
  /// as infrastructure method.
  /// No aspects will be applied to it.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, 
    AllowMultiple = false, Inherited = true)]
  [Serializable]
  public sealed class InfrastructureAttribute : Attribute
  {
  }
}