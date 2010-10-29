// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.24

namespace Xtensive.Reflection
{
  partial class WellKnown
  {
    ///<summary>
    /// Various well-known constants related to <see cref="object"/>.
    ///</summary>
    public static class Object
    {
      /// <summary>
      /// Returns "Clone".
      /// </summary>
      public const string Clone = "Clone";

      /// <summary>
      /// Returns "Equals".
      /// </summary>
      public new const string Equals = "Equals";

      /// <summary>
      /// Returns "GetHashCode".
      /// </summary>
      public new const string GetHashCode = "GetHashCode";
    }
  }
}