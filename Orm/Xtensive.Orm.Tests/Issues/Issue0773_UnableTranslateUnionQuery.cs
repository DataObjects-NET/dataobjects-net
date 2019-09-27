// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.08.11

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Issues
{
  [Serializable]
  public class Issue0773_UnableTranslateUnionQuery : ChinookDOModelTest
  {
    private class DTO
    {
      public int InvoiceId { get; set; }
      public int TrackId { get; set; }
      public decimal Amount { get; set; }
      public decimal? Commission;
    }

    [Test]
    public void MainTest()
    {
      var query =
        from i in Session.Query.All<Invoice>()
        join il in Session.Query.All<InvoiceLine>() on i equals il.Invoice
        where i.Commission > 0.1m
        select new DTO {InvoiceId = i.InvoiceId, Commission = i.Commission, TrackId = il.Track.TrackId, Amount = il.UnitPrice*il.Quantity};
      var list = query.ToList();
      var secondQuery =
        from il in Session.Query.All<InvoiceLine>()
        select new DTO() {InvoiceId = il.Invoice.InvoiceId, Commission = null, TrackId = il.Track.TrackId, Amount = 1234};
      var secondList = secondQuery.ToList();
      var result = query
        .Union(secondQuery)
        .OrderByDescending(dto => dto.Amount)
        .ThenBy(dto=>dto.InvoiceId);
      var resultList = result.ToList();

    }
    /*var innerEx = from b in Session.Query.All<RegBalance>()
                          join p in Session.Query.All<RegCalculatedPrice>() on new { b.FinTool, b.AuthDate } equals
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
            var balEx = from b in Session.Query.All<RegBalance>()
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

            var unionPrice = from p in Session.Query.All<RegCalculatedPrice>()
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