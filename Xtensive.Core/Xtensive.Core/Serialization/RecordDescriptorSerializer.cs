// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.24

namespace Xtensive.Core.Serialization
{
  /// <inheritdoc/>
  public class RecordDescriptorSerializer : ObjectSerializerBase<RecordDescriptor>
  {
    /// <inheritdoc/>
    public override RecordDescriptor CreateObject() {
      return new RecordDescriptor(typeof (RecordDescriptor));
    }

    /// <inheritdoc/>
    public override void GetObjectData(RecordDescriptor obj, SerializationData data) {
      data.AddValue("Type", obj.TypeName);
      data.AddValue("Assembly", obj.AssemblyName);
    }

    /// <inheritdoc/>
    public override RecordDescriptor SetObjectData(RecordDescriptor obj, SerializationData data) {
      var descriptor = new RecordDescriptor(data.GetValue<string>("Assembly"), data.GetValue<string>("Type"));
      SerializationContext.Current.Formatter.RegisterDescriptor(descriptor);
      return null;
    }

    /// <inheritdoc/>
    public override bool IsReferable {
      get { return false; }
    }

    /// <inheritdoc/>
    public RecordDescriptorSerializer(IObjectSerializerProvider provider)
      : base(provider) {}
  }
}