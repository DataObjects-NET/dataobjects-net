// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse.Compilation;

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
    public Rse.Compilation.CompilationContext CompilationContext { get; private set; }

    /// <summary>
    /// Gets the client compiler.
    /// </summary>
    public ICompiler ClientCompiler { get; protected set; }

    /// <summary>
    /// Gets the compiler.
    /// </summary>
    public ICompiler Compiler { get; private set; }
   

    // Abstract methods

    /// <summary>
    /// Builds the <see cref="Compiler"/> value.
    /// Invoked from <see cref="Initialize"/>.
    /// </summary>
    protected abstract ICompiler BuildCompiler();

    /// <summary>
    /// Builds the <see cref="Domain"/> in recreate mode.
    /// </summary>
    public abstract void BuildRecreate();


    /// <summary>
    /// Builds <see cref="Domain"/>s in recycling data while upgrade.
    /// </summary>
    public abstract void BuildRecycling();

    /// <summary>
    /// Checks if storage contains correct system metadata.
    /// </summary>
    /// <returns></returns>
    public abstract bool CheckSystemTypes();

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
      if ((type & SessionType.System)!=0)
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
      ClientCompiler = new DefaultCompiler();
      Compiler = BuildCompiler();
      CompilationContext = new CompilationContext(new SitePreferenceCompiler(Compiler, ClientCompiler));
    }
  }
}