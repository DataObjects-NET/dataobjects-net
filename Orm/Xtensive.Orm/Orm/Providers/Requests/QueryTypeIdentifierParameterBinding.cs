// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.


using System;
using Xtensive.Core;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A special version of <see cref="QueryParameterBinding"/> used for type identifire values within query.
  /// </summary>
  public class QueryTypeIdentifierParameterBinding : QueryParameterBinding
  {
    private static Func<ParameterContext, object> DummyFactory = (c) => null;

    /// <summary>
    /// Gets type indentifier of type in Domain model - <see cref="Xtensive.Orm.Model.TypeInfo.TypeId"/>.
    /// </summary>
    public int OriginalTypeId { get; private set; }

    /// <summary>
    /// Creates Instance of this class;
    /// </summary>
    /// <param name="originalTypeId">Type indentifier of type in Domain model - <see cref="Xtensive.Orm.Model.TypeInfo.TypeId"/></param>
    /// <param name="typeMapping">Type mapping for type of type idetifier.</param>
    public QueryTypeIdentifierParameterBinding(int originalTypeId, TypeMapping typeMapping)
      : base(typeMapping, DummyFactory, QueryParameterBindingType.TypeIdentifier)
    {
      OriginalTypeId = originalTypeId;
    }
  }
}