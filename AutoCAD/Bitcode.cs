using Autodesk.AutoCAD.ApplicationServices.Core;
using System;
using System.Collections;

namespace ICA.AutoCAD
{
    public class Bitcode
    {
        public string Name { get; }

        public Bitcode(string name)
        {
            Name = name;
        }

        public short Get() => (short)Application.GetSystemVariable(Name);

        public bool Get(int index) => new BitArray(new int[1] { Get() })[index];

        public void Set(short value) => Application.SetSystemVariable(Name, value);

        public void Set(int index, bool value)
        {
            if (Get(index) != value)
            {
                if (value)
                {
                    Set((short)(Get() + Math.Pow(2, index)));
                }
                else
                {
                    Set((short)(Get() - Math.Pow(2, index)));
                }
            }
        }
    }
}
