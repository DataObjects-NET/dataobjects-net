// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.22

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Configuration
{
  /// <summary>
  /// Base class for <see cref="IConfigurable{TConfiguration}"/> implementors.
  /// </summary>
  /// <remarks>
  /// <para id="Ctor"><see cref="ParameterlessCtorClassDocTemplate"/></para>
  /// </remarks>
  /// <typeparam name="TConfiguration">Type of <see cref="Configuration"/>.</typeparam>
  [Serializable]
  public abstract class ConfigurableBase<TConfiguration>: 
    IConfigurable<TConfiguration>
  {
    private static readonly bool configurationIsStruct = typeof (TConfiguration).IsValueType;
    private bool isConfigured;
    private TConfiguration configuration;

    /// <inheritdoc/>
    public bool IsConfigured
    {
      [DebuggerStepThrough]
      get { return isConfigured; }
    }

    /// <inheritdoc/>
    public TConfiguration Configuration {
      [DebuggerStepThrough]
      get {
        EnsureConfigured();
        return configuration;
      }
      set { Configure(value); }
    }

    /// <summary>
    /// Ensures instance is already configured - i.e. its
    /// <see cref="Configuration"/> property is successfully set, or
    /// <see cref="Configure"/> method is successfully called.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="Configuration"/> property is not initialized.</exception>
    protected void EnsureConfigured()
    {
      if (!isConfigured)
        throw Exceptions.NotInitialized("Configuration");
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"><see cref="Configuration"/> property is already initialized.</exception>
    public virtual void Configure(TConfiguration configuration)
    {
      if (isConfigured)
        throw Exceptions.AlreadyInitialized("Configuration");
      if (!configurationIsStruct)
        ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      this.configuration = configuration;
      try {
        var iConfiguration = this.configuration as IConfiguration;
        var lockable = this.configuration as ILockable;
        if (iConfiguration!=null)
          iConfiguration.Validate();
        if (lockable!=null)
          lockable.Lock();
        isConfigured = true;
        ValidateConfiguration();
        OnConfigured();
      }
      catch {
        this.configuration = default(TConfiguration);
        isConfigured = false;
        throw;
      }
    }

    /// <summary>
    /// Called by <see cref="Configure"/> to validate the
    /// provided configuration.
    /// </summary>
    protected virtual void ValidateConfiguration()
    {
      // Does nothing by default.
    }

    /// <summary>
    /// Called by <see cref="Configure"/> on completion of 
    /// (accepting the) configuration.
    /// </summary>
    protected virtual void OnConfigured()
    {
      // Does nothing by default.
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected ConfigurableBase()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="configuration">Configuration to <see cref="Configure"/> with.</param>
    protected ConfigurableBase(TConfiguration configuration)
    {
      Configure(configuration);
    }
  }
}