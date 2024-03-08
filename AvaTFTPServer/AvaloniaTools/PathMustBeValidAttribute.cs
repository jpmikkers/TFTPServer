using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Baksteen.Avalonia.Tools;

public sealed class PathMustBeValidAttribute : ValidationAttribute
{
    public PathMustBeValidAttribute()
    {
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if(value is string p)
        {
            try
            {
                if(Directory.Exists(Path.GetFullPath(p)))
                {
                    return ValidationResult.Success!;
                }
            }
            catch { }
        }

        return new ValidationResult("path is not valid");
    }
}
