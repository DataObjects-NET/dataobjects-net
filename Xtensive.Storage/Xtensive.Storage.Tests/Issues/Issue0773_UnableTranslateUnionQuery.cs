// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.08.11

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Issues
{
  [Serializable]
  public class Issue0773_UnableTranslateUnionQuery : NorthwindDOModelTest
  {
    private class DTO
    {
      public int Id { get; set; }
      public int ProductId { get; set; }
      public decimal Amount { get; set; }
      public decimal? Freight;
    }

    [Test]
    public void MainTest()
    {
      var query =
        from o in Query.All<Order>()
        join od in Query.All<OrderDetails>() on o equals od.Order
        where o.Freight > 0.1m
        select new DTO {Id = o.Id, Freight = o.Freight, ProductId = od.Product.Id, Amount = od.UnitPrice*od.Quantity};
      var list = query.ToList();
      var secondQuery =
        from od in Query.All<OrderDetails>()
        select new DTO() {Id = od.Order.Id, Freight = null, ProductId = od.Product.Id, Amount = 1234};
      var secondList = secondQuery.ToList();
      var result = query
        .Union(secondQuery)
        .OrderByDescending(dto => dto.Amount)
        .ThenBy(dto=>dto.Id);
      var resultList = result.ToList();

    }
    /*var innerEx = from b in Query.All<RegBalance>()
                          join p in Query.All<RegCalculatedPrice>() on new { b.FinTool, b.AuthDate } equals
                              new { p.FinTool, AuthDate = (DateTime?)p.ActualizationDate }
                          where b.ActualVolume > 0
                          select
                              new AnonymousClass
                                  {
                                      FinTool = b.FinTool,
                                      AuthDate = b.AuthDate,
                                      Fund = b.Fund,
                                      ActualVolume = (decimal?)b.ActualVolume,
                                      VolumeDelta = (decimal?)b.VolumeDelta,
                                      Price = (decimal?)null
                                  };
            var balEx = from b in Query.All<RegBalance>()
                        select
                            new AnonymousClass
                                {
                                    FinTool = b.FinTool,
                                    AuthDate = b.AuthDate,
                                    Fund = b.Fund,
                                    ActualVolume = (decimal?)b.ActualVolume,
                                    VolumeDelta = (decimal?)b.VolumeDelta,
                                    Price = (decimal?)null
                                };

            var unionPrice = from p in Query.All<RegCalculatedPrice>()
                             select
                                 new AnonymousClass
                                     {
                                         FinTool = p.FinTool,
                                         AuthDate = (DateTime?)p.ActualizationDate,
                                         Fund = (Fund)null,
                                         ActualVolume = (decimal?)null,
                                         VolumeDelta = (decimal?)null,
                                         Price = (decimal?)p.Price
                                     };

            var unionRes = balEx.Except(innerEx).Union(unionPrice)
                .OrderBy(a => a.FinTool).ThenBy(a => a.AuthDate).ThenBy(a => a.Fund);*/
  }
}