// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.07

using System;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class ServiceInfo: Node
  {
    private DomainInfo domain;
    private readonly Type underlyingType;

    /// <summary>
    /// Gets or sets the underlying system type.
    /// </summary>
    public Type UnderlyingType
    {
      get { return underlyingType; }
    }

    /// <summary>
    /// Gets the <see cref="Domain"/> this instance belongs to.
    /// </summary>
    public DomainInfo Domain
    {
      get { return domain; }
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceInfo"/> class.
    /// </summary>
    /// <param name="type">The underlying type.</param>
    /// <param name="domain">The storage info.</param>
    internal ServiceInfo(DomainInfo domain, Type type)
    {
      this.domain = domain;
      underlyingType = type;
    }
  }
}