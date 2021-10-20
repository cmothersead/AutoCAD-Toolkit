using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class EraseLinksOverrule : ObjectOverrule
    {
        public override void Erase(DBObject dbObject, bool erasing)
        {
            if (dbObject.HasXData() && dbObject is Entity entity)
            {
                Graph<EntityNode, Entity> linkGraph = new Graph<EntityNode, Entity>(new EntityNode(entity));
                linkGraph.RemoveNode(new EntityNode(entity));
            }

            base.Erase(dbObject, erasing);
        }
    }
}
