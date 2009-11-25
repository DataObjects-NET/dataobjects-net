// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.26

using System;

namespace Xtensive.Core.Aspects
{
  /// <summary>
  /// Marks a method or property as infrastructure method or property.
  /// No any aspects will be applied to it by default.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Constructor, 
    AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class InfrastructureAttribute : Attribute
  {
  }
}