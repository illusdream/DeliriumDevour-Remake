namespace ilsActionEditor
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class NodeMenuItemAttribute : System.Attribute
    {
        public string menuItemName;

        public NodeMenuItemAttribute(string menuItemName)
        {
            this.menuItemName = menuItemName;
        }
    }
}