// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.IoC;
using Xtensive.Linq;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers.Sql;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Upgrade;
using Xtensive.Sorting;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Threading;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// <see cref="Orm.Domain"/>-level handler.
  /// </summary>
  public abstract class DomainHandler : HandlerBase
  {
    private Dictionary<Type, IMemberCompilerProvider> memberCompilerProviders;
    private ThreadSafeDictionary<PersistRequestBuilderTask, IEnumerable<PersistRequest>> requestCache
      = ThreadSafeDictionary<PersistRequestBuilderTask, IEnumerable<PersistRequest>>.Create(new object());

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
    public IEnumerable<IQueryPreprocessor> QueryPreprocessors { get; private set; }

    /// <summary>
    /// Gets the model mapping.
    /// </summary>
    public ModelMapping Mapping { get; private set; }

    /// <summary>
    /// Gets the SQL request builder.
    /// </summary>
    public PersistRequestBuilder PersistRequestBuilder { get; private set; }

    /// <summary>
    /// Gets the temporary table manager.
    /// </summary>
    public TemporaryTableManager TemporaryTableManager { get; private set; }

    /// <summary>
    /// Gets the command processor factory.
    /// </summary>
    public CommandProcessorFactory CommandProcessorFactory { get; private set; }

    /// <summary>
    /// Gets the partial index filter normalizer.
    /// </summary>
    public PartialIndexFilterNormalizer PartialIndexFilterNormalizer { get; private set; }

    /// <summary>
    /// Builds the mapping schema.
    /// </summary>
    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    public void BuildMapping()
    {
      var sqlModel  = Handlers.SchemaUpgradeHandler.GetExtractedSqlSchema();
      Mapping = ModelMappingBuilder.Build(Handlers, sqlModel);
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

    /// <summary>
    /// Gets the persist request for the specified <paramref name="task"/>.
    /// </summary>
    /// <param name="task">The task to get request from.</param>
    /// <returns>A <see cref="PersistRequest"/> that represents <paramref name="task"/>.</returns>
    public IEnumerable<PersistRequest> GetPersistRequest(PersistRequestBuilderTask task)
    {
      return requestCache.GetValue(task, PersistRequestBuilder.Build);
    }

    #region Customization members

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

    #endregion

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

      PersistRequestBuilder = Handlers.CreateAndInitialize<PersistRequestBuilder>();
      TemporaryTableManager = Handlers.CreateAndInitialize<TemporaryTableManager>();
      CommandProcessorFactory = Handlers.CreateAndInitialize<CommandProcessorFactory>();
      PartialIndexFilterNormalizer = Handlers.CreateAndInitialize<PartialIndexFilterNormalizer>();
    }

    public void ConfigureServices()
    {
      BuildQueryPreprocessors();
    }
  }
}