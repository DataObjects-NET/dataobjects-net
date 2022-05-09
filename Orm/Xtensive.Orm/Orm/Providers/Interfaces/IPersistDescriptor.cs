// Copyright (C) 2012-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.03.22

using System;

namespace Xtensive.Orm.Providers
{
  public interface IPersistDescriptor
  {
    Lazy<PersistRequest> LazyStoreRequest { get; }
    Lazy<PersistRequest> LazyLevel1BatchStoreRequest { get; }
    Lazy<PersistRequest> LazyLevel2BatchStoreRequest { get; }

    Lazy<PersistRequest> ClearRequest { get; }
  }
}