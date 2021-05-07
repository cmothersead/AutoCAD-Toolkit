using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Object
{
    public class Balloon
    {
        [CommandMethod("BALLOON")]
        public void Insert()
        {
            Document currentDocument = Application.DocumentManager.MdiActiveDocument;
            Database currentDatabase = currentDocument.Database;
            Editor currentEditor = currentDocument.Editor;
            using (Transaction transaction = currentDatabase.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = transaction.GetObject(currentDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                PromptSelectionResult blockPrompt = currentEditor.GetSelection();
                if (blockPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet selectionSet = blockPrompt.Value;
                    foreach (SelectedObject selectedObject in selectionSet)
                    {
                        if (selectedObject != null)
                        {
                            if (selectedObject.ObjectId.ObjectClass.Name == "AcDbBlockReference")
                            {
                                BlockReference selectedBlockReference = transaction.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as BlockReference;
                                string itemNumber = selectedBlockReference.GetItemNumber();
                                if (itemNumber != null)
                                {
                                    MLeader balloon = new MLeader
                                    {
                                        Layer = "BALLOON"
                                    };
                                    //balloon.SetPropertiesFromMLeaderStyle(GetBalloonStyle());
                                    PromptPointResult balloonLocation = currentEditor.GetPoint(new PromptPointOptions("Specify leader landing location"));
                                    balloon.BlockPosition = balloonLocation.Value;
                                    balloon.AddLeaderLine(selectedBlockReference.Position);

                                    BlockTableRecord leaderBlock = transaction.GetObject(balloon.BlockContentId, OpenMode.ForRead) as BlockTableRecord;
                                    Matrix3d transform = Matrix3d.Displacement(balloon.BlockPosition.GetAsVector());

                                    foreach (ObjectId blockEntityID in leaderBlock)
                                    {
                                        AttributeDefinition attDef = transaction.GetObject(blockEntityID, OpenMode.ForRead) as AttributeDefinition;
                                        if (attDef != null)
                                        {
                                            AttributeReference attRef = new AttributeReference();
                                            attRef.SetAttributeFromBlock(attDef, transform);
                                            attRef.Position = attDef.Position.TransformBy(transform);
                                            attRef.TextString = itemNumber;
                                            balloon.SetBlockAttribute(attDef.ObjectId, attRef);
                                        }
                                    }

                                    modelSpace.AppendEntity(balloon);
                                    transaction.AddNewlyCreatedDBObject(balloon, true);
                                    transaction.Commit();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UpdateBalloon(object senderObj, EventArgs eventArgs)
        {
            BlockReference blockReference = senderObj as BlockReference;
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Item Number: " + blockReference.GetXData("VIA_WD_P_ITEM"));
        }

        public MLeaderStyle GetBalloonStyle()
        {
            Document currentDocument = Application.DocumentManager.MdiActiveDocument;
            Database currentDatabase = currentDocument.Database;
            using (Transaction transaction = currentDatabase.TransactionManager.StartTransaction())
            {
                DBDictionary mLStyles = transaction.GetObject(currentDatabase.MLeaderStyleDictionaryId, OpenMode.ForRead) as DBDictionary;
                if (!mLStyles.Contains("Balloon"))
                {
                    MLeaderStyle balloonStyle = new MLeaderStyle
                    {
                        BlockId = GetBalloonBlock().ObjectId,
                        ContentType = ContentType.BlockContent,
                        EnableBlockRotation = false,
                        EnableBlockScale = false,
                        Name = "Balloon"
                    };
                    balloonStyle.PostMLeaderStyleToDb(currentDatabase, "Balloon");
                    transaction.AddNewlyCreatedDBObject(balloonStyle, true);
                    transaction.Commit();
                }
                return transaction.GetObject((ObjectId)(mLStyles["Balloon"]), OpenMode.ForRead) as MLeaderStyle;
            }
        }

        public BlockTableRecord GetBalloonBlock()
        {
            Document currentDocument = Application.DocumentManager.MdiActiveDocument;
            Database currentDatabase = currentDocument.Database;
            using (Transaction transaction = currentDatabase.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = transaction.GetObject(currentDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                ObjectId balloonBlockRecordId = ObjectId.Null;
                if (!blockTable.Has("BALLOON"))
                {
                    using (BlockTableRecord balloon = new BlockTableRecord())
                    {
                        balloon.Name = "BALLOON";
                        balloon.Origin = new Point3d(0, 0, 0);
                        using (Circle circle = new Circle())
                        {
                            circle.Center = new Point3d(0, 0, 0);
                            circle.Diameter = 0.62;
                            balloon.AppendEntity(circle);
                        }
                        using (AttributeDefinition attributeDefinition = new AttributeDefinition())
                        {
                            attributeDefinition.Tag = "ITEM_NUMBER";
                            attributeDefinition.TextStyleId = GetBalloonTextStyle().ObjectId;
                            attributeDefinition.Height = 0.125;
                            attributeDefinition.Position = new Point3d(0, 0, 0);
                            balloon.AppendEntity(attributeDefinition);
                        }
                        blockTable.UpgradeOpen();
                        blockTable.Add(balloon);
                        transaction.AddNewlyCreatedDBObject(balloon, true);
                        balloonBlockRecordId = balloon.Id;
                    }
                }
                return transaction.GetObject(blockTable["BALLOON"], OpenMode.ForRead) as BlockTableRecord;
            }
        }

        public TextStyleTableRecord GetBalloonTextStyle()
        {
            Document currentDocument = Application.DocumentManager.MdiActiveDocument;
            Database currentDatabase = currentDocument.Database;
            using (Transaction transaction = currentDatabase.TransactionManager.StartTransaction())
            {
                TextStyleTable textStyleTable = transaction.GetObject(currentDatabase.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                if (!textStyleTable.Has("ROMANS"))
                {
                    TextStyleTableRecord romans = new TextStyleTableRecord
                    {
                        Name = "ROMANS",
                        FileName = "romans.shx"
                    };
                    textStyleTable.UpgradeOpen();
                    textStyleTable.Add(romans);
                    transaction.AddNewlyCreatedDBObject(romans, true);
                    transaction.Commit();
                }
                return transaction.GetObject(textStyleTable["ROMANS"], OpenMode.ForRead) as TextStyleTableRecord;
            }
        }
    }

    public static class MLeaderExtenstions
    {
        //public static void SetPropertiesFromMLeaderStyle(this MLeader mLeader, MLeaderStyle mLeaderStyle)
        //{
        //    MLeaderStyle test = new MLeaderStyle();
        //    mLeader.MLeaderStyle = mLeaderStyle.ObjectId;
        //    mLeader.ContentType = mLeaderStyle.ContentType;
        //    mLeader.BlockContentId = mLeaderStyle.BlockId;
        //}
    }

    public static class BlockReferenceExtensions
    {
        public static AttributeReference GetAttributeReference(this BlockReference blockReference, string tag)
        {
            using (Transaction transaction = blockReference.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId attRefID in blockReference.AttributeCollection)
                {
                    AttributeReference attRef = transaction.GetObject(attRefID, OpenMode.ForRead) as AttributeReference;
                    if (attRef.Tag == tag)
                    {
                        return attRef;
                    }
                }
            }
            return null;
        }

        public static string GetXData(this BlockReference blockReference, string name)
        {
            bool nameFound = false;
            foreach (TypedValue value in blockReference.XData)
            {
                if (nameFound)
                {
                    if (value.TypeCode == 1000)
                    {
                        return value.Value.ToString();
                    }
                }
                else
                {
                    if (value.TypeCode == 1001 && value.Value.ToString() == name)
                    {
                        nameFound = true;
                    }
                }
            }
            return null;
        }

        public static string GetItemNumber(this BlockReference blockReference)
        {
            AttributeReference itemNumberAttRef;
            string itemNumber;

            itemNumberAttRef = blockReference.GetAttributeReference("ITEM");
            if (itemNumberAttRef != null)
            {
                itemNumber = itemNumberAttRef.TextString;
                if (itemNumberAttRef.TextString != "")
                {
                    return itemNumber;
                }
            }
            else
            {
                itemNumberAttRef = blockReference.GetAttributeReference("P_ITEM");
                if (itemNumberAttRef != null)
                {
                    itemNumber = itemNumberAttRef.TextString;
                    if (itemNumber != "")
                    {
                        return itemNumber;
                    }
                }
                else
                {
                    itemNumber = blockReference.GetXData("VIA_WD_ITEM");
                    if (itemNumber != "")
                    {
                        return itemNumber;
                    }
                    else
                    {
                        itemNumber = blockReference.GetXData("VIA_WD_P_ITEM");
                        if (itemNumber != "")
                        {
                            return itemNumber;
                        }
                    }
                }
            }
            return null;
        }
    }
}
