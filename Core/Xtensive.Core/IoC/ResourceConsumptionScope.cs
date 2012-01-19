// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.28

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.IoC
{
  /// <summary>
  /// Represent base scope type for resource consumption pattern implementation.
  /// </summary>
  /// <typeparam name="TResource">The type of the resource.</typeparam>
  public abstract class ResourceConsumptionScope<TResource>: IDisposable
    where TResource: class, IResource
  {
    [ThreadStatic]
    private static ResourceConsumptionScope<TResource> current;

    private ResourceConsumptionScope<TResource> outer;
    private TResource resource;
    private bool isDisposed = false;

    /// <summary>
    /// Gets or sets the current consumption scope.
    /// </summary>
    /// <value>The current consumption scope.</value>
    protected static ResourceConsumptionScope<TResource> Current
    {
      [DebuggerStepThrough]
      get { return current; }
      [DebuggerStepThrough]
      set { current = value; }
    }


    ///<summary>
    /// Gets outer consumption scope if exists.
    ///</summary>
    /// <value>The outer consumption scope or <see langword="null"/> if it does not exist.</value>
    protected ResourceConsumptionScope<TResource> Outer
    {
      [DebuggerStepThrough]
      get { return outer; }
    }

    /// <summary>
    /// Gets or sets the <see cref="IResource"/> object this instance is bound to.
    /// </summary>
    /// <value>The <see cref="IResource"/>.</value>
    /// <exception cref="ObjectDisposedException">Scope is already disposed.</exception>
    protected TResource Resource
    {
      [DebuggerStepThrough]
      get { return resource; }
      [DebuggerStepThrough]
      set 
      { 
        if (isDisposed)
          throw Exceptions.AlreadyDisposed(null);
        resource = value; 
      }
    }

    
    // Constructors

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
    protected ResourceConsumptionScope(TResource resource) 
      : this()
    {
      ArgumentValidator.EnsureArgumentNotNull(resource, "resource");
      this.resource = resource;
      resource.AddConsumer(this);
    }


    // Descructors
    
    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose(bool)" copy="true"/>
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
      if (!isDisposed)
        try {
          resource.RemoveConsumer(this);          
        }
        finally {
          current = outer;
          resource = null;
          outer = null;
          isDisposed = true;
        }
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dtor" copy="true"/>
    /// </summary>
    ~ResourceConsumptionScope()
    {
      Dispose(false);
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