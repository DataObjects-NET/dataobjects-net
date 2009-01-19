using System.Xml;

namespace Xtensive.Storage.Install.Wix
{
  internal class WixAdapter
  {
    private static readonly string fileName = @"..\..\..\..\Xtensive.Storage.Install\Xtensive.Storage.Install.Setup\";

    private static void Main(string[] args)
    {
      var doc = new XmlDocument();
      doc.Load(fileName + "Files.wxs");

      var manager = new XmlNamespaceManager(doc.NameTable);
      manager.AddNamespace("wxs", "http://schemas.microsoft.com/wix/2006/wi");

      if (doc.DocumentElement!=null) {
        var NodeToChange = ((XmlElement)(doc.DocumentElement.SelectSingleNode("//wxs:Fragment/wxs:Directory", manager)));

        var NewNode = (XmlElement)doc.CreateElement("DirectoryRef", "http://schemas.microsoft.com/wix/2006/wi").Clone();
        NewNode.SetAttribute("Id", "INSTALLLOCATION");

        for (int i = 0; i < NodeToChange.ChildNodes.Count; i++)
          NewNode.AppendChild(NodeToChange.ChildNodes[i].CloneNode(true));

        XmlNode ParentOfNodeToChange = NodeToChange.ParentNode;
        ParentOfNodeToChange.ReplaceChild(NewNode, NodeToChange);
        doc.DocumentElement.ReplaceChild(ParentOfNodeToChange, doc.DocumentElement.FirstChild);

        var classNodes = doc.SelectNodes("//wxs:Class", manager);
        if (classNodes != null)
          foreach (XmlElement item in classNodes)
            item.SetAttribute("Server", item.ParentNode.Attributes["Id"].Value);

        var registryValueNodes = doc.SelectNodes("//wxs:RegistryValue", manager);
        if (registryValueNodes != null)
          foreach (XmlElement registry in registryValueNodes)
            if (registry.Attributes["Value"].Value == "mscoree.dll")
              registry.ParentNode.RemoveChild(registry);
        doc.Save(fileName + "Files.wxs");

        string wpfValue = string.Empty;
        string consoleValue = string.Empty;
        string sampleValue = string.Empty;
        string templateValue = string.Empty;
        var sampleNodes = doc.DocumentElement.SelectNodes(@"//wxs:File[@Source and contains(@Source,'Samples\Xtensive.Storage.Samples.Wpf\bin\Release\Xtensive.Storage.Samples.Wpf.exe')]", manager);
        if (sampleNodes!=null) {
          foreach (XmlElement item in sampleNodes)
            if (item.Attributes["Source"].Value.EndsWith("Xtensive.Storage.Samples.Wpf.exe")) {
              wpfValue = item.ParentNode.ParentNode.Attributes["Id"].Value;
              break;
            }
        }

        sampleNodes = doc.DocumentElement.SelectNodes(@"//wxs:File[@Source and contains(@Source,'Samples\Xtensive.Storage.Samples.Console\bin\Release\Xtensive.Storage.Samples.Console.exe')]", manager);
        if (sampleNodes!=null) {
          foreach (XmlElement item in sampleNodes)
            if (item.Attributes["Source"].Value.EndsWith("Xtensive.Storage.Samples.Console.exe")) {
              consoleValue = item.ParentNode.ParentNode.Attributes["Id"].Value;
              break;
            }
        }

        var sampleNode = (XmlElement) doc.DocumentElement.SelectSingleNode(@"//wxs:File[@Source and contains(@Source,'Samples\Xtensive.Storage.Samples.sln')]", manager);
        sampleValue = sampleNode.ParentNode.ParentNode.Attributes["Id"].Value;

        sampleNode = (XmlElement) doc.DocumentElement.SelectSingleNode(@"//wxs:File[@Source and contains(@Source,'DataObjects.Net.zip')]", manager);
        var componenRefNodeToReplace = (XmlElement) doc.DocumentElement.SelectSingleNode("//wxs:ComponentRef[@Id='" + sampleNode.ParentNode.Attributes["Id"].Value + "']", manager);
        componenRefNodeToReplace.ParentNode.RemoveChild(componenRefNodeToReplace);
        templateValue = sampleNode.Attributes["Source"].Value;
        sampleNode.ParentNode.ParentNode.RemoveChild(sampleNode.ParentNode);

        doc.Save(fileName + "Files.wxs");

        doc = new XmlDocument();
        doc.Load(fileName + "Setup.wxs");
        if (doc.DocumentElement!=null) {
          var node = doc.DocumentElement.SelectSingleNode("//wxs:Shortcut[@Id='WpfShortcut']", manager);
          node.Attributes["Target"].Value = "[" + wpfValue + "]" + "Xtensive.Storage.Samples.Wpf.exe";
          node.Attributes["WorkingDirectory"].Value = wpfValue;
          node = doc.DocumentElement.SelectSingleNode("//wxs:Shortcut[@Id='ConsoleShortcut']", manager);
          node.Attributes["Target"].Value = "[" + consoleValue + "]" + "Xtensive.Storage.Samples.Console.exe";
          node.Attributes["WorkingDirectory"].Value = consoleValue;
          node = doc.DocumentElement.SelectSingleNode("//wxs:Shortcut[@Id='SampleShortcut']", manager);
          node.Attributes["Target"].Value = "[" + sampleValue + "]" + "Xtensive.Storage.Samples.sln";
          node.Attributes["WorkingDirectory"].Value = sampleValue;

          node = doc.DocumentElement.SelectSingleNode("//wxs:File[@Id='DataObjects.Net.zip']", manager);
          node.Attributes["Source"].Value = templateValue;

          doc.Save(fileName + "Setup.wxs");
        }
      }
    }
  }
}
