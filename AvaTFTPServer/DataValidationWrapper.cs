using Avalonia.Data.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaTFTPServer
{
    internal class DataValidationWrapper(IDataValidationPlugin wrapped) : IDataValidationPlugin
    {
        public bool Match(WeakReference<object?> reference, string memberName)
        {
            return wrapped.Match(reference, memberName);
        }

        public IPropertyAccessor Start(WeakReference<object?> reference, string propertyName, IPropertyAccessor inner)
        {
            return wrapped.Start(reference, propertyName, inner);
        }
    }
}
