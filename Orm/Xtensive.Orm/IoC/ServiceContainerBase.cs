// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.30

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Reflection;



namespace Xtensive.IoC
{
  /// <summary>
  /// Abstract base class for <see cref="IServiceContainer"/>
  /// implementation.
  /// </summary>
  public abstract class ServiceContainerBase : IServiceContainer
  {
    /// <inheritdoc/>
    public IServiceContainer Parent { get; protected set; }

    /// <inheritdoc/>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s).</exception>
    public virtual IEnumerable<TService> GetAll<TService>()
    {
      return GetAll(typeof(TService)).Cast<TService>();
    }

    /// <inheritdoc/>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s).</exception>
    public virtual IEnumerable<object> GetAll(Type serviceType)
    {
      IEnumerable<object> myServices;
      try {
        myServices = HandleGetAll(serviceType);
      }
      catch (Exception e) {
        throw new ActivationException(FormatActivationErrorMessage(e, serviceType), e);
      }
      if (Parent==null)
        return myServices;
      else
        return myServices.Concat(Parent.GetAll(serviceType));
    }

    /// <inheritdoc/>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s).</exception>
    public virtual TService Get<TService>()
    {
        return (TService) Get(typeof(TService), null);
    }

    /// <inheritdoc/>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s).</exception>
    public virtual TService Get<TService>(string name)
    {
        return (TService) Get(typeof(TService), name);
    }

    /// <inheritdoc/>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s).</exception>
    public virtual object Get(Type serviceType)
    {
        return Get(serviceType, null);
    }

    /// <inheritdoc/>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s).</exception>
    public virtual object GetService(Type serviceType)
    {
      return Get(serviceType, null);
    }

    /// <inheritdoc/>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s).</exception>
    public object Get(Type serviceType, string name)
    {
      object myService;
      try {
        myService = HandleGet(serviceType, name);
      }
      catch (Exception ex) {
        throw new ActivationException(FormatActivationErrorMessage(ex, serviceType, name), ex);
      }
      if (myService!=null || Parent==null)
        return myService;
      else
        return Parent.Get(serviceType, name);
    }

    #region IHasServices methods

    /// <inheritdoc/>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s).</exception>
    public T GetService<T>() 
      where T : class
    {
      return Get<T>();
    }

    #endregion

    #region Protected methods (to override)

    /// <summary>
    /// Actual implementation of <see cref="GetAll"/> method.
    /// </summary>
    /// <param name="serviceType">Type of the services to get.</param>
    /// <returns>The sequence of requested services.</returns>
    protected abstract IEnumerable<object> HandleGetAll(Type serviceType);

    /// <summary>
    /// Actual implementation of <see cref="Get(System.Type,string)"/> method.
    /// </summary>
    /// <param name="serviceType">Type of the service to get.</param>
    /// <param name="name">The name of the service. <see langword="null" />, if name is not specified.</param>
    /// <returns>The requested service.</returns>
    protected abstract object HandleGet(Type serviceType, string name);

    /// <summary>
    /// Formats the activation error message.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="serviceType">Type of the service.</param>
    /// <returns>Formatted error message.</returns>
    protected virtual string FormatActivationErrorMessage(Exception exception, Type serviceType)
    {
      return string.Format(Strings.ExCannotActivateServiceXErrorY, serviceType.GetShortName(), exception);
    }

    /// <summary>
    /// Formats the activation error message.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="serviceType">Type of the service.</param>
    /// <param name="name">The service name.</param>
    /// <returns>Formatted error message.</returns>
    protected virtual string FormatActivationErrorMessage(Exception exception, Type serviceType, string name)
    {
      return string.Format(Strings.ExCannotActivateServiceXWithKeyYErrorZ, serviceType.GetShortName(), name, exception);
    }

    #endregion

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected ServiceContainerBase()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent"><see cref="Parent"/> property value.</param>
    protected ServiceContainerBase(IServiceContainer parent)
    {
      Parent = parent;
    }

    // IDisposable implementation

    /// <see cref="DisposableDocTemplate.Dispose()"/>
    public virtual void Dispose()
    {
    }
  }
}