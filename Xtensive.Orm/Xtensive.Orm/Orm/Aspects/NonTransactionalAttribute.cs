// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.08

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// Indicates whether transactional aspect should not be applied to the method, property or constructor.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  [Serializable]
  public class NonTransactionalAttribute : Attribute
  {
  }
}