// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;

namespace Xtensive.Core.Serialization
{
  [Serializable]
  public class RecordDescriptor
  {
    private readonly string assemblyName;
    private readonly string fullTypeName;

    public string FullTypeName
    {
      get { return fullTypeName; }
    }

    public string AssemblyName
    {
      get { return assemblyName; }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("Assembly: {0}, Type: {1}", AssemblyName, FullTypeName);
    }

    public RecordDescriptor(string assemblyName, string fullTypeName)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(assemblyName, "assemblyName");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(fullTypeName, "fullTypeName");
      this.assemblyName = assemblyName;
      this.fullTypeName = fullTypeName;
    }

    public RecordDescriptor(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      fullTypeName = type.FullName;
      assemblyName = type.Assembly.FullName;
    }
  }
}