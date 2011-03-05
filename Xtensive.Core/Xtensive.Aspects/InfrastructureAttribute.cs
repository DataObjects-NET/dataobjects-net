// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2008.08.26

using System;

namespace Xtensive.Aspects
{
  /// <summary>
  /// Marks a method or property as infrastructure method or property.
  /// No any aspects will be applied to it by default.
  /// </summary>
  [AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct | 
    AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | 
    AttributeTargets.Constructor, 
    AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class InfrastructureAttribute : Attribute
  {
  }
}