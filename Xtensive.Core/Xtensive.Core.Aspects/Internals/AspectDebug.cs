// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using System;
using PostSharp.Extensibility;

namespace Xtensive.Core.Aspects
{
  internal class AspectDebug
  {
    public static void WriteLine(string format, params object[] args)
    {
      AspectsMessageSource.Instance.Write(SeverityType.Warning, "AspectDebug",
         new object[] {String.Format(format, args)});
    }
  }
}