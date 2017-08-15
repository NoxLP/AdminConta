using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using AdConta;
using Extensions;

namespace Converters
{
    public class NotifyTaskErrorMessagesCollectionsJoinMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.IsNullOrEmptyOrAllElementsAreNull() || targetType == null)
                return null;
            
            string initTaskError = "";
            int index = 0;
            foreach (object o in values)
            {
                if (o is string)
                    initTaskError = (string)o;

                index++;
            }

            IEnumerable<ObservableCollection<NotifyTask>> valuesArray = null;
            List<string> messages = null;

            if (!string.IsNullOrEmpty(initTaskError))
            {
                valuesArray = values
                    .Except(new string[] { initTaskError })
                    .Select(o => (ObservableCollection<NotifyTask>)o);
                messages = GetErrorMessages(valuesArray);
                messages.Insert(index, initTaskError);
            }
            else
            {
                valuesArray = values.Select(o => (ObservableCollection<NotifyTask>)o);
                messages = GetErrorMessages(valuesArray);
            }

            return string.Join(Environment.NewLine, messages);
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        

        private List<string> GetErrorMessages(IEnumerable<ObservableCollection<NotifyTask>> valuesArray)
        {
            List<string> messages = valuesArray
                .SelectMany(collection => collection.ToArray())
                .Select(task => task.ErrorMessage)
                .ToList();

            return messages;
        }
    }
}
