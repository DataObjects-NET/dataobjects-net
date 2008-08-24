// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Contains the information about the serialized type.
  /// </summary>
  [Serializable]
  public class RecordDescriptor
  {
    /// <summary>
    /// Gets full name of type.
    /// </summary>
    public string TypeName { get; private set; }

    /// <summary>
    /// Gets full name of the assembly containing the type.
    /// </summary>
    public string AssemblyName { get; private set; }

    /// <inheritdoc/>
    public override string ToString() 
    {
      return string.Format("Assembly: {0}, Type: {1}", AssemblyName, TypeName);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="assemblyName">Name of assembly containing type.</param>
    /// <param name="fullTypeName">Full name of type.</param>
    public RecordDescriptor(string assemblyName, string fullTypeName) 
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(assemblyName, "assemblyName");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(fullTypeName, "fullTypeName");
      AssemblyName = assemblyName;
      TypeName = fullTypeName;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">Type to represent.</param>
    public RecordDescriptor(Type type) 
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      TypeName = type.FullName;
      AssemblyName = type.Assembly.FullName;
    }
  }
}