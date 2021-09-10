using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using ICA.Schematic;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter.Prompt
{
    public static class Select
    {

        /// <summary>
        /// Prompts for selection of a schematic symbol from the current drawing, or selects the implied symbol
        /// </summary>
        /// <returns></returns>
        public static ISymbol Symbol(Editor editor)
        {
            if (SingleImplied(editor)?.Open() is BlockReference reference)
                return FromReference(editor, reference);

            return null;
        }

        private static ISymbol FromReference(Editor editor, BlockReference reference)
        {
            if (reference.HasAttributeReference("TAG1"))
                return new ParentSymbol(reference);
            else if (reference.HasAttributeReference("TAG2"))
                return new ChildSymbol(reference);
            else
            {
                PromptKeywordOptions options = new PromptKeywordOptions("\nSymbol type: ");
                options.Keywords.Add("Parent");
                options.Keywords.Add("Child");
                options.Keywords.Default = "Parent";

                PromptResult result = editor.GetKeywords(options);
                if (result.Status != PromptStatus.OK)
                    return null;

                switch (result.StringResult)
                {
                    case "Parent":
                        return new ParentSymbol(reference);
                    case "Child":
                        return new ChildSymbol(reference);
                }
            }
            return null;
        }

        public static List<ISymbol> Symbols(Editor editor)
        {
            List<ISymbol> list = Multiple(editor).Where(obj => obj is BlockReference)
                                           .Select(reference => FromReference(editor, (BlockReference)reference))
                                           .ToList();
            return list;
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
                    SingleOnly = true,
                };
                selectionResult = editor.GetSelection(selectionOptions);
            }
            else
                editor.SetImpliedSelection(new ObjectId[0]);

            return selectionResult.Status == PromptStatus.OK ? (ObjectId?)selectionResult.Value.GetObjectIds()[0] : null;
        }

        public static List<DBObject> Multiple(Editor editor)
        {
            PromptSelectionResult selectionResult = editor.GetSelection();

            if (selectionResult.Status == PromptStatus.OK)
                return selectionResult.Value.GetObjectIds().Select(id => id.Open()).ToList();

            return null;
        }
    }
}
