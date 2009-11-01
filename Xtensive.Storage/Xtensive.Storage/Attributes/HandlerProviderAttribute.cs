// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.01.21

using System;

namespace Xtensive.Storage
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  public class HandlerProviderAttribute : Attribute, IEquatable<HandlerProviderAttribute>
  {
    private readonly string name;
    private string description;

    public string Name
    {
      get { return name; }
    }

    public string Description
    {
      get { return description; }
      set { description = value; }
    }

    /// <inheritdoc/>
    public bool Equals(HandlerProviderAttribute other)
    {
      if (other==null)
        return false;
      return String.Equals(name, other.name);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null || !(obj is HandlerProviderAttribute))
        return false;
      return Equals((HandlerProviderAttribute)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return name==null ? 0 : name.GetHashCode();
    }

    public HandlerProviderAttribute(string providerName)
    {
      name = providerName;
    }
  }
}