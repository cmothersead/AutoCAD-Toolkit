using System;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter.Windows.Models
{
    public class DescriptionLine
    {
        private string _value;
        /// <summary>
        /// Text value of this description line. Forces all characters to uppercase
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
               if(string.IsNullOrEmpty(value))
                {
                    Emptied?.Invoke(this, new EventArgs());
                    return;
                }
                else if (string.IsNullOrEmpty(_value))
                {
                    Filled?.Invoke(this, new EventArgs());
                }
                _value = value.ToUpper();
            }
        }

        public DescriptionLine() { }
        public DescriptionLine(string value) => Value = value;

        /// <summary>
        /// Occurs when <see cref="Value"/> is assigned to be a null or empty string
        /// </summary>
        public event EventHandler Emptied;

        /// <summary>
        /// Occurs when <see cref="Value"/> takes on a non-null, non-empty value
        /// </summary>
        public event EventHandler Filled;

        public override int GetHashCode()
        {
            int hashCode = 1571931217;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_value);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        public static bool operator ==(DescriptionLine x, DescriptionLine y) => x.Equals(y);
        public static bool operator !=(DescriptionLine x, DescriptionLine y) => !x.Equals(y);

    }
}
