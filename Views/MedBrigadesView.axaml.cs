using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace VAMedicalCallClient.Views;

public partial class MedBrigadesView : UserControl
{
    public MedBrigadesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
