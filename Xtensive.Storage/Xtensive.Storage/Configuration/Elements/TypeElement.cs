// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System;
using System.Configuration;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using R=System.Reflection;

namespace Xtensive.Storage.Configuration.Elements
{
  /// <summary>
  /// Type configuration element within a configuration file.
  /// </summary>
  public class TypeElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string AssemblyElementName = "assembly";
    private const string NamespaceElementName = "namespace";

    /// <inheritdoc/>
    public override object Identifier {
      get {
        return new Pair<string, string>(Assembly, Namespace);
      }
    }

    /// <summary>
    /// Gets or sets the name of the type to register.
    /// </summary>
    [ConfigurationProperty(NameElementName, IsRequired = false, DefaultValue = null, IsKey = true)]
    public string Name
    {
      get { return (string)this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the assembly where types to register are located.
    /// </summary>
    [ConfigurationProperty(AssemblyElementName, IsRequired = true, IsKey = true)]
    public string Assembly
    {
      get { return (string)this[AssemblyElementName]; }
      set { this[AssemblyElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the namespace withing the <see cref="Assembly"/>, 
    /// where types to register are located.
    /// If <see langword="null" /> or <see cref="string.Empty"/>, 
    /// all the persistent types from the <see cref="Assembly"/> will be registered.
    /// </summary>
    [ConfigurationProperty(NamespaceElementName, IsRequired = false, DefaultValue = null, IsKey = true)]
    public string Namespace
    {
      get { return (string)this[NamespaceElementName]; }
      set { this[NamespaceElementName] = value; }
    }

    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="TypeRegistration"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public TypeRegistration ToNative()
    {
      if (Name!=null)
        return new TypeRegistration(Type.GetType(Name));
      else {
        var assembly = R.Assembly.Load(Assembly);
        if (Namespace.IsNullOrEmpty())
          return new TypeRegistration(assembly);
        else
          return new TypeRegistration(assembly, Namespace);
      }
    }
  }
}