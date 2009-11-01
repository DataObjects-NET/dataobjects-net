// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using System;
using PostSharp.Extensibility;

namespace Xtensive.Storage.Weaver
{
  internal class WeaverDebug
  {
    public static void WriteLine(string format, params object[] args)
    {
      WeaverMessageSource.Instance.Write(SeverityType.Warning, "WeaverDebug",
         new object[] {String.Format(format, args)});
    }
  }
}