// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.IoC;
using Xtensive.Core.Linq;
using Xtensive.Core.Sorting;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Services;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// <see cref="Storage.Domain"/>-level handler.
  /// </summary>
  public abstract class DomainHandler : InitializableHandlerBase
  {
    private readonly object syncRoot = new object();
    private Dictionary<Type, IMemberCompilerProvider> memberCompilerProviders = new Dictionary<Type, IMemberCompilerProvider>();
    private IQueryPreprocessor[] queryPreprocessors;

    /// <summary>
    /// Gets the domain this handler is bound to.
    /// </summary>
    public Domain Domain { get; private set; }

    /// <summary>
    /// Gets the information about provider's capabilities.
    /// </summary>
    public ProviderInfo ProviderInfo { get; private set; }

    /// <summary>
    /// Gets the <see cref="Rse.Compilation.CompilationContext"/>
    /// associated with the domain.
    /// </summary>
    public CompilationContext CompilationContext { get; private set; }

    /// <summary>
    /// Builds the <see cref="ICompiler"/>.
    /// Invoked from <see cref="Initialize"/>.
    /// </summary>
    public ICompiler ServerSideCompiler { get; protected set; }

    /// <summary>
    /// Gets the storage location.
    /// </summary>
    public Location StorageLocation { get; protected set; }

    /// <summary>
    /// Gets the ordered sequence of query preprocessors to apply to any LINQ query.
    /// </summary>
    /// <returns>The ordered sequence of query preprocessors to apply to any LINQ query.</returns>
    /// <exception cref="InvalidOperationException">Cyclic dependency in query preprocessor graph
    /// is detected.</exception>
    public virtual IQueryPreprocessor[] GetQueryPreprocessors()
    {
      if (queryPreprocessors==null) lock (syncRoot) if (queryPreprocessors==null) {
        var unordered = Domain.Services.GetAll<IQueryPreprocessor>();
        var ordered = TopologicalSorter.Sort(unordered, 
          (first, second) => second.IsDependentOn(first));
        if (ordered==null)
          throw new InvalidOperationException(Strings.ExCyclicDependencyInQueryPreprocessorGraphIsDetected);
        queryPreprocessors = ordered.ToArray();
      }
      return queryPreprocessors;
    }

    /// <summary>
    /// Gets the member compiler provider by its type parameter.
    /// </summary>
    /// <typeparam name="T">The type of member compiler provider type parameter.</typeparam>
    /// <returns>
    /// Found member compiler provider;
    /// <see langword="null"/>, if it was not found.
    /// </returns>
    public IMemberCompilerProvider<T> GetMemberCompilerProvider<T>()
    {
      return (IMemberCompilerProvider<T>) memberCompilerProviders[typeof (T)];
    }

    // Abstract methods

    /// <summary>
    /// Creates the compiler.
    /// </summary>
    /// <returns>A new compiler.</returns>
    protected abstract ICompiler CreateCompiler();

    /// <summary>
    /// Creates the <see cref="IPreCompiler"/>.
    /// </summary>
    /// <returns>A new pre-compiler.</returns>
    protected abstract IPreCompiler CreatePreCompiler();

    /// <summary>
    /// Creates the <see cref="IPostCompiler"/>.
    /// </summary>
    /// <param name="compiler">Currently used compiler instance.</param>
    /// <returns>A new post-compiler.</returns>
    protected abstract IPostCompiler CreatePostCompiler(ICompiler compiler);

    /// <summary>
    /// Gets compiler containers specific to current storage provider.
    /// </summary>
    /// <returns>Compiler containers for current provider.</returns>
    protected virtual IEnumerable<Type> GetProviderCompilerContainers()
    {
      return EnumerableUtils<Type>.Empty;
    }

    /// <summary>
    /// Builds the mapping schema.
    /// </summary>
    public abstract void BuildMapping();

    /// <summary>
    /// Creates <see cref="ProviderInfo"/>.
    /// </summary>
    /// <returns></returns>
    protected abstract ProviderInfo CreateProviderInfo();

    #region IoC support (Domain.Services)

    /// <summary>
    /// Creates parent service container 
    /// for <see cref="Storage.Domain.Services"/> container.
    /// </summary>
    /// <returns>Container providing base services.</returns>
    public virtual IServiceContainer CreateBaseServices()
    {
      var registrations = new List<ServiceRegistration>{
        new ServiceRegistration(typeof (Domain), Domain),
        new ServiceRegistration(typeof (DomainConfiguration), Domain.Configuration),
        new ServiceRegistration(typeof (HandlerAccessor), Handlers),
        new ServiceRegistration(typeof (DomainHandler), this),
      };
      AddBaseServiceRegistrations(registrations);
      return new ServiceContainer(registrations);
    }

    /// <summary>
    /// Adds base service registration entries into the list of
    /// registrations used by <see cref="CreateBaseServices"/>
    /// method.
    /// </summary>
    /// <param name="registrations">The list of service registrations.</param>
    protected virtual void AddBaseServiceRegistrations(List<ServiceRegistration> registrations)
    {
      return;
    }

    #endregion

    #region Private \ internal methods

    private void BuildCompilationContext()
    {
      CompilationContext = new CompilationContext(
        CreateCompiler,
        CreatePreCompiler,
        CreatePostCompiler,
        Domain.Configuration.RseQueryCacheSize);
    }

    #endregion


    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
      Domain = BuildingContext.Demand().Domain;
      ProviderInfo = CreateProviderInfo();
      memberCompilerProviders = MemberCompilerProviderBuilder.Build(
        Domain.Configuration, GetProviderCompilerContainers());
      BuildCompilationContext();
    }
  }
}