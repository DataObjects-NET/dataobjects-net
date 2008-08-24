// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System.IO;

namespace Xtensive.Core.Serialization.Binary
{
  public class ValueSerializationScope : Scope<IValueSerializer<Stream>>
  {
    public static IValueSerializer<Stream> CurrentSerializer {
      get { return CurrentContext ?? BinarySerializer.Instance; }
    }

    public IValueSerializer<Stream> Serializer {
      get { return Context; }
    }

    internal new ValueSerializationScope OuterScope {
      get { return (ValueSerializationScope) base.OuterScope; }
    }

    public override void Activate(IValueSerializer<Stream> newContext) {
      ArgumentValidator.EnsureArgumentNotNull(newContext, "newContext");
      base.Activate(newContext);
    }

    // Constructors

    public ValueSerializationScope(IValueSerializer<Stream> context)
      : base(context) {}

    public ValueSerializationScope() {}
  }
}