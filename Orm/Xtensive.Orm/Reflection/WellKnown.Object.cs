// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.02.24

namespace Xtensive.Reflection
{
  public partial class WellKnown
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