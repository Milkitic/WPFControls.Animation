using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace System.Windows.Controls.Animation
{
    internal class FindVisualChildHelper
    {
        public static T GetFirstChildOfType<T>(DependencyObject obj) where T : DependencyObject
        {
            // Iterate through all immediate children
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child is T dependencyObject)
                    return dependencyObject;
                var childOfChild = GetFirstChildOfType<T>(child);

                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }

        public static T GetFirstParentOfType<T>(DependencyObject obj) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            if (parent is T dependencyObject)
                return dependencyObject;
            var parentOfParent = GetFirstParentOfType<T>(parent);

            if (parentOfParent != null)
                return parentOfParent;

            return null;
        }
    }
}
