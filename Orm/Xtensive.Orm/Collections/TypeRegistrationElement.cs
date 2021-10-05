// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Collections
{
  /// <summary>
  /// Type registration element within a configuration file.
  /// </summary>
  public class TypeRegistrationElement : ConfigurationCollectionElementBase
  {
    private const string TypeElementName = "type";
    private const string AssemblyElementName = "assembly";
    private const string NamespaceElementName = "namespace";

    /// <inheritdoc/>
    public override object Identifier {
      get {
        if (!Type.IsNullOrEmpty())
          return Type;
        else
          return (Assembly, Namespace.IsNullOrEmpty() ? null : Namespace);
      }
    }

    /// <summary>
    /// Gets or sets the name of the type to register.
    /// </summary>
    [ConfigurationProperty(TypeElementName, IsRequired = false, DefaultValue = null, IsKey = true)]
    public string Type
    {
      get { return (string)this[TypeElementName]; }
      set { this[TypeElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the assembly where types to register are located.
    /// </summary>
    [ConfigurationProperty(AssemblyElementName, IsRequired = false, DefaultValue = null, IsKey = true)]
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
      if (!Type.IsNullOrEmpty())
        return new TypeRegistration(System.Type.GetType(Type, true));
      else {
        var assembly = System.Reflection.Assembly.Load(Assembly);
        if (Namespace.IsNullOrEmpty())
          return new TypeRegistration(assembly);
        else
          return new TypeRegistration(assembly, Namespace);
      }
    }
  }
}