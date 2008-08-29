// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.31

using Xtensive.Core.Conversion;

namespace Xtensive.TransactionLog.Tests
{
  public static class TestFileNameProvider
  {
    public static Biconverter<long, string> Instance =
      new Biconverter<long, string>(
        AdvancedConverterProvider.Default.GetConverter<long, string>().Implementation,
        AdvancedConverterProvider.Default.GetConverter<string, long>().Implementation);
  }
}