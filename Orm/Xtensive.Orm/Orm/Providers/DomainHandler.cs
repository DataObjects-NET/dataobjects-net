// Copyright (C) 2003-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Linq.MemberCompilation;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Transformation;

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
    /// Gets the <see cref="Providers.CompilationService"/>
    /// associated with the domain.
    /// </summary>
    public CompilationService CompilationService { get; private set; }

    /// <summary>
    /// Gets the ordered sequence of query preprocessors to apply to any LINQ query.
    /// </summary>
    public IEnumerable<IQueryPreprocessor> QueryPreprocessors { get; private set; }

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

    internal SearchConditionCompiler GetSearchConditionCompiler()
    {
      return CreateSearchConditionVisitor();
    }

    #region Customization members

    /// <summary>
    /// Creates the compiler.
    /// </summary>
    /// <param name="configuration">Compiler configuration to use.</param>
    /// <returns>A new compiler.</returns>
    protected virtual ICompiler CreateCompiler(CompilerConfiguration configuration)
    {
      return new SqlCompiler(Handlers, configuration);
    }

    /// <summary>
    /// Creates the <see cref="IPreCompiler"/>.
    /// </summary>
    /// <param name="configuration">Compiler configuration to use.</param>
    /// <returns>A new pre-compiler.</returns>
    protected virtual IPreCompiler CreatePreCompiler(CompilerConfiguration configuration)
    {
      var providerInfo = Handlers.ProviderInfo;

      var applyCorrector = providerInfo.Supports(ProviderFeatures.Apply)
        ? ApplyProviderCorrector.SilentCorrector
        : ApplyProviderCorrector.ExceptionThrowingCorrector;

      var skipTakeCorrector = (providerInfo.Supports(ProviderFeatures.NativeTake | ProviderFeatures.NativeSkip))
        ? SkipTakeCorrector.FullPaginationSupportCorrector
        : new SkipTakeCorrector(providerInfo.Supports(ProviderFeatures.NativeTake),
            providerInfo.Supports(ProviderFeatures.NativeSkip));

      return new CompositePreCompiler(
        applyCorrector,
        skipTakeCorrector,
        RedundantColumnOptimizer.Instance,
        OrderingCorrector.DefaultInstance);
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
      IEnumerable<Type> basicCompilerContainers = new[] {
        typeof (NullableCompilers),
        typeof (StringCompilers),
        typeof (DateTimeCompilers),
        typeof (DateTimeOffsetCompilers),
        typeof (DateOnlyCompilers),
        typeof (TimeOnlyCompilers),
        typeof (TimeSpanCompilers),
        typeof (MathCompilers),
        typeof (NumericCompilers),
        typeof (DecimalCompilers),
        typeof (GuidCompilers),
        typeof (EnumCompilers),
        
      };
      var result = basicCompilerContainers;
      var defaultLoadedAssemblies = AssemblyLoadContext.Default.Assemblies;
      var currentLoadedAssemblies = AssemblyLoadContext.CurrentContextualReflectionContext.Assemblies;
      // dynamic registration to not cause assembly loading
      if (defaultLoadedAssemblies.Any(static a => a.GetName().Name.Equals("FSharp.Core", StringComparison.OrdinalIgnoreCase))
        || defaultLoadedAssemblies.Any(static a => a.GetName().Name.Equals("FSharp.Core", StringComparison.OrdinalIgnoreCase))) {
        result = result.Concat(new[] {
          typeof (FSharpMathOperationsCompilers),
          typeof (FSharpOperatorsCompilers),
          typeof (FSharpStringCompilers),
          typeof (FSharpConversionsCompilers),
        });
      }

      if (defaultLoadedAssemblies.Any(static a => a.GetName().Name.Equals("Microsoft.VisualBasic", StringComparison.OrdinalIgnoreCase))
        || defaultLoadedAssemblies.Any(static a => a.GetName().Name.Equals("Microsoft.VisualBasic", StringComparison.OrdinalIgnoreCase))) {
        result = result.Concat(new[] {
          typeof (VbConversionsCompilers),
          typeof (VbStringsCompilers),
          typeof (VbDateAndTimeCompilers),
        });
      }

      return result;
    }

    protected virtual SearchConditionCompiler CreateSearchConditionVisitor()
    {
      return new NullSearchConditionCompiler();
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
      var ordered = unordered.SortTopologically((first, second) => second.IsDependentOn(first));
      QueryPreprocessors = ordered ?? throw new InvalidOperationException(Strings.ExCyclicDependencyInQueryPreprocessorGraphIsDetected);
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