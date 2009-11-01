// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

namespace Xtensive.Core.Serialization.Binary
{
  public class ValueSerializationScope: Scope<IValueSerializer>
  {
    public static IValueSerializer CurrentSerializer {
      get {
        return CurrentContext ?? BinarySerializer.Instance;
      }
    }

    public IValueSerializer Serializer
    {
      get { return Context; }
    }

    internal new ValueSerializationScope OuterScope
    {
      get { return (ValueSerializationScope)base.OuterScope; }
    }

    public override void Activate(IValueSerializer newContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(newContext, "newContext");
      base.Activate(newContext);
    }


    // Constructors

    public ValueSerializationScope(IValueSerializer context) 
      : base(context)
    {
    }

    public ValueSerializationScope() 
    {
    }
  }
}