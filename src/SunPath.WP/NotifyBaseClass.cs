using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SunPath.WP
{
    public abstract class NotifyBaseClass : INotifyPropertyChanged
    {
        private readonly Dictionary<string, object> _propertyBackingDictionary = new Dictionary<string, object>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            Expression<Func<T>> current = property;

            PropertyHelper.NotifyPropertyChanged<T>(current, this, this.PropertyChanged);
        }

        protected void OnPropertyChanged<T>(params Expression<Func<T>>[] propertyExpressions)
        {
            foreach (var propertyExpression in propertyExpressions)
            {
                this.OnPropertyChanged<T>(propertyExpression);
            }
        }

        protected void OnPropertyChanged([CallerMemberName]string caller = null)
        {
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(caller));
            }
        }

        /// <summary>
        /// Return value for property from backing dictionary
        /// </summary>
        protected T Get<T>([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            object value;
            if (_propertyBackingDictionary.TryGetValue(propertyName, out value))
            {
                return (T)value;
            }

            return default(T);
        }

        /// <summary>
        /// Set property value to backing dictionary
        /// </summary>
        protected bool Set<T>(T newValue, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            if (EqualityComparer<T>.Default.Equals(newValue, Get<T>(propertyName))) return false;

            _propertyBackingDictionary[propertyName] = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Set property value to backing dictionary also when equality comparison compares to equal.
        /// </summary>
        protected bool BrutalForceSet<T>(T newValue, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            _propertyBackingDictionary[propertyName] = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises PropertyChanged event from the INotifyPropertyChanged interface and sets value of the property.
        /// </summary>
        protected void SetAndNotifyPropertyValue<T>(ref T field, T value, Expression<Func<T>> property)
        {
            PropertyHelper.SetValueAndNotifyPropertyChanged<T>(
                ref field, value, property, this, this.PropertyChanged);
        }
    }
}
