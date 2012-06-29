// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;

namespace Xtensive.Orm {

  /// <summary>
  /// Flags for specifying kind of compilable members.
  /// You should provide only one flag possibly OR'ed with <see cref="Static"/> flag.
  /// </summary>
  [Flags]
  public enum TargetKind
  {
    /// <summary>
    /// Compilable member is a property getter.
    /// </summary>
    PropertyGet = 0x1,
    /// <summary>
    /// Compilable member is as property setter.
    /// </summary>
    PropertySet = 0x2,
    /// <summary>
    /// Compilable member is a field (compiler translates read access).
    /// </summary>
    Field = 0x4,
    /// <summary>
    /// Compilable member is a regular method.
    /// </summary>
    Method = 0x8,
    /// <summary>
    /// Compilable member is a constuctor.
    /// You should NOT specify <see cref="Static"/> flag with this flag.
    /// </summary>
    Constructor = 0x10,
    /// <summary>
    /// Compilable member is <see langword="static"/>.
    /// </summary>
    Static = 0x20,
    /// <summary>
    /// Compilable member is an overloaded operator.
    /// </summary>
    Operator = Static | Method
  }
}