using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ICA.AutoCAD.Adapter.Windows.Models
{
    /// <summary>
    /// Represents an <see cref="ObservableCollection{DescriptionLine}"/>  with functionality to handle insertions and deletions automatically
    /// </summary>
    public class DescriptionCollection : ObservableCollection<DescriptionLine>
    {
        #region Public Properties

        public DescriptionLine CurrentLine { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DescriptionCollection()
        {
            AddEmpty();
        }

        /// <summary>
        /// Constructor to initialize from an <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="value"></param>
        public DescriptionCollection(IEnumerable<string> value)
        {
            AddEmpty();
            foreach(string item in value)
            {
                Add(item);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds <paramref name="item"/> to the collection
        /// </summary>
        /// <param name="item"></param>
        public new void Add(DescriptionLine item)
        {
            if (string.IsNullOrEmpty(item.Value))
                return;
            item.Emptied += DescriptionLine_Emptied;
            Insert(Count - 1, item);
        }

        /// <summary>
        /// Adds a new <see cref="DescriptionLine"/> with <see cref="DescriptionLine.Value"/> of <paramref name="value"/> to the collection
        /// </summary>
        /// <param name="value"></param>
        public void Add(string value)
        {
            Add(new DescriptionLine(value));
        }

        /// <summary>
        /// Unsubscribes <paramref name="item"/> from the <see cref="DescriptionLine_Emptied(object, EventArgs)"/> handler and removes it from the collection
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is successfully removed, otherwise <see langword="false"/></returns>
        public new bool Remove(DescriptionLine item)
        {
            item.Emptied -= DescriptionLine_Emptied;
            return base.Remove(item);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds an empty <see cref="DescriptionLine"/> to the end of the collection
        /// </summary>
        private void AddEmpty()
        {
            DescriptionLine empty = new DescriptionLine();
            empty.Filled += DescriptionLine_Filled;
            Insert(Count, empty);
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Removes <see cref="DescriptionLine"/> from the collection upon value being set to null or empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DescriptionLine_Emptied(object sender, EventArgs e)
        {
            Remove(sender as DescriptionLine);
        }

        /// <summary>
        /// Adds a new empty <see cref="DescriptionLine"/> to the end of the collection when the current one takes on a non-empty value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DescriptionLine_Filled(object sender, EventArgs e)
        {
            DescriptionLine d = sender as DescriptionLine;
            d.Filled -= DescriptionLine_Filled;
            d.Emptied += DescriptionLine_Emptied;
            AddEmpty();
        }

        #endregion
    }
}
