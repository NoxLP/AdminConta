using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Extensions
{
    public static class FrameworkElementExtension
    {
        /// <summary>
        /// Get for first parent FrameworkEelement of type T in visual tree .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="child"></param>
        /// <returns></returns>
        public static T FindFirstParentOfType<T>(this FrameworkElement child) where T : FrameworkElement
        {
            var parent = VisualTreeHelper.GetParent(child);

            while (!(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is T) return (T)parent;
            else return null;
        }
        /// <summary>
        /// Get for first parent FrameworkEelement of type T AND name in visual tree .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="child"></param>
        /// <returns></returns>
        public static T FindFirstParentOfType<T>(this FrameworkElement child, string name) where T : FrameworkElement
        {
            FrameworkElement parent = VisualTreeHelper.GetParent(child) as FrameworkElement;

            while (parent.Name != name && (!(parent is T)))
            {
                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            }

            if (parent is T) return (T)parent;
            else return null;
        }

        //http://stackoverflow.com/questions/9784038/get-and-iterate-through-controls-from-a-tabitem
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject rootObject) where T : DependencyObject
        {
            if (rootObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(rootObject); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(rootObject, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        public static T FindFirstVisualChildOfType<T>(this DependencyObject rootObject) where T : DependencyObject
        {
            if (rootObject == null || VisualTreeHelper.GetChildrenCount(rootObject) == 0) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(rootObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(rootObject, i);

                if (child != null && child is T)
                    return (T)child;

                if (VisualTreeHelper.GetChildrenCount(child) != 0)
                {
                    DependencyObject childOfChild = child.FindFirstVisualChildOfType<T>();

                    if (childOfChild != null)
                        return (T)childOfChild;
                }
            }

            return null;
        }

        public static T FindVisualChild<T>(this DependencyObject rootObject, T childToFind) where T : DependencyObject
        {
            if (rootObject == null || VisualTreeHelper.GetChildrenCount(rootObject) == 0) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(rootObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(rootObject, i);

                if (child != null && child == childToFind)
                    return (T)child;

                if (VisualTreeHelper.GetChildrenCount(child) != 0)
                {
                    DependencyObject childOfChild = child.FindVisualChild<T>(childToFind);

                    if (childOfChild != null)
                        return (T)childOfChild;
                }
            }

            return null;
        }

        public static T FindVisualChild<T>(this DependencyObject rootObject, Func<object, bool> condition) where T : DependencyObject
        {
            if (rootObject == null || VisualTreeHelper.GetChildrenCount(rootObject) == 0) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(rootObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(rootObject, i);

                if (child != null && child is T && condition(child))
                    return (T)child;

                if (VisualTreeHelper.GetChildrenCount(child) != 0)
                {
                    DependencyObject childOfChild = child.FindVisualChild<T>(condition);

                    if (childOfChild != null)
                        return (T)childOfChild;
                }
            }

            return null;
        }
    }
}