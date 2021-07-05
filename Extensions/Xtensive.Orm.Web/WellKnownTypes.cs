// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Orm.Web.Filters;

namespace Xtensive.Orm.Web
{
  internal static class WellKnownTypes
  {
    public static readonly Type DomainType = typeof(Domain);
    public static readonly Type SessionAccessorType = typeof(SessionAccessor);
  }
}
