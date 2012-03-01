// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.IoC;
using Xtensive.Linq;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Sorting;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// <see cref="Orm.Domain"/>-level handler.
  /// </summary>
  public abstract class DomainHandler : InitializableHandlerBase
  {
    private readonly object syncRoot = new object();

    private Dictionary<Type, IMemberCompilerProvider> memberCompilerProviders;

    /// <summary>
    /// Gets the domain this handler is bound to.
    /// </summary>
    public Domain Domain { get { return Handlers.Domain; } }

    /// <summary>
    /// Gets the <see cref="Xtensive.Orm.Rse.Compilation.CompilationService"/>
    /// associated with the domain.
    /// </summary>
    public CompilationService CompilationService { get; private set; }

    /// <summary>
    /// Gets the ordered sequence of query preprocessors to apply to any LINQ query.
    /// </summary>
    /// <value> The ordered sequence of query preprocessors to apply to any LINQ query. </value>
    /// <exception cref="InvalidOperationException">Cyclic dependency in query preprocessor graph
    /// is detected.</exception>
    public IEnumerable<IQueryPreprocessor> QueryPreprocessors { get; private set; }

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
    /// <param name="configuration">Compiler configuration to use.</param>
    /// <returns>A new compiler.</returns>
    protected abstract ICompiler CreateCompiler(CompilerConfiguration configuration);

    /// <summary>
    /// Creates the <see cref="IPreCompiler"/>.
    /// </summary>
    /// <param name="configuration">Compiler configuration to use.</param>
    /// <returns>A new pre-compiler.</returns>
    protected abstract IPreCompiler CreatePreCompiler(CompilerConfiguration configuration);

    /// <summary>
    /// Creates the <see cref="IPostCompiler"/>.
    /// </summary>
    /// <param name="configuration">Compiler configuration to use.</param>
    /// <param name="compiler">Currently used compiler instance.</param>
    /// <returns>A new post-compiler.</returns>
    protected abstract IPostCompiler CreatePostCompiler(CompilerConfiguration configuration, ICompiler compiler);

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

    #region IoC support (Domain.Services)

    /// <summary>
    /// Creates parent service container 
    /// for <see cref="Orm.Domain.Services"/> container.
    /// </summary>
    /// <returns>Container providing base services.</returns>
    public virtual IServiceContainer CreateBaseServices()
    {
      var registrations = new List<ServiceRegistration>{
        new ServiceRegistration(typeof (Domain), Domain),
        new ServiceRegistration(typeof (DomainConfiguration), Domain.Configuration),
        new ServiceRegistration(typeof (HandlerAccessor), Handlers),
        new ServiceRegistration(typeof (NameBuilder), Handlers.NameBuilder),
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
    }

    #endregion

    #region Private / internal methods

    private void BuildCompilationService()
    {
      CompilationService = new CompilationService(CreateCompiler, CreatePreCompiler, CreatePostCompiler);
    }

    private void BuildMemberCompilerProviders()
    {
      memberCompilerProviders = MemberCompilerProviderBuilder.Build(
        Domain.Configuration,
        GetProviderCompilerContainers());
    }

    private void BuildQueryPreprocessors()
    {
      var unordered = Domain.Services.GetAll<IQueryPreprocessor>();
      var ordered = TopologicalSorter.Sort(unordered, (first, second) => second.IsDependentOn(first));
      if (ordered==null)
        throw new InvalidOperationException(Strings.ExCyclicDependencyInQueryPreprocessorGraphIsDetected);
      QueryPreprocessors = ordered;
    }

    #endregion


    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
      BuildMemberCompilerProviders();
      BuildCompilationService();
    }

    public void ConfigureServices()
    {
      BuildQueryPreprocessors();
    }
  }
}