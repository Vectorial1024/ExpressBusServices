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
        public static readonly string rootNodeName = "ExpressBusServices_Config";

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
            bool enableSelfBalancing = true;
            bool selfBalCanTargetMid = true;
            bool canUseMinibusMode = true;

            // sectino break
            EBSModConfig.ExpressTramMode expressTramMode = EBSModConfig.ExpressTramMode.NONE;

            if (File.Exists(pathToConfigXml))
            {
                try
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(pathToConfigXml);
                    for (int i = 0; i < document.ChildNodes.Count; i++)
                    {
                        XmlNode root = document.ChildNodes[i];
                        if (root.Name == rootNodeName)
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
                                if (currentConfigNode.Name == "ExpressBuses_ServiceSelfBalancing")
                                {
                                    string tempValue = currentConfigNode.InnerText;
                                    enableSelfBalancing = Convert.ToBoolean(tempValue);
                                }
                                if (currentConfigNode.Name == "ExpressBuses_SSB_CanTargetMid")
                                {
                                    string tempValue = currentConfigNode.InnerText;
                                    selfBalCanTargetMid = Convert.ToBoolean(tempValue);
                                }
                                if (currentConfigNode.Name == "ExpressBuses_EnableMinibusMode")
                                {
                                    string tempValue = currentConfigNode.InnerText;
                                    canUseMinibusMode = Convert.ToBoolean(tempValue);
                                }
                                if (currentConfigNode.Name == "ExpressTrams_SelectedIndex")
                                {
                                    string tempIndex = currentConfigNode.InnerText;
                                    int selectedIndex = Convert.ToInt32(tempIndex);
                                    expressTramMode = (EBSModConfig.ExpressTramMode)selectedIndex;
                                    // Debug.Log($"Read {EBSModConfig.CurrentExpressBusMode} from settings file.");
                                }
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    Debug.LogError($"Could not load config file: {x}");
                }
            }

            EBSModConfig.CurrentExpressBusMode = expressBusMode;
            EBSModConfig.UseServiceSelfBalancing = enableSelfBalancing;
            EBSModConfig.ServiceSelfBalancingCanDoMiddleStop = selfBalCanTargetMid;
            EBSModConfig.CanUseMinibusMode = canUseMinibusMode;

            EBSModConfig.CurrentExpressTramMode = expressTramMode;
        }

        public static void WriteSettings()
        {
            var expressBusMode = EBSModConfig.CurrentExpressBusMode;
            var enableSelfBalancing = EBSModConfig.UseServiceSelfBalancing;
            var selfBalCanTargetMid = EBSModConfig.ServiceSelfBalancingCanDoMiddleStop;
            var canUseMinibusMode = EBSModConfig.CanUseMinibusMode;
            // var interpretation = IPT2UnbunchingRuleReader.CurrentRuleInterpretation;
            var expressTramMode = EBSModConfig.CurrentExpressTramMode;
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineOnAttributes = true;
                settings.CloseOutput = true;

                XmlWriter writer = XmlWriter.Create(pathToConfigXml, settings);

                writer.WriteStartDocument();
                writer.WriteStartElement(rootNodeName);

                writer.WriteStartElement("ExpressBuses_SelectedIndex");
                writer.WriteString(((int)expressBusMode).ToString());
                //Debug.Log($"Write {((int)interpretation).ToString()} to config file.");
                writer.WriteEndElement();

                writer.WriteStartElement("ExpressBuses_ServiceSelfBalancing");
                writer.WriteString(enableSelfBalancing.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("ExpressBuses_SSB_CanTargetMid");
                writer.WriteString(selfBalCanTargetMid.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("ExpressBuses_EnableMinibusMode");
                writer.WriteString(canUseMinibusMode.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("ExpressTrams_SelectedIndex");
                writer.WriteString(((int)expressTramMode).ToString());
                //Debug.Log($"Write {((int)interpretation).ToString()} to config file.");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Close();
            }
            catch (Exception x)
            {
                Debug.LogError($"Could not write to config file: {x}");
            }
        }
    }
}
