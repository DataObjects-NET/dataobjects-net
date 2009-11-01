// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.31

namespace Xtensive.TransactionLog.Tests
{
  public class TestFileNameProvider : IFileNameFormatter<long>
  {
    public long RestoreFromString(string value)
    {
      return long.Parse(value);
    }

    public string SaveToString(long key)
    {
      return key.ToString("D10");
    }
  }
}