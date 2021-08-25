using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD
{
    public static class AttributeDefinitionExtensions
    {
        #region Public Extension Methods

        public static void Hide(this AttributeDefinition attributeDefinition, Transaction transaction) => attributeDefinition.GetForWrite(transaction).Invisible = true;

        public static void Unhide(this AttributeDefinition attributeDefinition, Transaction transaction) => attributeDefinition.GetForWrite(transaction).Invisible = false;

        public static void ToggleVisibility(this AttributeDefinition attributeDefinition, Transaction transaction) => attributeDefinition.GetForWrite(transaction).Invisible ^= true;

        public static void SetVisibility(this AttributeDefinition attributeDefinition, Transaction transaction, bool value) => attributeDefinition.GetForWrite(transaction).Invisible = !value;

        #endregion

        #region Transacted Overloads

        public static void Hide(this AttributeDefinition attributeDefinition) => attributeDefinition.Transact(Hide);

        public static void Unhide(this AttributeDefinition attributeDefinition) => attributeDefinition.Transact(Unhide);

        public static void ToggleVisibility(this AttributeDefinition attributeDefinition) => attributeDefinition.Transact(ToggleVisibility);

        public static void SetVisibility(this AttributeDefinition attributeDefinition, bool value) => attributeDefinition.Transact(SetVisibility, value);

        #endregion
    }
}
