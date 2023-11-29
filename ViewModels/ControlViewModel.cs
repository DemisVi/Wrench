using System;
using Avalonia.Controls;

namespace Wrench.ViewModels;

public class ControlViewModel : ViewModelBase
{
    public ComboBoxItem[] ComboBoxItems { get; set; } = new[] {
        new ComboBoxItem() {Content = "CimCom-pedro-fit"},
        new ComboBoxItem() {Content = "Telit-ultra-dub"},
        new ComboBoxItem() {Content = "Telit-ultra-dub"},
        new ComboBoxItem() {Content = "Telit-ultra-dub"},
        new ComboBoxItem() {Content = "Telit-ultra-dub"},
        new ComboBoxItem() {Content = "Telit-ultra-dub"},
        new ComboBoxItem() {Content = "SimSim-used-simp"}
        };
}
