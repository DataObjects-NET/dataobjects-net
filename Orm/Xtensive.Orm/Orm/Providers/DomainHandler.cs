// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using Xtensive.IoC;
using Xtensive.Linq;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Linq.MemberCompilation;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.PreCompilation.Correction;
using Xtensive.Orm.Rse.PreCompilation.Correction.ApplyProviderCorrection;
using Xtensive.Orm.Rse.PreCompilation.Optimization;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Sorting;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// <see cref="Orm.Domain"/>-level handler.
  /// </summary>
  public abstract class DomainHandler : DomainBoundHandler
  {
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
    public IEnumerable<IQueryPreprocessor> QueryPreprocessors { get; private set; }

    /// <summary>
    /// Gets the model mapping.
    /// </summary>
    public ModelMapping Mapping { get; private set; }

    /// <summary>
    /// Gets the temporary table manager.
    /// </summary>
    public TemporaryTableManager TemporaryTableManager { get; private set; }

    /// <summary>
    /// Gets the command processor factory.
    /// </summary>
    public CommandProcessorFactory CommandProcessorFactory { get; private set; }

    internal Persister Persister { get; private set; }

    /// <summary>
    /// Builds the mapping schema.
    /// </summary>
    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    public void BuildMapping(SqlExtractionResult model)
    {
      Mapping = ModelMappingBuilder.Build(Handlers, model);
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

    #region Customization members

    /// <summary>
    /// Creates the compiler.
    /// </summary>
    /// <param name="configuration">Compiler configuration to use.</param>
    /// <returns>A new compiler.</returns>
    protected virtual ICompiler CreateCompiler(CompilerConfiguration configuration)
    {
      return new SqlCompiler(Handlers);
    }

    /// <summary>
    /// Creates the <see cref="IPreCompiler"/>.
    /// </summary>
    /// <param name="configuration">Compiler configuration to use.</param>
    /// <returns>A new pre-compiler.</returns>
    protected virtual IPreCompiler CreatePreCompiler(CompilerConfiguration configuration)
    {
      var providerInfo = Handlers.ProviderInfo;

      var applyCorrector = new ApplyProviderCorrector(
        !providerInfo.Supports(ProviderFeatures.Apply));
      var skipTakeCorrector = new SkipTakeCorrector(
        providerInfo.Supports(ProviderFeatures.NativeTake),
        providerInfo.Supports(ProviderFeatures.NativeSkip));
      return new CompositePreCompiler(
        applyCorrector,
        skipTakeCorrector,
        new RedundantColumnOptimizer(),
        new OrderingCorrector(ResolveOrderingDescriptor));
    }

    /// <summary>
    /// Creates the <see cref="IPostCompiler"/>.
    /// </summary>
    /// <param name="configuration">Compiler configuration to use.</param>
    /// <param name="compiler">Currently used compiler instance.</param>
    /// <returns>A new post-compiler.</returns>
    protected virtual IPostCompiler CreatePostCompiler(CompilerConfiguration configuration, ICompiler compiler)
    {
      var result = new CompositePostCompiler(new SqlSelectCorrector(Handlers.ProviderInfo));
      if (configuration.PrepareRequest)
        result.Items.Add(new SqlProviderPreparer(Handlers));
      return result;
    }

    /// <summary>
    /// Gets compiler containers specific to current storage provider.
    /// </summary>
    /// <returns>Compiler containers for current provider.</returns>
    protected virtual IEnumerable<Type> GetProviderCompilerContainers()
    {
      return new[] {
        typeof (NullableCompilers),
        typeof (StringCompilers),
        typeof (DateTimeCompilers),
        typeof (TimeSpanCompilers),
        typeof (MathCompilers),
        typeof (NumericCompilers),
        typeof (DecimalCompilers),
        typeof (GuidCompilers),
        typeof (VbStringsCompilers),
        typeof (VbDateAndTimeCompilers),
      };
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

    private static ProviderOrderingDescriptor ResolveOrderingDescriptor(CompilableProvider provider)
    {
      bool isOrderSensitive = provider.Type==ProviderType.Skip 
        || provider.Type == ProviderType.Take
        || provider.Type == ProviderType.Seek
        || provider.Type == ProviderType.Paging
        || provider.Type == ProviderType.RowNumber;
      bool preservesOrder = provider.Type==ProviderType.Take
        || provider.Type == ProviderType.Skip
        || provider.Type == ProviderType.Seek
        || provider.Type == ProviderType.RowNumber
        || provider.Type == ProviderType.Paging
        || provider.Type == ProviderType.Distinct
        || provider.Type == ProviderType.Alias;
      bool isOrderBreaker = provider.Type == ProviderType.Except
        || provider.Type == ProviderType.Intersect
        || provider.Type == ProviderType.Union
        || provider.Type == ProviderType.Concat
        || provider.Type == ProviderType.Existence;
      bool isSorter = provider.Type==ProviderType.Sort || provider.Type == ProviderType.Index;
      return new ProviderOrderingDescriptor(isOrderSensitive, preservesOrder, isOrderBreaker, isSorter);
    }

    #endregion


    // Initialization

    internal void BuildHandlers()
    {
      TemporaryTableManager = Handlers.Create<TemporaryTableManager>();
      CommandProcessorFactory = Handlers.Create<CommandProcessorFactory>();
      var persistRequestBuilder = Handlers.Create<PersistRequestBuilder>();
      Persister = new Persister(Handlers, persistRequestBuilder);
    }

    public void InitializeServices()
    {
      BuildMemberCompilerProviders();
      BuildCompilationService();
      BuildQueryPreprocessors();
    }
  }
}