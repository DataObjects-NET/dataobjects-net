// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal sealed class ParameterizedQuery : TranslatedQuery
  {
    public readonly Parameter QueryParameter;

    public ParameterizedQuery(TranslatedQuery translatedQuery, Parameter parameter)
      : base(translatedQuery.DataSource, translatedQuery.Materializer, translatedQuery.ResultAccessMethod)
    {
      QueryParameter = parameter;
    }
  }
}