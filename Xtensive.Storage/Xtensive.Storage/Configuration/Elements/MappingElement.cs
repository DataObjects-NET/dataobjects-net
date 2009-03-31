// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.30

using System;
using System.Configuration;
using System.Diagnostics;

namespace Xtensive.Storage.Configuration.Elements
{
  [Serializable]
  public class MappingElement : ConfigurationCollectionElementBase
  {
    private const string AssemblyElementName = "assembly";
    private const string TypeElementName = "type";

    /// <inheritdoc/>
    public override object Identifier
    {
      get { return TypeElementName; }
    }

    /// <summary>
    /// <see cref="MappingConfiguration.Assembly" copy="true"/>
    /// </summary>
    [ConfigurationProperty(AssemblyElementName, IsRequired = true, IsKey = false)]
    public string Assembly
    {
      get { return (string)this[AssemblyElementName]; }
      set { this[AssemblyElementName] = value; }
    }

    /// <summary>
    /// <see cref="MappingConfiguration.Type" copy="true"/>
    /// </summary>
    [ConfigurationProperty(TypeElementName, IsRequired = true, IsKey = true)]
    public string Type
    {
      get { return (string)this[TypeElementName]; }
      set { this[TypeElementName] = value; }
    }

    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="MappingConfiguration"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public MappingConfiguration ToNative()
    {
      var result = new MappingConfiguration
      {
        Assembly = Assembly,
        Type = Type
      };
      return result;
    }
  }
}