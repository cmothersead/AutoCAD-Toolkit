using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ICA.Schematic;

namespace ICA.AutoCAD.Adapter
{
    public class ParentSymbol : IParentSymbol
    {
        #region Private Properties

        private BlockReference BlockReference { get; }

        private AttributeReference TagAttribute => BlockReference.GetAttributeReference("TAG1");

        private AttributeReference FamilyAttribute => BlockReference.GetAttributeReference("FAMILY");

        private List<AttributeReference> DescAttributes
        {
            get
            {
                int index = 1;
                List<AttributeReference> list = new List<AttributeReference>();
                AttributeReference attributeReference;
                do
                {
                    attributeReference = BlockReference.GetAttributeReference($"DESC{index}");
                    if (attributeReference != null)
                    {
                        list.Add(attributeReference);
                        index++;
                    }
                } while (attributeReference != null);
                return list;
            }
        }

        private AttributeReference MfgAttribute => BlockReference.GetAttributeReference("MFG");

        private AttributeReference CatAttribute => BlockReference.GetAttributeReference("CAT");

        private AttributeReference InstAttribute => BlockReference.GetAttributeReference("INST");

        private AttributeReference LocAttribute => BlockReference.GetAttributeReference("LOC");

        #endregion

        #region Public Properties

        public string Tag
        {
            get => TagAttribute.TextString;
            set
            {
                TagAttribute.SetValue(value);
            }
        }

        public string Family
        {
            get
            {
                return FamilyAttribute.TextString;
            }
            set
            {
                FamilyAttribute.SetValue(value);
            }
        }

        public string ManufacturerName
        {
            get => MfgAttribute.TextString;
            set
            {
                MfgAttribute.SetValue(value);
            }
        }

        public string PartNumber
        {
            get => CatAttribute.TextString;
            set
            {
                CatAttribute.SetValue(value);
            }
        }

        public string Enclosure
        {
            get => InstAttribute.TextString;
            set
            {
                InstAttribute.SetValue(value);
            }
        }

        public string Location
        {
            get => LocAttribute.TextString;
            set
            {
                LocAttribute.SetValue(value);
            }
        }

        public List<string> Description
        {
            get
            {
                List<string> list = new List<string>();
                foreach (AttributeReference attributeReference in DescAttributes)
                {
                    list.Add(attributeReference.TextString);
                }
                return list;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool DescriptionHidden
        {
            get
            {
                return DescAttributes[0].Invisible;
            }
            set
            {
                foreach (AttributeReference attributeReference in DescAttributes)
                {
                    if (value)
                    {
                        attributeReference.Hide();
                    }
                    else
                    {
                        attributeReference.Unhide();
                    }

                }
            }
        }

        public bool InstallationHidden
        {
            get
            {
                return InstAttribute.Invisible;
            }
            set
            {
                if (value)
                {
                    InstAttribute.Hide();
                    LocAttribute.Hide();
                }
                else
                {
                    InstAttribute.Unhide();
                    LocAttribute.Unhide();
                }
            }
        }

        public bool PartInfoHidden
        {
            get
            {
                return MfgAttribute.Invisible;
            }
            set
            {
                if (value)
                {
                    MfgAttribute.Hide();
                    CatAttribute.Hide();
                }
                else
                {
                    MfgAttribute.Unhide();
                    CatAttribute.Unhide();
                }
            }
        }

        #endregion

        #region Construtctor

        public ParentSymbol(BlockReference blockReference) => BlockReference = blockReference;

        #endregion

        #region Public Methods

        public void CollapseAttributeStack()
        {
            CollapseAttributeStack(TagAttribute.AlignmentPoint);
        }

        public void CollapseAttributeStack(Point3d position)
        {
            List<AttributeReference> list = DescAttributes;
            list.Add(MfgAttribute);
            list.Add(CatAttribute);
            list.Add(TagAttribute);
            list.Reverse();
            foreach (AttributeReference attributeReference in list)
            {
                attributeReference.SetPosition(position);
                if (!(attributeReference.Invisible | attributeReference.TextString == ""))
                {
                    position = new Point3d(position.X, position.Y + 0.15625, position.Z);
                }
            }
        }

        #endregion
    }
}
