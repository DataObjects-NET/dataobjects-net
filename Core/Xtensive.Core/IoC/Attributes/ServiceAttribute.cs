// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.30

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.IoC
{
  /// <summary>
  /// An attribute describing mapping of service implementation to service.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  public class ServiceAttribute : Attribute
  {
    private bool singleton = true;

    /// <summary>
    /// Gets or sets the type of the service.
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the service.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this service is singleton.
    /// Default value is <see langword="true" />.
    /// </summary>
    public bool Singleton
    {
      get { return singleton; }
      set { singleton = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this is default service implementation or not.
    /// Default value is <see langword="false" />.
    /// </summary>
    public bool Default { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type of the service.</param>
    public ServiceAttribute(Type type)
    {
      Type = type;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type of the service.</param>
    /// <param name="name">The name of the service.</param>
    public ServiceAttribute(Type type, string name)
    {
      Type = type;
      Name = name;
    }
  }
}