// Copyright (C) 2013-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    private const string OriginalConfigFileName = "Xtensive.Orm.Tests.dll.config";
    private const string XsdFileName = "Xtensive.Orm.xsd";
    private const string XsdInLowerCaseFileName = "Test.xsd";
    private const string ConfigFileName = "Test.config";
    private const string OriginalRootElementName = "AppConfigTest";
    private const string RootElementName = "Xtensive.Orm";
    private const string ConfigXmlNamespace = "http://dataobjects.net/schemas/appconfig/";
    private bool hasErrors;

    [Test]
    public void TestSchema()
    {
      hasErrors = false;

      var segmentConfig = XElement.Load(OriginalConfigFileName).Element(OriginalRootElementName);
      Debug.Assert(segmentConfig != null, "segmentConfig != null");
      segmentConfig.Name = RootElementName;

      foreach (var element in segmentConfig.DescendantsAndSelf()) {
        element.Name = (XNamespace) ConfigXmlNamespace + element.Name.LocalName;
      }

      using (var segmentConfigWriter = File.CreateText(ConfigFileName)) {
        segmentConfigWriter.Write(segmentConfig.ToString().ToLower());
      }

      ChangeXsdElementsToLowerCase();

      try {
        var schemaSettings = new XmlReaderSettings();
        _ = schemaSettings.Schemas.Add(ConfigXmlNamespace, XsdInLowerCaseFileName);
        schemaSettings.ValidationType = ValidationType.Schema;
        schemaSettings.ValidationEventHandler += new ValidationEventHandler(ValidationHandler);

        var configReader = XmlReader.Create(ConfigFileName, schemaSettings);
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
        File.Delete(ConfigFileName);
        File.Delete(XsdInLowerCaseFileName);
      }

      Assert.IsFalse(hasErrors);
    }

    public void ValidationHandler(object sender, ValidationEventArgs validationEventArgs)
    {
      hasErrors = true;
      var exception = validationEventArgs.Exception;
      Console.WriteLine($"({validationEventArgs.Severity}) {exception.GetType()}: {validationEventArgs.Message}");
      Console.WriteLine($"LineNumber = {exception.LineNumber}");
      Console.WriteLine($"LinePosition = {exception.LinePosition}");
    }

    private static void ChangeXsdElementsToLowerCase()
    {
      var elementsUsedInXsd = new string[] { "element", "complexType", "attribute", "simpleType", "restriction", "enumeration", "pattern" };

      var namespaceXsd = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
      var xsdWithElementsToLowerCase = XElement.Load(XsdFileName);

      foreach (var element in elementsUsedInXsd) {
        foreach (var attributes in xsdWithElementsToLowerCase.Descendants(namespaceXsd + element)) {
          foreach (var attribute in attributes.Attributes()) {
            attribute.Value = attribute.Value.ToLower();
          }
        }
      }

      using var xElementXsdWriter = File.CreateText(XsdInLowerCaseFileName);
      xElementXsdWriter.Write(xsdWithElementsToLowerCase);
    }
  }
}
