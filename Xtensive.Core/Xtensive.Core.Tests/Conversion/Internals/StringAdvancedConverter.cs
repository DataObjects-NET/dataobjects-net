// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using System;
using Xtensive.Core.Conversion;

namespace Xtensive.Core.Tests.Conversion
{
  public class StringAdvancedConverter:
    IAdvancedConverter<string, int>, 
    IAdvancedConverter<int, string>
  {
    int IAdvancedConverter<string, int>.Convert(string value)
    {
      return Int32.Parse(value);
    }

    /// <summary>
    /// Gets <see langword="true"/> if converter is rough, otherwise gets <see langword="false"/>.
    /// </summary>
    bool IAdvancedConverter<string, int>.IsRough
    {
      get { return false; }
    }

    public string Convert(int value)
    {
      return value.ToString();
    }

    /// <summary>
    /// Gets <see langword="true"/> if converter is rough, otherwise gets <see langword="false"/>.
    /// </summary>
    public bool IsRough
    {
      get { return false; }
    }

    public IAdvancedConverterProvider Provider
    {
      get { return null; }
    }

    public StringAdvancedConverter(IAdvancedConverterProvider provider)
    {

    }

  }
}