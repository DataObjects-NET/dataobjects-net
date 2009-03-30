// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using Xtensive.Core.Collections;
using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// <see cref="Storage.Domain"/>-level handler.
  /// </summary>
  public abstract class DomainHandler : InitializableHandlerBase
  {
    /// <summary>
    /// Gets the domain this handler is bound to.
    /// </summary>
    public Domain Domain { get; private set; }

    /// <summary>
    /// Gets the <see cref="Rse.Compilation.CompilationContext"/>
    /// associated with the domain.
    /// </summary>
    public CompilationContext CompilationContext { get; private set; }


    // Abstract methods

    /// <summary>
    /// Builds the <see cref="ICompiler"/>.
    /// Invoked from <see cref="Initialize"/>.
    /// </summary>
    protected abstract ICompiler BuildCompiler(BindingCollection<object, ExecutableProvider> compiledSources);

    /// <summary>
    /// Builds the <see cref="IOptimizer"/>.
    /// Invoked from <see cref="Initialize"/>.
    /// </summary>
    protected abstract IOptimizer BuildOptimizer();

    /// <summary>
    /// Builds the <see cref="Domain"/> in recreate mode.
    /// </summary>
    public abstract void BuildRecreate();


    /// <summary>
    /// Builds <see cref="Domain"/>s in recycling data while upgrade.
    /// </summary>
    public abstract void BuildRecycling();

    /// <summary>
    /// Checks storage conformity with model.
    /// </summary>
    /// <returns></returns>
    public abstract StorageConformity CheckStorageConformity();

    /// <summary>
    /// Deletes recycling data after upgrade.
    /// </summary>
    public abstract void DeleteRecycledData();

    /// <summary>
    /// Builds <see cref="Domain"/> in Perform mode.
    /// </summary>
    public abstract void BuildPerform();

    /// <summary>
    /// Opens the session with specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of the session to open.</param>
    /// <returns>New <see cref="SessionConsumptionScope"/> object.</returns>
    public SessionConsumptionScope OpenSession(SessionType type)
    {
      if (type == SessionType.System)
        return OpenSession(type, (SessionConfiguration)Domain.Configuration.Sessions.System.Clone());
      return OpenSession(type, (SessionConfiguration)Domain.Configuration.Sessions.Default.Clone());
    }

    /// <summary>
    /// Opens the session with specified <paramref name="type"/> 
    /// and <paramref name="configuration"/>.
    /// </summary>
    /// <param name="type">The type of the session to open.</param>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>New <see cref="SessionConsumptionScope"/> object.</returns>
    public SessionConsumptionScope OpenSession(SessionType type, SessionConfiguration configuration)
    {
      configuration.Type = type;
      return Domain.OpenSession(configuration);
    }


    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
      Domain = BuildingContext.Current.Domain;
      CompilationContext = new CompilationContext(
        () => {
          var compiledSources = new BindingCollection<object, ExecutableProvider>();
          return new ManagingCompiler(
            compiledSources,
            BuildCompiler(compiledSources),
            new ClientCompiler(compiledSources));
        },
        BuildOptimizer);
    }

    /// <summary>
    /// Initializes the system session.
    /// </summary>
    public virtual void InitializeSystemSession()
    {
    }
  }
}