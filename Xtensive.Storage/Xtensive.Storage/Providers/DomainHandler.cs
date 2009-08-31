// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Linq;
using Xtensive.Storage.Building;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// <see cref="Storage.Domain"/>-level handler.
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
    /// Gets the information about provider's capabilities.
    /// </summary>
    public ProviderInfo ProviderInfo { get; private set; }

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
    /// <returns>A new post-compiler.</returns>
    protected abstract IPostCompiler CreatePostCompiler();

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
    public abstract void BuildMapping();

    /// <summary>
    /// Creates <see cref="ProviderInfo"/>.
    /// </summary>
    /// <returns></returns>
    protected abstract ProviderInfo CreateProviderInfo();

    #region Private \ internal methods

    private void BuildCompilationContext()
    {
      CompilationContext = new CompilationContext(
        CreateCompiler,
        CreatePreCompiler,
        CreatePostCompiler);
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

    #endregion

    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
      Domain = BuildingContext.Current.Domain;
      ProviderInfo = CreateProviderInfo();
      BuildMemberCompilerProviders();
      BuildCompilationContext();
    }
  }
}