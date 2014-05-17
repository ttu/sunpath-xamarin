using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SunPath.WP
{
    /// <summary>
    /// Static class that helps on notifying property changes on objects
    /// which implement INotifyPropertyChanged interface.
    /// </summary>
    public static class PropertyHelper
    {
        /// <summary>
        /// Returns name of the property passed as a Linq Expression Func.
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="propertyExpression">test passed as Linq Expression</param>
        /// <returns>test name</returns>
        public static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            MemberExpression body = propertyExpression.Body as MemberExpression;

            if (body.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException("Argument type isn't property.", "propertyExpression");

            if (body == null || body.Member == null)
                throw new ArgumentException("Body or member is missing.", "propertyExpression");

            return body.Member.Name;
        }

        /// <summary>
        /// Notify property change to all event listeners on current object instance.
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="property">test passed as Linq Expression</param>
        /// <param name="instance">Object instance which has been changed.</param>
        /// <param name="handler">test changed event handler instance of the object instance specified.</param>
        public static void NotifyPropertyChanged<T>(
            Expression<Func<T>> property, object instance, PropertyChangedEventHandler handler)
        {
            // NOTE: These null checks are removed due to performance reasons

            //if (property == null)
            //    throw new ArgumentNullException("property");
            //if (instance == null)
            //    throw new ArgumentNullException("instance");

            if (handler != null)
            {
                string propertyName = GetPropertyName<T>(property);

                //if (string.IsNullOrEmpty(propertyName))
                //    throw new ArgumentException("Couldn't determine property name.", "property");

                handler(instance, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Sets value of the property field and notifies property change to all event listeners on
        /// current object instance.
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="field">field which contains property value</param>
        /// <param name="value">value which will be set to field</param>
        /// <param name="property">test passed as Linq Expression</param>
        /// <param name="instance">Object instance which has been changed.</param>
        /// <param name="handler">test changed event handler instance of the object instance specified.</param>
        public static void SetValueAndNotifyPropertyChanged<T>(
            ref T field, T value, Expression<Func<T>> property, object instance, PropertyChangedEventHandler handler)
        {
            field = value;

            NotifyPropertyChanged<T>(property, instance, handler);
        }
    }
}
