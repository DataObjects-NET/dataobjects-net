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
  [Explicit("Requires PostgreSQL")]
  public sealed class PgSqlActionTranslatorTest : SqlActionTranslatorTest
  {
    protected override string GetConnectionUrl() { return "postgresql://do4test:do4testpwd@localhost:8332/do40test?Encoding=ASCII"; } 
    protected override bool IsIncludedColumnsSupported { get { return false; } }
    
    protected override ProviderInfo CreateProviderInfo()
    {
      return new ProviderInfo(new Version(8,4), ProviderFeatures.Sequences | ProviderFeatures.ForeignKeyConstraints, 63);
    }
  }
}
