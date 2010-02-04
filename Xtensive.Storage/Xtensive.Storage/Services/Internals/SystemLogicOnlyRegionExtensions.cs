// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.05

using System;

namespace Xtensive.Storage.Services
{
  internal static class SystemLogicOnlyRegionExtensions
  {
    public static IDisposable OpenSystemLogicOnlyRegion(this IUsesSystemLogicOnlyRegions instance)
    {
      return instance.Session.OpenSystemLogicOnlyRegion();
    }

    public static IDisposable OpenSystemLogicOnlyRegion(this Session session)
    {
      return session.Services.Get<DirectSessionAccessor>()
        .OpenSystemLogicOnlyRegion();
    }
  }
}