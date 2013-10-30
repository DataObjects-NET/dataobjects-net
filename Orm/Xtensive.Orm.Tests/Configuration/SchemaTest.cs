﻿// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2013.10.04

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Configuration
{
  [TestFixture]
  public class SchemaTest
  {
    private const string originalConfigFileName = "Xtensive.Orm.Tests.dll.config";
    private const string xsdFileName = "Xtensive.Orm.xsd";
    private const string configFileName = "Test.config";
    private const string originalRootElementName = "AppConfigTest";
    private const string rootElementName = "Xtensive.Orm";
    private const string configXmlNamespace = "http://dataobjects.net/schemas/appconfig/";
    private bool hasErrors;

    [Test]
    public void TestSchema()
    {
      hasErrors = false;

      XElement segmentConfig = XElement.Load(originalConfigFileName).Element(originalRootElementName);
      Debug.Assert(segmentConfig != null, "segmentConfig != null");
      segmentConfig.Name = rootElementName;

      foreach (XElement element in segmentConfig.DescendantsAndSelf()) {
        element.Name = (XNamespace)configXmlNamespace + element.Name.LocalName;
      }

      using (StreamWriter segmentConfigWriter = File.CreateText(configFileName)) {
        segmentConfigWriter.Write(segmentConfig);
      }

      try {
        XmlReaderSettings schemaSettings = new XmlReaderSettings();
        schemaSettings.Schemas.Add(configXmlNamespace, xsdFileName);
        schemaSettings.ValidationType = ValidationType.Schema;
        schemaSettings.ValidationEventHandler += new ValidationEventHandler(ValidationHandler);

        XmlReader configReader = XmlReader.Create(configFileName, schemaSettings);
        while (configReader.Read()) {
        }
        configReader.Close();
      }
      catch (XmlException exception) {
        hasErrors = true;
        Console.WriteLine("{0}: {1}", exception.GetType(), exception.Message);
        Console.WriteLine("LineNumber = {0}", exception.LineNumber);
        Console.WriteLine("LinePosition = {0}", exception.LinePosition);
      }
      catch (XmlSchemaValidationException exception) {
        hasErrors = true;
        Console.WriteLine("{0}: {1}", exception.GetType(), exception.Message);
        Console.WriteLine("LineNumber = {0}", exception.LineNumber);
        Console.WriteLine("LinePosition = {0}", exception.LinePosition);
      }
      catch (XmlSchemaException exception) {
        hasErrors = true;
        Console.WriteLine("{0}: {1}", exception.GetType(), exception.Message);
        Console.WriteLine("LineNumber = {0}", exception.LineNumber);
        Console.WriteLine("LinePosition = {0}", exception.LinePosition);
      }
      catch (ArgumentNullException exception) {
        hasErrors = true;
        Console.WriteLine("{0}: {1}", exception.GetType(), exception.Message);
      }
      catch (InvalidOperationException exception) {
        hasErrors = true;
        Console.WriteLine("{0}: {1}", exception.GetType(), exception.Message);
      }
      catch (Exception exception) {
        hasErrors = true;
        Console.WriteLine("{0}: {1}", exception.GetType(), exception.Message);
      }
      finally {
        File.Delete(configFileName);
      }

      Assert.IsFalse(hasErrors);
    }

    public void ValidationHandler(object sender, ValidationEventArgs exception)
    {
      hasErrors = true;
      Console.WriteLine("({0}) {1}: {2}",exception.Severity, exception.GetType(), exception.Message);
    }
  }
}
