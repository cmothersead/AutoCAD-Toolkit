using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class GroupGripOverrule : GripOverrule
    {
        internal class Grip : GripData
        {
            public Grip() { }

            public Grip(Point2d point)
            {
                GripPoint = point.ToPoint3d();
            }
        }

        public override void GetGripPoints(Entity entity, GripDataCollection grips, double curViewUnitSize, int gripSize, Vector3d curViewDir, GetGripPointsFlags bitFlags)
        {
            if(!entity.HasGroup())
            {
                try
                {
                    base.GetGripPoints(entity, grips, curViewUnitSize, gripSize, curViewDir, bitFlags);
                }
                catch { }
                return;
            }

            Group group = entity.GetGroups()
                                .FirstOrDefault();

            BlockReference primary = group.GetEntities()
                                          .OfType<BlockReference>()
                                          .OrderBy(reference => reference.Position.Y)
                                          .LastOrDefault();

            grips.Add(new Grip(primary.Position.ToPoint2D()));
        }

        public override void MoveGripPointsAt(Entity entity, GripDataCollection grips, Vector3d offset, MoveGripPointsFlags bitFlags)
        {
            if(entity.Database != null && !entity.HasGroup())
            {
                base.MoveGripPointsAt(entity, grips, offset, bitFlags);
                return;
            }

            entity.TransformBy(Matrix3d.Displacement(offset));
        }
    }
}
