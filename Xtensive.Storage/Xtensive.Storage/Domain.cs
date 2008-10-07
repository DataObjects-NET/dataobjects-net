// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposable;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage
{
  /// <summary>
  /// An access point to a single storage.
  /// </summary>
  public sealed class Domain : CriticalFinalizerObject,
    IDisposableContainer
  {
    private ICache<RecordSetHeader, RecordSetMapping> recordSetMappings;
    private int sessionCounter = 1;

    /// <summary>
    /// Gets the current <see cref="Domain"/> object
    /// using <see cref="Session"/>. <see cref="Session.Current"/>.
    /// </summary>
    public static Domain Current {
      get {
        var session = Session.Current;
        return session!=null ? session.Domain : null;
      }
    }
    
    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public DomainConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets the disposing state of the domain.
    /// </summary>
    public DisposingState DisposingState { get; private set; }

    /// <summary>
    /// Gets the domain model.
    /// </summary>
    public DomainModel Model { get; internal set; }

    /// <summary>
    /// Gets the handler factory.
    /// </summary>
    public HandlerFactory HandlerFactory  { 
      [DebuggerStepThrough]
      get { return Handlers.HandlerFactory; }
    }

    /// <summary>
    /// Gets the name builder.
    /// </summary>
    public NameBuilder NameBuilder { 
      [DebuggerStepThrough]
      get { return Handlers.NameBuilder; }
    }

    /// <summary>
    /// Gets the key manager.
    /// </summary>
    public KeyManager KeyManager {
      [DebuggerStepThrough]
      get { return Handlers.KeyManager; }
    }

    /// <summary>
    /// Gets the domain-level temporary data.
    /// </summary>
    public GlobalTemporaryData TemporaryData { get; private set; }

    /// <summary>
    /// Indicates whether debug event logging is enabled.
    /// Caches <see cref="Log.IsLogged"/> method result for <see cref="LogEventTypes.Debug"/> event.
    /// </summary>
    public bool IsDebugEventLoggingEnabled { get; private set; }

    internal DomainHandler Handler {
      [DebuggerStepThrough]
      get { return Handlers.DomainHandler; }
    }

    #region Private \ internal properties

    internal Dictionary<TypeInfo, Tuple> Prototypes { get; private set; }

    internal HandlerAccessor Handlers { get; private set; }

    #endregion

    #region OpenSession methods

    /// <summary>
    /// Opens the session with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <returns>New <see cref="SessionConsumptionScope"/> object.</returns>
    public SessionConsumptionScope OpenSession()
    {
      return OpenSession((SessionConfiguration)Configuration.Session.Clone());
    }

    /// <summary>
    /// Opens the session with specified <paramref name="configuration"/>.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>New <see cref="SessionConsumptionScope"/> object.</returns>
    public SessionConsumptionScope OpenSession(SessionConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      if (EnumerableExtensions.IsNullOrEmpty(configuration.Name)) {
        configuration.Name = sessionCounter.ToString();
        Interlocked.Increment(ref sessionCounter);
      }
      configuration.Lock(true);

      if (IsDebugEventLoggingEnabled)
        Log.Debug("Opening session '{0}'", configuration);

      var session = new Session(this, configuration);
      return new SessionConsumptionScope(session);
    }

    #endregion

    /// <summary>
    /// Builds the new <see cref="Domain"/> according to the specified 
    /// <see cref="DomainConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The <see cref="DomainConfiguration"/>.</param>
    /// <returns>Newly built <see cref="Domain"/>.</returns>
    public static Domain Build(DomainConfiguration configuration)
    {
      return DomainBuilder.Build(configuration);
    }

    #region Private \ internal methods

    internal RecordSetMapping GetMapping(RecordSetHeader header)
    {
      RecordSetMapping result;
      lock (recordSetMappings) {
        result = recordSetMappings[header, true];
        if (result!=null)
          return result;
      }
      var mappings = new List<ColumnGroupMapping>();
      foreach (var group in header.ColumnGroups) {
        var mapping = BuildColumnGroupMapping(header, group);
        if (mapping!=null)
          mappings.Add(mapping);
      }
      result = new RecordSetMapping(header, mappings);
      lock (recordSetMappings) {
        recordSetMappings.Add(result);
      }
      return result;
    }

    private ColumnGroupMapping BuildColumnGroupMapping(RecordSetHeader header, ColumnGroup group)
    {
      int typeIdColumnIndex = -1;
      var typeIdColumnName = NameBuilder.TypeIdColumnName;
      var columnMapping = new Dictionary<ColumnInfo, MappedColumn>(group.Columns.Count);

      foreach (int columnIndex in group.Columns) {
        var column = (MappedColumn)header.Columns[columnIndex];
        ColumnInfo columnInfo = column.ColumnInfoRef.Resolve(Model);
        columnMapping[columnInfo] = column;
        if (columnInfo.Name == typeIdColumnName)
          typeIdColumnIndex = column.Index;
      }

      if (typeIdColumnIndex == -1)
        return null;

      return new ColumnGroupMapping(Model, typeIdColumnIndex, columnMapping);
    }

    #endregion


    // Constructors

    internal Domain(DomainConfiguration configuration)
    {
      IsDebugEventLoggingEnabled = Log.IsLogged(LogEventTypes.Debug); // Just to cache this value
      Configuration = configuration;
      Handlers = new HandlerAccessor(this);
      Prototypes = new Dictionary<TypeInfo, Tuple>();
      TemporaryData = new GlobalTemporaryData();
      recordSetMappings = 
        new LruCache<RecordSetHeader, RecordSetMapping>(configuration.RecordSetMappingCacheSize, 
          m => m.Header,
          new WeakestCache<RecordSetHeader, RecordSetMapping>(false, false, m => m.Header));
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~Domain()
    {
      Dispose(false);
    }

    private void Dispose(bool isDisposing)
    {
      if (DisposingState == DisposingState.None) lock(this) if (DisposingState == DisposingState.None) {
        DisposingState = DisposingState.Disposing;
        try {
          if (IsDebugEventLoggingEnabled)
            Log.Debug("Domain disposing {0}.", isDisposing ? "explicitly" : "by calling finalizer.");
          Handlers.DisposeSafely();
        }
        finally {
          DisposingState=DisposingState.Disposed;
        }
      }
    }
  }
}
