// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.22

using Xtensive.Internals.DocTemplates;

namespace Xtensive.Core
{
  /// <summary>
  /// Configurable object contract.
  /// </summary>
  /// <remarks>
  /// <para id="Ctor"><see cref="ParameterlessCtorClassDocTemplate"/></para>
  /// </remarks>
  /// <typeparam name="TConfiguration">Type of <see cref="Configuration"/>.</typeparam>
  public interface IConfigurable<TConfiguration>
  {
    /// <summary>
    /// Indicates whether instance is already configured -
    /// i.e. its <see cref="Configuration"/> is already set
    /// or <see cref="Configure"/> method is called.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Gets or sets the configuration of this instance.
    /// </summary>
    TConfiguration Configuration { get; set; }

    /// <summary>
    /// Configures the instance by the specified <paramref name="configuration"/>.
    /// Invoked by <see cref="Configuration"/> property setter.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    void Configure(TConfiguration configuration);
  }
}