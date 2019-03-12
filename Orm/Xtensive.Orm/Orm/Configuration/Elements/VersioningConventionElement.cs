// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.03.14

using System;
using System.Configuration;

namespace Xtensive.Orm.Configuration.Elements
{
  public sealed class VersioningConventionElement : ConfigurationElement
  {
    private const string EntityVersioningPolicyElementName = "entityVersioningPolicy";
    private const string DenyEntitySetOwnerVersionChangeElementName = "denyEntitySetOwnerVersionChange";

    /// <summary>
    /// <see cref="VersioningConvention.EntityVersioningPolicy" copy="true"/>
    /// </summary>
    [ConfigurationProperty(EntityVersioningPolicyElementName, IsRequired = false, IsKey = false, DefaultValue = "Default")]
    public string EntityVersioningPolicy
    {
      get { return (string)this[EntityVersioningPolicyElementName]; }
      set { this[EntityVersioningPolicyElementName] = value; }
    }

    /// <summary>
    /// <see cref="VersioningConvention.DenyEntitySetOwnerVersionChange" copy="true"/>
    /// </summary>
    [ConfigurationProperty(DenyEntitySetOwnerVersionChangeElementName, IsRequired = false, IsKey = false, DefaultValue = false)]
    public bool DenyEntitySetOwnerVersionChange
    {
      get { return (bool)this[DenyEntitySetOwnerVersionChangeElementName]; }
      set { this[DenyEntitySetOwnerVersionChangeElementName] = value; }
    }

    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="VersioningConvention"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public VersioningConvention ToNative()
    {
      var result = new VersioningConvention {
        EntityVersioningPolicy = (EntityVersioningPolicy) Enum.Parse(typeof (EntityVersioningPolicy), EntityVersioningPolicy, true),
        DenyEntitySetOwnerVersionChange = DenyEntitySetOwnerVersionChange
      };
      return result;
    }
  }
}

