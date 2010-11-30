// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Abstract base class for any SQL temporary data provider.
  /// </summary>
  public abstract class SqlTemporaryDataProvider : SqlProvider
  {
    public const string TemporaryTableLockName = "TemporaryTableLockName";

    public TemporaryTableDescriptor TableDescriptor { get; private set; }


    // Constructors

    protected SqlTemporaryDataProvider(
      HandlerAccessor handlers, QueryRequest request, TemporaryTableDescriptor tableDescriptor,
      CompilableProvider origin, ExecutableProvider[] sources)
      : base(handlers, request, origin, sources)
    {
      TableDescriptor = tableDescriptor;
    }
  }
}