// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.30

using System;


namespace Xtensive.IoC
{
  /// <summary>
  /// An attribute tagging default service constructor to use.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
  public sealed class ServiceConstructorAttribute : Attribute
  {
  }
}