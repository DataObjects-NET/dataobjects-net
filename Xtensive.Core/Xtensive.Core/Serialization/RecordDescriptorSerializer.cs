// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.24

namespace Xtensive.Core.Serialization
{
  public class RecordDescriptorSerializer : SerializerBase<RecordDescriptor>
  {
    public override RecordDescriptor CreateObject(SerializationData data, SerializationContext context)
    {
      return new RecordDescriptor(data.GetValue<string>("Assembly"), data.GetValue<string>("Type"));
    }

    public override void GetObjectData(RecordDescriptor obj, SerializationData data, SerializationContext context)
    {
      data.AddValue("Type", obj.FullTypeName);
      data.AddValue("Assembly", obj.AssemblyName);
    }

    public override void SetObjectData(RecordDescriptor obj, SerializationData data, SerializationContext context)
    {
    }

    public RecordDescriptorSerializer(ISerializerProvider provider)
      : base(provider)
    {
    }
  }
}