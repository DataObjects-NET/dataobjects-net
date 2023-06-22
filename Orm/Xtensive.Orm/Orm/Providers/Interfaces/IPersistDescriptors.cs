// Copyright (C) 2012-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.03.22

using System;

namespace Xtensive.Orm.Providers
{
  public interface IPersistDescriptor
  {
    /// <summary>
    /// Request to store data.
    /// </summary>
    PersistRequest StoreRequest { get; }

    /// <summary>
    /// Request to clear data.
    /// </summary>
    PersistRequest ClearRequest { get; }
  }

  public interface IMultiRecordPersistDescriptor : IPersistDescriptor
  {
    /// <summary>
    /// Stores data by one record at a time. The request is used when data
    /// protion can't be stored by neither <see cref="StoreBigBatchRequest"/>
    /// nor <see cref="StoreSmallBatchRequest"/>.
    /// </summary>
    Lazy<PersistRequest> StoreSingleRecordRequest { get { return new Lazy<PersistRequest>(StoreRequest); } }

    /// <summary>
    /// Request that stores smaller portion of data at a time.
    /// The request is used when data can't be stored with <see cref="StoreBigBatchRequest"/>
    /// due to smaller size. When data portion size becomes smaller than the reqired by
    /// this request the <see cref="StoreSingleRecordRequest"/> will be used.
    /// </summary>
    Lazy<PersistRequest> StoreSmallBatchRequest { get; }

    /// <summary>
    /// Request that stores big portion of data at a time. The request is used
    /// unless data portion is smaller than the request can store, in this case 
    /// <see cref="StoreSmallBatchRequest"/> or even <see cref="StoreSingleRecordRequest" />
    /// will be used.
    /// </summary>
    Lazy<PersistRequest> StoreBigBatchRequest { get; }
  }
}
