namespace AddFamilyParameters.Static_classes
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;

    public static class UtilityClass
    {
        public static void ExpandAll(this TreeViewItem treeViewItem, bool isExpanded = true)
        {
            var stack = new Stack<TreeViewItem>(treeViewItem.Items.Cast<TreeViewItem>());
            while (stack.Count > 0)
            {
                TreeViewItem item = stack.Pop();

                foreach (var child in item.Items)
                {
                    var childContainer = child as TreeViewItem ?? item.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;

                    stack.Push(childContainer);
                }

                item.IsExpanded = isExpanded;
            }
        }

        public static void CollapseAll(this TreeViewItem treeViewItem)
        {
            treeViewItem.ExpandAll(false);
        }
    }
}