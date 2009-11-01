// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.28

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core
{
  /// <summary>
  /// Represent base scope type for resource consumption pattern implementation.
  /// </summary>
  /// <typeparam name="TResource">The type of the resource.</typeparam>
  public abstract class ResourceConsumptionScope<TResource>: IDisposable
    where TResource: class, IResource
  {
    [ThreadStatic]
    private static ResourceConsumptionScope<TResource> current = null;

    private ResourceConsumptionScope<TResource> outer;
    private TResource resource;

    /// <summary>
    /// Gets or sets the current consumption scope.
    /// </summary>
    /// <value>The current consumption scope.</value>
    protected static ResourceConsumptionScope<TResource> Current
    {
      get { return current; }
      set { current = value; }
    }


    ///<summary>
    /// Gets outer consumption scope if exists.
    ///</summary>
    /// <value>The outer consumption scope or <see langword="null"/> if it does not exist.</value>
    protected ResourceConsumptionScope<TResource> Outer
    {
      get { return outer; }
    }

    /// <summary>
    /// Gets or sets the <see cref="IResource"/> object this instance is bound to.
    /// </summary>
    /// <value>The <see cref="IResource"/>.</value>
    protected TResource Resource
    {
      get { return resource; }
      set { resource = value; }
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    public virtual void Dispose()
    {
      try {
        resource.RemoveConsumer(this);
      }
      finally {
        current = outer;
        resource = null;
        outer = null;
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected ResourceConsumptionScope()
    {
      outer = current;
      current = this;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="resource">The resource.</param>
    protected ResourceConsumptionScope(TResource resource) : this()
    {
      ArgumentValidator.EnsureArgumentNotNull(resource, "resource");
      this.resource = resource;
      resource.AddConsumer(this);
    }
  }


  /// <summary>
  /// Represent base configurable scope type for resource consumption pattern implementation.
  /// </summary>
  /// <typeparam name="TResource">The type of the resource.</typeparam>
  /// <typeparam name="TConfig">The type of the resource configuration.</typeparam>
  public class ResourceConsumptionScope<TResource, TConfig>: ResourceConsumptionScope<TResource>
    where TResource: class, IResource
    where TConfig: class
  {

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected ResourceConsumptionScope()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="resource">The resource.</param>
    protected ResourceConsumptionScope(TResource resource) : base(resource)
    {
    }
  }
}