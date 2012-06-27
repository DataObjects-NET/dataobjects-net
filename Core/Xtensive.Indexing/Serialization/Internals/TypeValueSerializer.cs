// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using Xtensive.Threading;

namespace Xtensive.Indexing.Serialization
{
  [Serializable]
  internal sealed class TypeValueSerializer : WrappingValueSerializer<Type, string, string>
  {
    private static ThreadSafeDictionary<Assembly, string> cachedNames =
      ThreadSafeDictionary<Assembly, string>.Create(new object());

    public override Type Deserialize(Stream stream)
    {
      string typeName = BaseSerializer1.Deserialize(stream);
      string assemblyName = BaseSerializer2.Deserialize(stream);
      return SerializationContext.Current.Configuration.Binder.BindToType(assemblyName, typeName);
    }

    public override void Serialize(Stream stream, Type value)
    {
      var configuration = SerializationContext.Current.Configuration;
      BaseSerializer1.Serialize(stream, value.FullName);
      if (configuration.AssemblyStyle==FormatterAssemblyStyle.Full)
        BaseSerializer1.Serialize(stream, value.Assembly.FullName);
      else
        BaseSerializer1.Serialize(stream, 
          cachedNames.GetValue(value.Assembly, a => a.GetName().Name));
    }

    
    // Constructors

    public TypeValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
    }
  }
}