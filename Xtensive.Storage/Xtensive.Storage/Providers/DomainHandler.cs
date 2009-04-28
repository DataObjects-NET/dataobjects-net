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
  ///   <see cref="Storage.Domain"/>-level handler.
  /// </summary>
  public abstract class DomainHandler : InitializableHandlerBase
  {
    private readonly Dictionary<Type, IMemberCompilerProvider> memberCompilerProviders = new Dictionary<Type, IMemberCompilerProvider>();

    /// <summary>
    /// Gets the domain this handler is bound to.
    /// </summary>
    public Domain Domain { get; private set; }

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
    /// Gets the member compiler provider by its type parameter.
    /// </summary>
    /// <typeparam name="T">The type of member compiler provider type parameter.</typeparam>
    /// <returns>
    /// Found member compiler provider;
    /// <see langword="null"/>, if it was not found.
    /// </returns>
    public IMemberCompilerProvider<T> GetMemberCompilerProvider<T>()
    {
      return (IMemberCompilerProvider<T>) memberCompilerProviders[typeof(T)];
    }


    // Abstract methods

    /// <summary>
    /// Builds the compiler.
    /// Invoked from <see cref="Initialize"/>.
    /// </summary>
    /// <param name="compiledSources">The compiled sources. Shared across all compilers.</param>
    /// <returns>A new compiler.</returns>
    protected abstract ICompiler BuildCompiler(BindingCollection<object, ExecutableProvider> compiledSources);

    /// <summary>
    /// Builds the <see cref="IPreCompiler"/>.
    /// Invoked from <see cref="Initialize"/>.
    /// </summary>
    /// <returns>A new pre-compiler.</returns>
    protected abstract IPreCompiler BuildPreCompiler();

    /// <summary>
    /// Gets the sequence of compiler provider container types.
    /// </summary>
    /// <returns>The sequence of compiler provider container types.</returns>
    protected virtual IEnumerable<Type> GetCompilerProviderContainerTypes()
    {
      return EnumerableUtils<Type>.Empty;
    }

    /// <summary>
    /// Builds the mapping schema.
    /// </summary>
    public abstract void BuildMappingSchema();

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

    /// <exception cref="InvalidOperationException">One of compiler containers is 
    /// improperly described.</exception>
    private void BuildMemberCompilerProviders()
    {
      var customCompilerContainers = Domain.Configuration.CompilerContainers;
      var builtinCompilerContainers = GetCompilerProviderContainerTypes();
      var typeGroups = builtinCompilerContainers.GroupBy(
        t => ((CompilerContainerAttribute[]) t.GetCustomAttributes(
          typeof(CompilerContainerAttribute), false))[0].ExtensionType);

      foreach (var types in typeGroups) {
        var extensionType = ((CompilerContainerAttribute) types.First().GetCustomAttributes(
          typeof(CompilerContainerAttribute), false)[0]).ExtensionType;
        var memberCompilerProvider = MemberCompilerProviderFactory.Create(extensionType);
        memberCompilerProviders.Add(extensionType, memberCompilerProvider);
        foreach (var type in types)
          memberCompilerProvider.RegisterCompilers(type);
      }

      foreach (var type in customCompilerContainers) {
        var atr = type.GetCustomAttributes(typeof(CompilerContainerAttribute), false);
        if (atr.IsNullOrEmpty())
          throw new InvalidOperationException(String.Format(
            Strings.ExCompilerContainerAttributeIsNotAppliedToTypeX, type.Name));
        var extensionType = ((CompilerContainerAttribute)atr[0]).ExtensionType;
        var conflictHandlingMethod = ((CompilerContainerAttribute)atr[0]).ConflictHandlingMethod;

        IMemberCompilerProvider memberCompilerProvider;
        memberCompilerProviders.TryGetValue(extensionType, out memberCompilerProvider);
        if (memberCompilerProvider == null)
          memberCompilerProvider = MemberCompilerProviderFactory.Create(extensionType);
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
        BuildPreCompiler);
      BuildMemberCompilerProviders();
    }

    /// <summary>
    /// Initializes the system session.
    /// </summary>
    public virtual void OnSystemSessionOpen()
    {
    }
  }
}