using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD.Adapter
{
    public class SymbolTranformOverrule : TransformOverrule
    {
        public override void TransformBy(Entity entity, Matrix3d transform)
        {
            base.TransformBy(entity, transform);
            if (transform == Matrix3d.Identity | entity.Id == ObjectId.Null)
                return;

            if (entity is BlockReference reference)
                if (reference.Layer == ElectricalLayers.SymbolLayer.Name)
                    if (reference.HasAttributeReference("TAG1"))
                        new Component(new ParentSymbol(reference)).UpdateTag();
        }
    }
}
