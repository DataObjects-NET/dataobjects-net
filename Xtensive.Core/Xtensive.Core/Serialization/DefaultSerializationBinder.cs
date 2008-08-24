// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.Runtime.Serialization;

namespace Xtensive.Core.Serialization
{
  internal class DefaultSerializationBinder : SerializationBinder
  {
    public override Type BindToType(string assemblyName, string typeName) {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(assemblyName, "assemblyName");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(typeName, "typeName");

      return Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
    }
  }
}