// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.26

using System;

namespace Xtensive.Core.Aspects
{
  /// <summary>
  /// Marks a method or property of as infrastructure method.
  /// No aspects will be applied to it.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, 
    AllowMultiple = false, Inherited = true)]
  [Serializable]
  public sealed class InfrastructureAttribute : Attribute
  {
  }
}