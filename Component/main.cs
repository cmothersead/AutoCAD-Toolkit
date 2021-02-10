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

namespace Component
{
    public class ComponentInstance
    {
        public BlockReference BlockReference;
        public AttributeReference Tag;
        public AttributeReference Family;
        public AttributeReference[] Desc = new AttributeReference[3];
        public AttributeReference Mfg;
        public AttributeReference Cat;
        public AttributeReference Inst;
        public AttributeReference Loc;

        [CommandMethod("TESTEDIT", CommandFlags.UsePickSet)]
        public static void OpenDialog()
        {
            ComponentInstance componentToEdit = new ComponentInstance();
            componentToEdit.Select();
            componentToEdit.Edit();
        }

        public void Edit()
        {
            EditComponentWindow editWindow = new EditComponentWindow();
            editWindow.Component = this;
            if(Desc[0].Invisible)
            {
                editWindow.Description_Checkbox.IsChecked = true;
            }
            if(Mfg.Invisible & Cat.Invisible)
            {
                editWindow.Catalog_Checkbox.IsChecked = true;
            }
            if(Inst.Invisible)
            {
                editWindow.Installation_Checkbox.IsChecked = true;
            }
            if((bool)Application.ShowModalWindow(editWindow))
            {
                if ((bool)editWindow.Description_Checkbox.IsChecked)
                {
                    Desc[0].Hide();
                    Desc[1].Hide();
                    Desc[2].Hide();
                }
                else
                {
                    Desc[0].Unhide();
                    Desc[1].Unhide();
                    Desc[2].Unhide();
                }
                if((bool)editWindow.Catalog_Checkbox.IsChecked)
                {
                    Mfg.Hide();
                    Cat.Hide();
                }
                else
                {
                    Mfg.Unhide();
                    Cat.Unhide();
                }
                if((bool)editWindow.Installation_Checkbox.IsChecked)
                {
                    Inst.Hide();
                    Loc.Hide();
                }
                else
                {
                    Inst.Unhide();
                    Loc.Unhide();
                }
                Desc[0].SetValue(editWindow.Description1_TextBox.Text);
                Desc[1].SetValue(editWindow.Description2_TextBox.Text);
                Desc[2].SetValue(editWindow.Description3_TextBox.Text);
                CollapseAttributeStack();
            }
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
                    PromptSelectionOptions selectionOptions = new PromptSelectionOptions();
                    selectionOptions.SingleOnly = true;
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
                                    Desc[0] = BlockReference.GetAttributeReference("DESC1");
                                    Desc[1] = BlockReference.GetAttributeReference("DESC2");
                                    Desc[2] = BlockReference.GetAttributeReference("DESC3");
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
