using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ICA.Schematic;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter.Prompt
{
    public static class Select
    {
        public static Component Component(Editor editor, string message = null)
        {
            if (SingleImplied(editor)?.Open() is BlockReference reference)
                if (reference.HasAttributeReference("TAG1"))
                    return new Component(new ParentSymbol(reference));

            return null;
        }

        /// <summary>
        /// Prompts for selection of a schematic symbol from the current drawing, or selects the implied symbol
        /// </summary>
        /// <returns></returns>
        public static ISymbol Symbol(Editor editor, string message = null)
        {
            if (SingleImplied(editor, message)?.Open() is BlockReference reference)
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

        public static List<ISymbol> Symbols(Editor editor, string message = null)
        {
            List<ISymbol> list = Multiple(editor, message)?.OfType<BlockReference>()
                                                           .Select(reference => FromReference(editor, reference))
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

        public static ObjectId? SingleImplied(Editor editor, string message = null)
        {
            PromptSelectionResult selectionResult = editor.SelectImplied();
            if (selectionResult.Status == PromptStatus.Error)
            {
                PromptSelectionOptions selectionOptions = new PromptSelectionOptions()
                {
                    MessageForAdding = message,
                    SingleOnly = true
                };
                selectionResult = editor.GetSelection(selectionOptions);
            }
            else
                editor.SetImpliedSelection(new ObjectId[0]);

            return selectionResult.Status == PromptStatus.OK ? (ObjectId?)selectionResult.Value.GetObjectIds()[0] : null;
        }

        public static IEnumerable<DBObject> Multiple(Editor editor, string message = null)
        {
            PromptSelectionOptions options = new PromptSelectionOptions { MessageForAdding = message };
            PromptSelectionResult selectionResult = editor.GetSelection(options);

            if (selectionResult.Status == PromptStatus.OK)
                return selectionResult.Value.GetObjectIds().Select(id => id.Open());

            return null;
        }

        public static List<DBObject> Implied(Editor editor)
        {
            PromptSelectionResult selectionResult = editor.SelectImplied();
            return selectionResult.Status == PromptStatus.OK ? selectionResult.Value.GetObjectIds().Select(id => id.Open()).ToList() : null;
        }

        public static Point2d? Point(Editor editor, string message = null)
        {
            PromptPointOptions options = new PromptPointOptions(message);
            PromptPointResult pointResult = editor.GetPoint(options);

            if (pointResult.Status == PromptStatus.OK)
                return pointResult.Value.ToPoint2D();

            return null;
        }
    }
}
