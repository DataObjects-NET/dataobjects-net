// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.05.15

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
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

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddArray("Initializers", Initializers);
    }

    public SerializableMemberListBinding()
    {
    }

    public SerializableMemberListBinding(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Initializers = info.GetArrayFromSerializableForm<SerializableElementInit>("Initializers");
    }
  }
}