using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rules._EntityModel
{
    [Serializable]
    public class EntityNode : Helpers.N_AryTree<EntityModel>
    {
        public EntityNode() : base()
        {

        }

        public EntityNode(EntityModel Value, EntityNode Father = null, params EntityNode[] Children) : base(Value, Father, Children)
        {
        }

        public void AddChild(EntityNode child)
        {
            base.AddChild(child);
            if(child.Value != null)
            {
                child.Value.Base = Value;
            }
        }

        public void AddChild(EntityNode child, List<PropertyModel> props_list)
        {
            AddChild(child);
            foreach (PropertyModel prop in props_list)
                prop.Inherited = true;
        }

        public void RemoveChild(EntityNode child)
        {
            base.RemoveChild(child);
            child.Value.Base = null;
            foreach(PropertyModel prop in child.Value.Properties.Values)
                prop.Inherited = false;
        }

        public new void ClearChildren()
        {
            foreach(EntityNode child in Children)
            {
                child.Father = null;
                child.Value.Base = null;
                foreach (PropertyModel prop in child.Value.Properties.Values)
                    prop.Inherited = false;
            }

            Children.Clear();
        }
    }
}
