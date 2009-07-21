// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.15

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Various extension methods related to this namespace.
  /// </summary>
  public static class Extensions
  {
    /// <summary>
    /// Determines whether the specified active features is supported.
    /// </summary>
    public static bool IsSupported(this QueryFeatures activeFeatures, QueryFeatures featuresToTest)
    {
      return (activeFeatures & featuresToTest)==featuresToTest;
    }

    /// <summary>
    /// Determines whether the specified active features is supported.
    /// </summary>
    public static bool IsSupported(this IndexFeatures activeFeatures, IndexFeatures featuresToTest)
    {
      return (activeFeatures & featuresToTest)==featuresToTest;      
    }

    /// <summary>
    /// Determines whether the specified active features is supported.
    /// </summary>
    public static bool IsSupported(this DataTypeFeatures activeFeatures, DataTypeFeatures featuresToTest)
    {
      return (activeFeatures & featuresToTest)==featuresToTest;
    }
  }
}