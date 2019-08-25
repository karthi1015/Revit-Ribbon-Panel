// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemHelper.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   The item helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AddFamilyParameters.HelperClass
{
    using System.Linq;
    using System.Windows;

    using AddFamilyParameters.Interfaces;

    /// <summary>
    /// The item helper.
    /// </summary>
    public class ItemHelper : DependencyObject
    {
        /// <summary>
        /// The is checked property.
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.RegisterAttached("IsChecked", typeof(bool?), typeof(ItemHelper), new PropertyMetadata(false, new PropertyChangedCallback(OnIsCheckedPropertyChanged)));

        /// <summary>
        /// The parent property.
        /// </summary>
        public static readonly DependencyProperty ParentProperty = DependencyProperty.RegisterAttached("Parent", typeof(object), typeof(ItemHelper));

        /// <summary>
        /// The set is checked.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="isChecked">
        /// The is checked.
        /// </param>
        public static void SetIsChecked(DependencyObject element, bool? isChecked)
        {
            element?.SetValue(IsCheckedProperty, isChecked);
        }

        /// <summary>
        /// The get is checked.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// The <see cref="bool?"/>.
        /// </returns>
        public static bool? GetIsChecked(DependencyObject element)
        {
            return (bool?)element?.GetValue(IsCheckedProperty);
        }

        /// <summary>
        /// The set parent.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        public static void SetParent(DependencyObject element, object parent)
        {
            element?.SetValue(ParentProperty, parent);
        }

        /// <summary>
        /// The get parent.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public static object GetParent(DependencyObject element)
        {
            return element?.GetValue(ParentProperty);
        }

        private static void OnIsCheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IParent<object> sect = d as IParent<object>;
            DependencyObject depObj = d;

            if (sect != null)
            {
                if (((bool?)e.NewValue).HasValue)
                {
                    foreach (DependencyObject p in sect.GetChildren())
                    {
                        SetIsChecked(p, (bool?)e.NewValue);
                    }
                }
            }

            if (depObj != null)
            {
                var parentObject = depObj.GetValue(ParentProperty) as IParent<object>;
                var parentDependencyObject = depObj.GetValue(ParentProperty) as DependencyObject;
                int ch = parentObject?.GetChildren()?.Where(x => GetIsChecked(x as DependencyObject) == true).Count() ?? 0;
                int un = parentObject?.GetChildren()?.Where(x => GetIsChecked(x as DependencyObject) == false).Count() ?? 0;
                if ((un > 0) && (ch > 0))
                {
                    SetIsChecked(parentDependencyObject, null);
                    return;
                }

                if (ch > 0)
                {
                    SetIsChecked(parentDependencyObject, true);
                    return;
                }

                SetIsChecked(parentDependencyObject, false);
            }
        }
    }
}