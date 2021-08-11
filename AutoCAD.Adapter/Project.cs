using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using ICA.AutoCAD.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace ICA.AutoCAD.Adapter
{
    public class Project
    {
        public enum DrawingType
        {
            Schematic,
            Panel,
            Reference
        }

        #region Public Properties

        public string Name { get; set; }
        public Uri Directory { get; set; }
        public List<Drawing> Drawings { get; set; }
        public ProjectProperties Properties { get; set; }

        #endregion

        public Project(string filePath)
        {
            ImportWDP(filePath);
        }

        public void Run<TArgument>(Action<Database, TArgument> action, TArgument value)
        {
            foreach(Drawing drawing in Drawings)
            {
                Database database = Commands.LoadDatabase(drawing.FileUri);
                if(Application.DocumentManager.Contains(drawing.FileUri))
                {
                    using (DocumentLock doclock = Application.DocumentManager.GetDocument(database).LockDocument())
                        action(database, value);
                }
                else
                {
                    action(database, value);
                    database.SaveAs(database.OriginalFileName, DwgVersion.Current);
                }
            }
        }

        public Document AddPage(DrawingType type, string savePath)
        {
            Document newPage;
            switch (type)
            {
                case DrawingType.Schematic:
                    newPage = Application.DocumentManager.Add(Properties.SchematicTemplate.LocalPath);
                    break;
                case DrawingType.Panel:
                    newPage = Application.DocumentManager.Add(Properties.PanelTemplate.LocalPath);
                    break;
                case DrawingType.Reference:
                    newPage = Application.DocumentManager.Add(Properties.ReferenceTemplate.LocalPath);
                    break;
                default:
                    return null;
            }
            newPage.Database.SaveAs(savePath, DwgVersion.Current);
            Drawings.Add(new Drawing()
            {
                FileUri = new Uri(Directory, savePath),
                Project = this,
                Properties = new ElectricalDocumentProperties(Properties)
            });
            return newPage;
        }

        public void ImportWDP(string filePath)
        {
            Drawings = new List<Drawing>();
            Name = Path.GetFileNameWithoutExtension(filePath);
            Directory = new Uri(Path.GetDirectoryName(filePath));
            Properties = new ProjectProperties();
            Uri projectUri = new Uri(filePath);
            string[] test = File.ReadAllLines(filePath);
            Drawing drawing = new Drawing();
            foreach (string line in test)
            {
                if (line.StartsWith("+"))
                    continue;
                else if (line.StartsWith("?"))
                    new WDPProperty(line);
                else if (line.StartsWith("==="))
                {
                    if (line != "===")
                        drawing.Description.Add(line.Substring(3));
                }
                else if (line.StartsWith("=="))
                    continue;
                else
                {
                    drawing.FileUri = new Uri(projectUri, line);
                    Drawings.Add(drawing);
                    drawing = new Drawing();
                }
            }
        }

        public void ExportWDP(string savePath)
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
            lines.Add($"+[19]{Properties.Wire.Offset}"); //Carried through. Probably to be removed for ICA specific applications (wire numbers attached to symbols & hidden, or centered)
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

            lines.Add($"?[0]{Properties.Ladder.RungOrientation.ToString().Substring(0, 1)}");   //RUNGHORV
            lines.Add($"?[1]1"); //Line reference numbers (1)                                   //REFNUMS
            lines.Add($"?[2]{Properties.Component.TagMode.ToString().Substring(0, 1)}");        //TAGMODE
            lines.Add($"?[3]{Properties.Component.TagStart}");                                  //TAG-START
            lines.Add($"?[4]{string.Join(",", Properties.Component.TagSuffixes)}");             //TAG-RSUF
            lines.Add($"?[5]{Properties.Component.TagFormat}");                                 //TAGFMT
            lines.Add($"?[6]{Properties.Wire.Mode.ToString().Substring(0, 1)}");                //WIREMODE
            lines.Add($"?[7]{Properties.Wire.Start}");                                          //WIRE-START
            lines.Add($"?[8]{string.Join(",", Properties.Wire.Suffixes)}");                     //WIRE-RSUF
            lines.Add($"?[9]{Properties.Wire.Format}");                                         //WIREFMT
            lines.Add($"?[10]{Properties.Wire.Incremement}");                                   //WINC
            lines.Add($"?[11]{Properties.Ladder.RungSpacing}");                                 //RUNGDIST
            lines.Add($"?[12]14.5");                                                            //DLADW
            //lines.Add($"?[13]");  //-----------------------Missing, Obsolete?----------------------------
            lines.Add($"?[14]{Properties.CrossReference.Format}");                              //XREFFMT
            lines.Add($"?[15]{Properties.Ladder.RungIncrement}");                               //RUNGINC
            lines.Add($"?[16]{Properties.Ladder.DrawRungs}");                                   //DRWRUNG
            lines.Add($"?[17]{Properties.Ladder.ThreePhaseSpacing}");                           //PH3SPACE
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
            lines.Add($"?[47]{Properties.CrossReference.ExternalFormat}");                      //ALT_XREFFMT
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
            lines.Add($"?[60]{(int)Properties.CrossReference.Style}");                          //XREF_STYLE
            lines.Add($"?[61]{Convert.ToInt32(Properties.CrossReference.IncludeUnused)}");      //XREF_FLAGS
            lines.Add($"?[62]{Convert.ToInt32(Properties.CrossReference.ContactCount)}");       //XREF_UNUSEDSTYLE
            lines.Add($"?[63]{Properties.CrossReference.FillWith}");                            //XREF_FILLWITH
            lines.Add($"?[64]{(int)Properties.CrossReference.SortMode}");                       //XREF_SORT
            lines.Add($"?[65]{Properties.CrossReference.Delimiter}");                           //XREF_TXTBTWN
            lines.Add($"?[66]1");                                                               //XREF_GRAPHIC
            lines.Add($"?[67]0");                                                               //XREF_GRAPHICSTYLE
            lines.Add($"?[68]NO,NC,NONC");                                                      //XREF_CONTACTMAP
            lines.Add($"?[69]ICA Default");                                                     //XREF_TBLSTYLE
            lines.Add($"?[70]%T");                                                              //XREF_TBLTITLE
            lines.Add($"?[71]1,2,3,4,5");                                                       //XREF_TBLINDEX
            lines.Add($"?[72]W1,T1,TYPE,T2,W2,REF,SH,SHDWGNAM,FILENAME,FULLFILENAME");          //XREF_TBLFLDNAMS
            lines.Add($"?[73]MC,MC,MC,MC,MC,MC,MC,MC,L,L");                                     //XREF_TBLCOLJUST
            lines.Add($"?[74]{Properties.Wire.Offset}");                                        //WNUM_OFFSET
            lines.Add($"?[75]{Properties.Wire.Flags}");                                         //WNUM_FLAGS
            lines.Add($"?[76]{string.Join(",",Properties.Wire.GapValues)}");                    //WNUM_GAP

            #endregion

            File.WriteAllLines(savePath, lines.ToArray());
        }
    }
}
