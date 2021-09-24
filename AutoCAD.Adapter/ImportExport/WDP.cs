using ICA.AutoCAD.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace ICA.AutoCAD.Adapter
{
    public static class WDP
    {
        private static readonly Dictionary<int, string> SettingIds = new Dictionary<int, string>
        {
            {0 , "RUNGHORV" },
            {1 , "REFNUMS" },
            {2 , "TAGMODE" },
            {3 , "TAG-START" },
            {4 , "TAG-RSUF" },
            {5 , "TAGFMT" },
            {6 , "WIREMODE" },
            {7 , "WIRE-START" },
            {8 , "WIRE-RSUF" },
            {9 , "WIREFMT" },
            {10, "WINC" },
            {11, "RUNGDIST" },
            {12, "DLADW" },
            //13
            {14, "XREFFMT" },
            {15, "RUNGINC" },
            {16, "DRWRUNG" },
            {17, "PH3SPACE" },
            {18, "DATUMX" },
            {19, "DATUMY" },
            {20, "DISTH" },
            {21, "DISTV" },
            {22, "CHAR_H" },
            {23, "CHAR_V" },
            {24, "HORIZ_FIRST" },
            {25, "XY_DELIM" },
            {26, "WIRELAYS" },
            {27, "WIRENO_LAY" },
            {28, "WIRECOPY_LAY" },
            {29, "WIREFIXED_LAY" },
            {30, "UNIT_SCL" },
            {31, "FEATURE_SCL" },
            {32, "TAG_LAY" },
            {33, "XREF_LAY" },
            {34, "DESC_LAY" },
            {35, "TERM_LAY" },
            {36, "CXREF_LAY" },
            {37, "CDESC_LAY" },
            {38, "LOC_LAY" },
            {39, "POS_LAY" },
            {40, "MISC_LAY" },
            {41, "WLEADERS" },
            {42, "PLC_STYLE" },
            {43, "ARROW_STYLE" },
            {44, "LINK_LAY" },
            {45, "COMP_LAY" },
            //46
            {47, "ALT_XREFFMT" },
            {48, "TAGFIXED_LAY" },
            {49, "GAP_STYLE" },
            {50, "MISC_FLAGS" },
            //51
            //52
            //53
            {54, "WIREREF_LAY" },
            //55
            {56, "FAN_INOUT_LAYS" },
            {57, "FAN_INOUT_STYLE" },
            {58, "LOCBOX_LAY" },
            {59, "SORTMODE" },
            {60, "XREF_STYLE" },
            {61, "XREF_FLAGS" },
            {62, "XREF_UNUSEDSTYLE" },
            {63, "XREF_FILLWITH" },
            {64, "XREF_SORT" },
            {65, "XREF_TXTBTWN" },
            {66, "XREF_GRAPHIC" },
            {67, "XREF_GRAPHICSTYLE" },
            {68, "XREF_CONTACTMAP" },
            {69, "XREF_TBLSTYLE" },
            {70, "XREF_TBLTITLE" },
            {71, "XREF_TBLINDEX" },
            {72, "XREF_TBLFLDNAMS" },
            {73, "XREF_TBLCOLJUST" },
            {74, "WNUM_OFFSET" },
            {75, "WNUM_FLAGS" },
            {76, "WNUM_GAP" },
        };

        public static Project Import(string filePath)
        {
            if (filePath is null)
                return null;

            Project output = new Project
            {
                DirectoryUri = new Uri(Path.GetDirectoryName(filePath)),
                Drawings = new List<Drawing>(),
                Job = new Job(Path.GetFileNameWithoutExtension(filePath))
            };
            string[] test = File.ReadAllLines(filePath);
            Drawing drawing = new Drawing()
            {
                Description = new List<string>()
            };
            Dictionary<string, string> settings = new Dictionary<string, string>();
            foreach (string line in test)
            {
                if (line.StartsWith("+"))
                    continue;
                else if (line.StartsWith("?"))
                {
                    Setting setting = new Setting(line);
                    if (setting.WDMName != null)
                        settings.Add(setting.WDMName, setting.Value);
                }
                else if (line.StartsWith("===="))
                    continue;
                else if (line.StartsWith("==="))
                    continue;
                else if (line.StartsWith("=="))
                    continue;
                else
                {
                    drawing.Name = Path.GetFileNameWithoutExtension(line);
                    drawing.Project = output;
                    output.Drawings.Add(drawing);
                    drawing = new Drawing()
                    {
                        Description = new List<string>()
                    };
                }
            }
            output.Settings = new ProjectSettings(settings);
            return output;
        }

        public static void Export(Project input, string savePath)
        {
            List<string> lines = new List<string>();

            #region Project Properties

            lines.Add($"+[1]{Paths.SchematicLibrary}");
            lines.Add($"+[2]ICA_MENU.dat");
            lines.Add($"+[3]{Paths.PanelLibrary}");
            lines.Add($"+[4]ICA_PANEL_MENU.dat");
            lines.Add($"+[5]1");    //Cross-reference options: Always use real-time (1)
            lines.Add($"+[6]0");    //MISC_CAT table: Always off (0)
            lines.Add($"+[7]");     //Obsolete
            lines.Add($"+[8]0");    //Obsolete
            lines.Add($"+[9]");     //LINEx Entries: Not used
            lines.Add($"+[10]0");   //Combined installation/location mode: Always off (0)
            lines.Add($"+[11]1");   //Description case mode: Always uppercase (1)
            lines.Add($"+[12]0");   //Obsolete
            lines.Add($"+[13]0");   //Wire tagging mode: Always normal (0 or blank)
            lines.Add($"+[14]0");   //IEC tag mode, only used if [10] is 1 or 3: Never used (0)
            lines.Add($"+[15]1");   //Auto-fill installation/location with drawing defaults: Always on (1)
            lines.Add($"+[16]");    //Horrible explanation in documentation, leave blank
            lines.Add($"+[17]");    //Same as [16]
            lines.Add($"+[18]1");   //Auto-hide wire number with terminals: Always on (1)
            lines.Add($"+[19]{input.Settings.Wire.Offset}"); //Carried through. Probably to be removed for ICA specific applications (wire numbers attached to symbols & hidden, or centered)
            lines.Add($"+[20]");    //Alternate wd.env files: Always blank
            lines.Add($"+[21]0");   //Wire number by layer mode: Always off (0)
            lines.Add($"+[22]");    //Wire number by layer settings: Always blank
            lines.Add($"+[23]0");   //Alternate catalog lookup file: Always disabled (0)
            lines.Add($"+[24]");    //Alternate catalog lookup file path: Always blank
            lines.Add($"+[25]1");   //Not documented
            lines.Add($"+[26](0 0.03125 0 )"); //Not documented
            lines.Add($"+[27]");    //Excluded wire numbers: Always blank
            lines.Add($"+[28]0");   //Obsolete
            lines.Add($"+[29]0");   //Wire number terminal override: Always disabled (0)
            lines.Add($"+[30]0");   //CLEN value: Always 0
            lines.Add($"+[31]");    //Wire number sort order: Always blank
            lines.Add($"+[32]1");   //Real-time error checking: Always on (1)
            lines.Add($"+[33]");    //Wire type column headers: Always blank
            lines.Add($"+[34]0");   //Suppress dash if inst/loc component tag mode is on: Always off (0)
            lines.Add($"+[35];");   //Panel wire annotation delimiter: defaulted to ";" not sure if it can be blank
            lines.Add($"+[36]0");   //Item number mode: Always by project (0)
            lines.Add($"+[37]1");   //Item number assignment: Always by part number (1)
            lines.Add($"+[38]");    //Electrical code standard: Always blank

            #endregion

            #region Default WD_M Properties

            lines.Add($"?[0]{input.Settings.Ladder.RungOrientation.ToString().Substring(0, 1)}");   //RUNGHORV
            lines.Add($"?[1]1"); //Line reference numbers (1)                                   //REFNUMS
            lines.Add($"?[2]{input.Settings.Component.Mode.ToString().Substring(0, 1)}");           //TAGMODE
            lines.Add($"?[3]{input.Settings.Component.Start}");                                     //TAG-START
            lines.Add($"?[4]{string.Join(",", input.Settings.Component.Suffixes)}");                //TAG-RSUF
            lines.Add($"?[5]{input.Settings.Component.Format}");                                    //TAGFMT
            lines.Add($"?[6]{input.Settings.Wire.Mode.ToString().Substring(0, 1)}");                //WIREMODE
            lines.Add($"?[7]{input.Settings.Wire.Start}");                                          //WIRE-START
            lines.Add($"?[8]{string.Join(",", input.Settings.Wire.Suffixes)}");                     //WIRE-RSUF
            lines.Add($"?[9]{input.Settings.Wire.Format}");                                         //WIREFMT
            lines.Add($"?[10]{input.Settings.Wire.Incremement}");                                   //WINC
            lines.Add($"?[11]{input.Settings.Ladder.RungSpacing}");                                 //RUNGDIST
            lines.Add($"?[12]14.5");                                                            //DLADW
            //lines.Add($"?[13]");  //-----------------------Missing, Obsolete?----------------------------
            lines.Add($"?[14]{input.Settings.CrossReference.Format}");                              //XREFFMT
            lines.Add($"?[15]{input.Settings.Ladder.RungIncrement}");                               //RUNGINC
            lines.Add($"?[16]{Convert.ToInt32(input.Settings.Ladder.DrawRungs)}");                  //DRWRUNG
            lines.Add($"?[17]{input.Settings.Ladder.ThreePhaseSpacing}");                           //PH3SPACE
            lines.Add($"?[18]");                                                                //DATUMX
            lines.Add($"?[19]");                                                                //DATUMY
            lines.Add($"?[20]");                                                                //DISTH
            lines.Add($"?[21]");                                                                //DISTV
            lines.Add($"?[22]");                                                                //CHAR_H
            lines.Add($"?[23]");                                                                //CHAR_V
            lines.Add($"?[24]");                                                                //HORIZ_FIRST
            lines.Add($"?[25]");                                                                //XY_DELIM
            lines.Add($"?[26]{ElectricalLayers.WireLayer.Name}");                               //WIRELAYS
            lines.Add($"?[27]{ElectricalLayers.WireNumberLayer.Name}");                         //WIRENO_LAY
            lines.Add($"?[28]");                                                                //WIRECOPY_LAY
            lines.Add($"?[29]");                                                                //WIREFIXED_LAY
            lines.Add($"?[30]1.0");                                                             //UNIT_SCL
            lines.Add($"?[31]1.0");                                                             //FEATURE_SCL
            lines.Add($"?[32]{ElectricalLayers.TagLayer.Name}");                                //TAG_LAY
            lines.Add($"?[33]{ElectricalLayers.XrefLayer.Name}");                               //XREF_LAY
            lines.Add($"?[34]{ElectricalLayers.DescriptionLayer.Name}");                        //DESC_LAY
            lines.Add($"?[35]{ElectricalLayers.TerminalLayer.Name}");                           //TERM_LAY
            lines.Add($"?[36]{ElectricalLayers.XrefLayer.Name}");                               //CXREF_LAY
            lines.Add($"?[37]{ElectricalLayers.DescriptionLayer.Name}");                        //CDESC_LAY
            lines.Add($"?[38]LOC");                                                             //LOC_LAY
            lines.Add($"?[39]POS");                                                             //POS_LAY
            lines.Add($"?[40]{ElectricalLayers.MiscellaneousLayer.Name}");                      //MISC_LAY
            lines.Add($"?[41]2");                                                               //WLEADERS
            lines.Add($"?[42]1");                                                               //PLC_STYLE
            lines.Add($"?[43]1");                                                               //ARROW_STYLE
            lines.Add($"?[44]{ElectricalLayers.LinkLayer.Name}");                               //LINK_LAY
            lines.Add($"?[45]{ElectricalLayers.SymbolLayer.Name}");                             //COMP_LAY
            //lines.Add($"?[46]"); //-----------------------Missing, Obsolete?----------------------------
            lines.Add($"?[47]{input.Settings.CrossReference.ExternalFormat}");                      //ALT_XREFFMT
            lines.Add($"?[48]{ElectricalLayers.TagLayer.Name}");                                //TAGFIXED_LAY
            lines.Add($"?[49]1");                                                               //GAP_STYLE
            lines.Add($"?[50]20");                                                              //MISC_FLAGS
            lines.Add($"?[51]"); //--------------------------No Clue--------------------------------------
            lines.Add($"?[52]"); //--------------------------No Clue--------------------------------------
            lines.Add($"?[53]"); //--------------------------No Clue--------------------------------------
            lines.Add($"?[54]{ElectricalLayers.WireLayer.Name}");                               //WIREREF_LAY
            lines.Add($"?[55]"); //--------------------------No Clue--------------------------------------
            lines.Add($"?[56]_MULTI_WIRE_");                                                    //FAN_INOUT_LAYS
            lines.Add($"?[57]1");                                                               //FAN_INOUT_STYLE
            lines.Add($"?[58]LOCBOX");                                                          //LOCBOX_LAY
            lines.Add($"?[59]");                                                                //SORTMODE
            lines.Add($"?[60]{(int)input.Settings.CrossReference.Style}");                          //XREF_STYLE
            lines.Add($"?[61]{Convert.ToInt32(input.Settings.CrossReference.IncludeUnused)}");      //XREF_FLAGS
            lines.Add($"?[62]{Convert.ToInt32(input.Settings.CrossReference.ContactCount)}");       //XREF_UNUSEDSTYLE
            lines.Add($"?[63]{input.Settings.CrossReference.FillWith}");                            //XREF_FILLWITH
            lines.Add($"?[64]{(int)input.Settings.CrossReference.SortMode}");                       //XREF_SORT
            lines.Add($"?[65]{input.Settings.CrossReference.Delimiter}");                           //XREF_TXTBTWN
            lines.Add($"?[66]1");                                                               //XREF_GRAPHIC
            lines.Add($"?[67]0");                                                               //XREF_GRAPHICSTYLE
            lines.Add($"?[68]NO,NC,NONC");                                                      //XREF_CONTACTMAP
            lines.Add($"?[69]ICA Default");                                                     //XREF_TBLSTYLE
            lines.Add($"?[70]%T");                                                              //XREF_TBLTITLE
            lines.Add($"?[71]1,2,3,4,5");                                                       //XREF_TBLINDEX
            lines.Add($"?[72]W1,T1,TYPE,T2,W2,REF,SH,SHDWGNAM,FILENAME,FULLFILENAME");          //XREF_TBLFLDNAMS
            lines.Add($"?[73]MC,MC,MC,MC,MC,MC,MC,MC,L,L");                                     //XREF_TBLCOLJUST
            lines.Add($"?[74]{input.Settings.Wire.Offset}");                                        //WNUM_OFFSET
            lines.Add($"?[75]{input.Settings.Wire.Flags}");                                         //WNUM_FLAGS
            lines.Add($"?[76]{string.Join(",", input.Settings.Wire.GapValues)}");                    //WNUM_GAP

            #endregion

            foreach(Drawing drawing in input.Drawings)
            {
                foreach (string description in drawing.Description)
                    lines.Add($"==={description}");
                lines.Add($"{input.FileUri.MakeRelativeUri(drawing.FileUri)}");
            }

            File.WriteAllLines(savePath, lines.ToArray());
        }

        public class Setting
        {
            public char Type { get; set; }
            public int Number { get; set; }
            public string Value { get; set; }

            public string WDMName => SettingIds.ContainsKey(Number) ? SettingIds[Number] : null;

            public Setting(string line)
            {
                switch (line[0])
                {
                    case '+':
                        Type = '+';
                        break;
                    case '?':
                        Type = '?';
                        break;
                    default:
                        throw new ArgumentException("Line is not a valid AutoCAD Electrical Project property");
                }
                int startIndex = line.IndexOf('[') + 1;
                int endIndex = line.IndexOf(']');
                int length = endIndex - startIndex;
                Number = int.Parse(line.Substring(startIndex, length));
                Value = line.Substring(endIndex + 1);
            }
        }
    }
}
