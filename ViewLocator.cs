using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Wrench.ViewModels;

namespace Wrench;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        string? name = default;
        Type? type = default;

        if (data is not null)
        {
            name = data.GetType().FullName!.Replace("ViewModel", "View");
            type = Type.GetType(name);

            if (type is not null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}