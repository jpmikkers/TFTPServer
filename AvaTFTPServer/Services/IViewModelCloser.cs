using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaTFTPServer;

public interface IViewModelCloser
{
    void Close(INotifyPropertyChanged vm);
    void Close<TResult>(INotifyPropertyChanged vm, TResult result);
}

public class ViewModelCloser(IViewResolver viewResolver) : IViewModelCloser
{
    public void Close(INotifyPropertyChanged vm)
    {
        viewResolver.LocateView(vm).Close();
    }

    public void Close<TResult>(INotifyPropertyChanged vm, TResult result)
    {
        viewResolver.LocateView(vm).Close(result);
    }
}
