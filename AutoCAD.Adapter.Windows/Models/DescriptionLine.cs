using System;

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
    }
}
