using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public class SymbolGripOverrule : GripOverrule
    {
        private ObjectId _prevId;
        private List<GripData> _prevGrips;

        public SymbolGripOverrule()
        {
            ReferenceMoved += SymbolGripOverrule_ReferenceMoved;
        }

        private void SymbolGripOverrule_ReferenceMoved(object sender, EventArgs e)
        {
            if (sender is BlockReference reference)
                if (reference.Layer == ElectricalLayers.SymbolLayer.Name && reference.HasAttributeReference("TAG1"))
                    new Component(new ParentSymbol(reference)).UpdateTag();
        }

        public override void GetGripPoints(Entity entity, GripDataCollection grips, double curViewUnitSize, int gripSize, Vector3d curViewDir, GetGripPointsFlags bitFlags)
        {
            base.GetGripPoints(entity, grips, curViewUnitSize, gripSize, curViewDir, bitFlags);
            List<GripData> forRemoval = new List<GripData>();
            if (entity is BlockReference reference)
                if (reference.Layer == ElectricalLayers.SymbolLayer.Name)
                {
                    grips.Where(grip => grip.GripPoint != reference.Position)
                         .ForEach(grip => grips.Remove(grip));
                    if (_prevId != null && _prevGrips != null && _prevGrips.Count == 1)
                        if (entity.Id == _prevId)
                            if (grips[0].GripPoint != _prevGrips[0].GripPoint)
                                ReferenceMoved?.Invoke(reference, new EventArgs());
                    _prevId = entity.Id;
                    _prevGrips = grips.ToList();
                    reference.Highlight();
                }
        }

        public event EventHandler ReferenceMoved;
    }
}
