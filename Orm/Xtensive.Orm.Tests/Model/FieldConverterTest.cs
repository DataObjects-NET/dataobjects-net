// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.20

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.FieldConverterTestModel;

namespace Xtensive.Orm.Tests.Model.FieldConverterTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    private string DateValue { get; set; }

    public DateTime? Date
    {
      get
      {
        if (string.IsNullOrEmpty(DateValue))
        if (string.IsNullOrEmpty(DateValue))
          return null;
        return DateTime.Parse(DateValue);
      }
      set { DateValue = value.ToString(); }
    }

    [Field]
    private bool IsChanged { get; set; }

    [Field]
    private byte[] CollectionValue { get; set; }

    public ObservableCollection<int> Collection { get; set; }

    private static byte[] Serialize(ObservableCollection<int> collection)
    {
      using (var ms = new MemoryStream()) {
        var dcSerializer = new DataContractSerializer(typeof(ObservableCollection<int>), new[] { typeof(int) });
        dcSerializer.WriteObject(ms, collection);
        return ms.ToArray();
      }
    }

    private static ObservableCollection<int> Deserialize(byte[] bytes)
    {
      using (var ms = new MemoryStream(bytes)) {
        var dcSerializer = new DataContractSerializer(typeof(ObservableCollection<int>), new[] { typeof(int) });
        return (ObservableCollection<int>) dcSerializer.ReadObject(ms);
      }
    }

    protected override void OnInitialize()
    {
      base.OnInitialize();
      if (CollectionValue != null && CollectionValue.Length > 0)
        Collection = Deserialize(CollectionValue);
      else
        Collection = new ObservableCollection<int>();
      Collection.CollectionChanged += Collection_CollectionChanged;
      Session.Events.Persisting += Session_Persisting;
    }

    void Session_Persisting(object sender, EventArgs e)
    {
      if (!IsChanged)
        return;
      CollectionValue = Serialize(Collection);
      IsChanged = false;
    }

    void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      IsChanged = true;
    }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  public class FieldConverterTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof (Person).Assembly, typeof (Person).Namespace);
      return config;
    }

    [Test]
    public void SingleValueTest()
    {
      using (var session = Domain.OpenSession()) {
        Key key = null;
        var dateTime = new DateTime(2000, 01, 07);
        using (var t = session.OpenTransaction()) {

          var person = new Person();
          key = person.Key;
          Assert.That(person.Date, Is.Null);
          person.Date = dateTime;

          t.Complete();
        }

        using (var t = session.OpenTransaction()) {

          var person = session.Query.Single<Person>(key);
          Assert.That(person.Date, Is.EqualTo(dateTime));

          t.Complete();
        }
      }
    }

    [Test]
    public void CollectionTest()
    {
      Key key;
      // Creating entity
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var person = new Person();
          key = person.Key;
          person.Collection.Add(4);
          person.Collection.Add(5);
          t.Complete();
        }
      }

      // Fetching entity & modifying it
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var person = session.Query.Single<Person>(key);
          Assert.That(person.Collection, Is.Not.Null);
          Assert.That(person.Collection.Count, Is.EqualTo(2));
          Assert.That(person.Collection[0], Is.EqualTo(4));
          Assert.That(person.Collection[1], Is.EqualTo(5));

          person.Collection.Add(6);
          t.Complete();
        }
      }

      // Fetching entity
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var person = session.Query.Single<Person>(key);
          Assert.That(person.Collection, Is.Not.Null);
          Assert.That(person.Collection.Count, Is.EqualTo(3));
          Assert.That(person.Collection[0], Is.EqualTo(4));
          Assert.That(person.Collection[1], Is.EqualTo(5));
          Assert.That(person.Collection[2], Is.EqualTo(6));
          t.Complete();
        }
      }
    }
  }
}