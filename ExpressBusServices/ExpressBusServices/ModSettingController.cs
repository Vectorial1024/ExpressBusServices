using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ExpressBusServices
{
    public class ModSettingController
    {
        public static readonly string pathToConfigXml = "ExpressBusServices_Config.xml";

        public static void Touch()
        {
            // with JSON being so tedious in C# I can understand why everyone opted for XML setting files
            ReadSettings();
            WriteSettings();
        }

        public static void ReadSettings()
        {
            // default values
            EBSModConfig.ExpressMode expressBusMode = EBSModConfig.ExpressMode.PRUDENTIAL;

            if (File.Exists(pathToConfigXml))
            {
                try
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(pathToConfigXml);
                    for (int i = 0; i < document.ChildNodes.Count; i++)
                    {
                        XmlNode root = document.ChildNodes[i];
                        if (root.Name == "ExpressBusServices_Config")
                        {
                            for (int j = 0; j < root.ChildNodes.Count; j++)
                            {
                                XmlNode currentConfigNode = root.ChildNodes[j];

                                if (currentConfigNode.Name == "ExpressBuses_SelectedIndex")
                                {
                                    string tempIndex = currentConfigNode.InnerText;
                                    int selectedIndex = Convert.ToInt32(tempIndex);
                                    expressBusMode = (EBSModConfig.ExpressMode)selectedIndex;
                                    // Debug.Log($"Read {EBSModConfig.CurrentExpressBusMode} from settings file.");
                                }
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    Debug.Log($"Could not load config file: {x}");
                }
            }

            EBSModConfig.CurrentExpressBusMode = expressBusMode;
        }

        public static void WriteSettings()
        {
            // var interpretation = IPT2UnbunchingRuleReader.CurrentRuleInterpretation;
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineOnAttributes = true;
                settings.CloseOutput = true;

                XmlWriter writer = XmlWriter.Create(pathToConfigXml, settings);

                writer.WriteStartDocument();
                writer.WriteStartElement("ExpressBusServices_IPT2_Config");

                writer.WriteStartElement("SelectedIndex");
                //writer.WriteString(((int)interpretation).ToString());
                //Debug.Log($"Write {((int)interpretation).ToString()} to config file.");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Close();
            }
            catch (Exception x)
            {
                Debug.Log($"Could not write to config file: {x}");
            }
        }
    }
}
