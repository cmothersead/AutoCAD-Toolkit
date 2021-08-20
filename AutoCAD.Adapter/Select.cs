using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ICA.Schematic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter.Prompt
{
    public class Select
    {

        /// <summary>
        /// Prompts for selection of a schematic symbol from the current drawing, or selects the implied symbol
        /// </summary>
        /// <returns></returns>
        public static ISymbol Symbol(Editor editor)
        {
            if (SingleImplied(editor)?.Open() is BlockReference reference)
            {
                if (reference.HasAttributeReference("TAG1"))
                    return new ParentSymbol(reference);
                else if (reference.HasAttributeReference("TAG2"))
                    return new ChildSymbol(reference);
            }

            return null;
        }

        public static SignalSymbol Signal(Editor editor)
        {
            if (SingleImplied(editor)?.Open() is BlockReference reference)
                if (reference.HasAttributeReference("SIGCODE"))
                    return new SignalSymbol(reference);

            return null;
        }

        public static ObjectId? SingleImplied(Editor editor)
        {
            PromptSelectionResult selectionResult = editor.SelectImplied();
            if (selectionResult.Status == PromptStatus.Error)
            {
                PromptSelectionOptions selectionOptions = new PromptSelectionOptions
                {
                    SingleOnly = true
                };
                selectionResult = editor.GetSelection(selectionOptions);
            }
            else
                editor.SetImpliedSelection(new ObjectId[0]);

            if (selectionResult.Status == PromptStatus.OK)
                return selectionResult.Value.GetObjectIds()[0];

            return null;
        }

        public static ObjectIdCollection Multiple(Editor editor)
        {
            PromptSelectionResult selectionResult = editor.GetSelection();

            if (selectionResult.Status == PromptStatus.OK)
                return new ObjectIdCollection(selectionResult.Value.GetObjectIds());

            return null;
        }
    }
}
