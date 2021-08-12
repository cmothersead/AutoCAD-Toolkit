using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class ElectricalDocument
    {
        #region Private Properties

        private Document Document { get; }

        #endregion

        #region Private Fields

        #endregion

        #region Public Properties

        public ElectricalDocumentProperties Properties { get; set; }

        #endregion

        #region Constructors

        public ElectricalDocument(Document document)
        {
            Document = document;
            ElectricalDocumentProperties properties = new ElectricalDocumentProperties(WDM.Read(Document.Database));
        }

        #endregion

        #region Private Methods

        private string ReadElectricalProperty(string name)
        {
            throw new NotImplementedException();
        }

        private void WriteElectricalProperty(string name, string value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
