// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.30

using System;


namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// An attribute providing persistence-related information for the assembly.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class,
    AllowMultiple = false, Inherited = false)]
  public class AssemblyInfoAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the name of the assembly to use in <see cref="UpgradeHandler"/>.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the version of the assembly to use in <see cref="UpgradeHandler"/>.
    /// </summary>
    public string Version { get; set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">The name of the assembly.</param>
    public AssemblyInfoAttribute(string name)
    {
      Name = name;
      Version = null;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">The name of the assembly.</param>
    /// <param name="version">The version of the assembly.</param>
    public AssemblyInfoAttribute(string name, string version)
    {
      Name = name;
      Version = version;
    }
  }
}