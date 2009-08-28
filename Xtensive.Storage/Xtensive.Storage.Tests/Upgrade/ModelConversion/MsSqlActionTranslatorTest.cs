// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.27

using System;
using NUnit.Framework;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Tests.Upgrade
{
  [Explicit("Requires MSSQL servers")]
  public sealed class MsSqlActionTranslatorTest : SqlActionTranslatorTest
  {
    protected override string GetConnectionUrl() { return "sqlserver://localhost/DO40-Tests"; }
    protected override bool IsIncludedColumnsSupported { get { return true; } }
    
    protected override ProviderInfo CreateProviderInfo()
    {
      return new ProviderInfo(new Version(9,0), ProviderFeatures.KeyColumnSortOrder | ProviderFeatures.IncludedColumns | ProviderFeatures.ForeignKeyConstraints, 128);
    }
  }
}