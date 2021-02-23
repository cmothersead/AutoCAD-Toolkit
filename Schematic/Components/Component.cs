using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace ICA.Schematic.Components
{
    public class Component
    {
        public BlockReference BlockReference { get; set; }
        public AttributeReference Tag { get; set; }
        public AttributeReference Family { get; set; }
        public List<AttributeReference> Desc { get; set; }
        public AttributeReference Mfg { get; set; }
        public AttributeReference Cat { get; set; }
        public AttributeReference Inst { get; set; }
        public AttributeReference Loc { get; set; }

        [CommandMethod("EDITCOMPONENT", CommandFlags.UsePickSet)]
        public static void OpenDialog()
        {
            Component componentToEdit = new Component();
            componentToEdit.Select();
            componentToEdit.Edit();
        }

        public void Edit()
        {
            EditWindow editWindow = new EditWindow
            {
                Component = this
            };
            Application.ShowModalWindow(editWindow);
        }

        public void Select()
        {
            Document currentDocument = Application.DocumentManager.MdiActiveDocument;
            Editor currentEditor = currentDocument.Editor;
            try
            {
                PromptSelectionResult selectionResult = currentEditor.SelectImplied();
                if (selectionResult.Status == PromptStatus.Error)
                {
                    PromptSelectionOptions selectionOptions = new PromptSelectionOptions
                    {
                        SingleOnly = true
                    };
                    selectionResult = currentEditor.GetSelection(selectionOptions);
                }
                else
                {
                    currentEditor.SetImpliedSelection(new ObjectId[0]);
                }

                if (selectionResult.Status == PromptStatus.OK)
                {
                    try
                    {
                        ObjectId[] objectIds = selectionResult.Value.GetObjectIds();
                        foreach (ObjectId objectId in objectIds)
                        {
                            if (objectId.ObjectClass.Name == "AcDbBlockReference")
                            {
                                using (Transaction transaction = currentDocument.TransactionManager.StartTransaction())
                                {
                                    BlockReference = (BlockReference)transaction.GetObject(objectId, OpenMode.ForRead);
                                    Tag = BlockReference.GetAttributeReference("TAG1");
                                    Family = BlockReference.GetAttributeReference("FAMILY");
                                    Mfg = BlockReference.GetAttributeReference("MFG");
                                    Cat = BlockReference.GetAttributeReference("CAT");
                                    Desc = new List<AttributeReference>
                                    {
                                        BlockReference.GetAttributeReference("DESC1"),
                                        BlockReference.GetAttributeReference("DESC2"),
                                        BlockReference.GetAttributeReference("DESC3")
                                    };
                                    Inst = BlockReference.GetAttributeReference("INST");
                                    Loc = BlockReference.GetAttributeReference("LOC");
                                }
                            }
                        }
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        currentEditor.WriteMessage(ex.Message);
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                currentEditor.WriteMessage(ex.Message);
            }
        }

        public void CollapseAttributeStack()
        {
            Cat.SetPosition(NextPosition(Tag.AlignmentPoint));


            if(Cat.Invisible | Cat.TextString ==  "")
            {
                Mfg.SetPosition(Cat.AlignmentPoint);
            }
            else
            {
                Mfg.SetPosition(NextPosition(Cat.AlignmentPoint));
            }

            if (Mfg.Invisible | Mfg.TextString == "")
            {
                Desc[2].SetPosition(Mfg.AlignmentPoint);
            }
            else
            {
                Desc[2].SetPosition(NextPosition(Mfg.AlignmentPoint));
            }

            if (Desc[2].Invisible | Desc[2].TextString == "")
            {
                Desc[1].SetPosition(Desc[2].AlignmentPoint);
            }
            else
            {
                Desc[1].SetPosition(NextPosition(Desc[2].AlignmentPoint));
            }

            if (Desc[1].Invisible | Desc[1].TextString == "")
            {
                Desc[0].SetPosition(Desc[1].AlignmentPoint);
            }
            else
            {
                Desc[0].SetPosition(NextPosition(Desc[1].AlignmentPoint));
            }
        }

        private static Point3d NextPosition (Point3d previousPosition)
        {
            return new Point3d(previousPosition.X, previousPosition.Y + 0.15625, previousPosition.Z);
        }
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
    }

    public static class AttributeReferenceExtensions
    {
        public static bool Hide(this AttributeReference attributeReference)
        {
            try
            {
                using (Transaction transaction = attributeReference.Database.TransactionManager.StartTransaction())
                {

                    AttributeReference attributeReferenceForWrite = transaction.GetObject(attributeReference.ObjectId, OpenMode.ForWrite) as AttributeReference;
                    attributeReferenceForWrite.Invisible = true;
                    transaction.Commit();
                    return true;

                }
            }
            catch
            {
                return false;
            }
        }

        public static bool Unhide(this AttributeReference attributeReference)
        {
            try
            {
                using (Transaction transaction = attributeReference.Database.TransactionManager.StartTransaction())
                {

                    AttributeReference atttributeReferenceForWrite = transaction.GetObject(attributeReference.ObjectId, OpenMode.ForWrite) as AttributeReference;
                    atttributeReferenceForWrite.Invisible = false;
                    transaction.Commit();
                    return true;

                }
            }
            catch
            {
                return false;
            }
        }

        public static bool SetValue(this AttributeReference attributeReference, string value)
        {
            try
            {
                using (Transaction transaction = attributeReference.Database.TransactionManager.StartTransaction())
                {
                    AttributeReference attributeReferenceForWrite = transaction.GetObject(attributeReference.ObjectId, OpenMode.ForWrite) as AttributeReference;
                    attributeReferenceForWrite.TextString = value;
                    transaction.Commit();
                    return true;

                }
            }
            catch
            {
                return false;
            }
        }

        public static bool SetPosition(this AttributeReference attributeReference, Point3d position)
        {
            try
            {
                using (Transaction transaction = attributeReference.Database.TransactionManager.StartTransaction())
                {

                    AttributeReference attributeReferenceForWrite = transaction.GetObject(attributeReference.ObjectId, OpenMode.ForWrite) as AttributeReference;
                    attributeReferenceForWrite.AlignmentPoint = position;
                    transaction.Commit();
                    return true;

                }
            }
            catch
            {
                return false;
            }
        }
    }
}
