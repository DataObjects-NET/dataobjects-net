// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Linq;
using Xtensive.Core.Collections;
using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Resources;
using System.Linq;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// 	<see cref="Storage.Domain"/>-level handler.
  /// </summary>
  public abstract class DomainHandler : InitializableHandlerBase
  {
    private Dictionary<Type, IMemberCompilerProvider> compilerExtensions = new Dictionary<Type, IMemberCompilerProvider>();

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
    public ICompiler ServerSideCompiler { get; protected set; }

    /// <summary>
    /// Builds the compiler.
    /// </summary>
    /// <param name="compiledSources">The compiled sources. Shared across all compilers.</param>
    protected abstract ICompiler BuildCompiler(BindingCollection<object, ExecutableProvider> compiledSources);

    /// <summary>
    /// Gets the compiler extensions.
    /// </summary>
    public IMemberCompilerProvider<T> GetCompilerExtensions<T>()
    {
      return (IMemberCompilerProvider<T>)compilerExtensions[typeof(T)];
    }

    // Abstract methods

    /// <summary>
    /// Gets the compiler extensions types.
    /// </summary>
    protected virtual IEnumerable<Type> GetProviderCompilerExtensionTypes()
    {
      return Type.EmptyTypes;
    }

    /// <summary>
    /// Builds the <see cref="ServerSideCompiler"/> value.
    /// Builds the <see cref="IOptimizer"/>.
    /// Invoked from <see cref="Initialize"/>.
    /// </summary>
    protected abstract IOptimizer BuildOptimizer();

    /// <summary>
    /// Builds the <see cref="Domain"/> in recreate mode.
    /// </summary>
    public abstract void BuildRecreate();

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

    private void BuildCompilerExtentions()
    {
      var userExtensionTypes = Domain.Configuration.Mappings;
      var providerExtensionTypes = GetProviderCompilerExtensionTypes();
      var typeGroups = providerExtensionTypes.GroupBy(t => ((CompilerContainerAttribute[])t.GetCustomAttributes(typeof(CompilerContainerAttribute), false))[0].ExtensionType);

      foreach (var types in typeGroups) {
        var compilerExtentionType = ((CompilerContainerAttribute)types.First().GetCustomAttributes(typeof(CompilerContainerAttribute), false)[0]).ExtensionType;
        var memberCompilerProvider = MemberCompilerProviderFactory.Create(compilerExtentionType);
        compilerExtensions.Add(compilerExtentionType, memberCompilerProvider);
        foreach (var type in types)
          memberCompilerProvider.RegisterCompilers(type);
      }

      foreach (var type in userExtensionTypes) {
        var atr = type.GetCustomAttributes(typeof(CompilerContainerAttribute), false);
        if (atr.IsNullOrEmpty())
          throw new InvalidOperationException(String.Format(Strings.ExCompilerContainerAttributeIsNotAppliedToTypeX, type.Name));
        var compilerExtentionType = ((CompilerContainerAttribute)atr[0]).ExtensionType;
        var conflictHandlingMethod = ((CompilerContainerAttribute)atr[0]).ConflictHandlingMethod;

        IMemberCompilerProvider memberCompilerProvider;
        compilerExtensions.TryGetValue(compilerExtentionType, out memberCompilerProvider);
        if (memberCompilerProvider == null)
          memberCompilerProvider = MemberCompilerProviderFactory.Create(compilerExtentionType);
        memberCompilerProvider.RegisterCompilers(type, conflictHandlingMethod);
      }
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
      BuildCompilerExtentions();
    }

    /// <summary>
    /// Initializes the system session.
    /// </summary>
    public virtual void InitializeSystemSession()
    {
    }
  }
}