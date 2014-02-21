// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System;
using JetBrains.Annotations;

namespace Xtensive.Orm.Weaving
{
  /// <summary>
  /// Identifies assembly processed by DataObjects.Net weaver.
  /// You should not use this attribute directly.
  /// It is automatically applied to your assembly when needed.
  /// </summary>
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
  [UsedImplicitly]
  public sealed class ProcessedByWeaverAttribute : Attribute
  {
  }
}
