// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.08

namespace Xtensive.Core.Serialization
{
  public class ReferenceInterfaceSerializer : SerializerBase<IReference>
  {
    public override IReference CreateObject(SerializationData data, SerializationContext context)
    {
      return context.ReferenceManager.CreateReference(data.GetValue<long>("Value"));
    }

    public override void GetObjectData(IReference obj, SerializationData data, SerializationContext context)
    {
      throw new System.NotImplementedException();
    }

    public override void SetObjectData(IReference obj, SerializationData data, SerializationContext context)
    {
      throw new System.NotImplementedException();
    }

    public ReferenceInterfaceSerializer(ISerializerProvider provider)
      : base(provider)
    {
    }
  }
}