// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.05.15

using System;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberListBinding"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableMemberListBinding : SerializableMemberBinding
  {
    /// <summary>
    /// <see cref="MemberListBinding.Initializers"/>
    /// </summary>
    public SerializableElementInit[] Initializers;
  }
}