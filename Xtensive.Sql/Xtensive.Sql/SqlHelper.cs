// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Core;

namespace Xtensive.Sql
{
  /// <summary>
  /// Various helper methods related to this namespace.
  /// </summary>
  public static class SqlHelper
  {
    /// <summary>
    /// Validates the specified URL againts charactes that usually forbidden inside connection strings.
    /// </summary>
    /// <param name="url">The URL.</param>
    public static void ValidateConnectionUrl(UrlInfo url)
    {
      var forbiddenChars = new[] {'=', ';'};
      var e = new ArgumentException("Part of URL contains '=' or ';' characters.", "url");
      if (url.Host.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (url.Resource.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (url.User.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (url.Password.IndexOfAny(forbiddenChars)>=0)
        throw e;
    }
  }
}