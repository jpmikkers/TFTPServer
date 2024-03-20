using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaTFTPServer.AvaloniaTools;

public static class Assertions
{
    public static void OnlyUseEmptyConstructorInDesignMode(string className)
    {
        if(!Design.IsDesignMode) throw new InvalidOperationException($"empty constructor for '{className}' should only be used in design mode");
    }
}
