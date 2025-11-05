using System.ComponentModel;
using System.Runtime.CompilerServices;
public class AddEditComponentViewModel : INotifyPropertyChanged
{
    public Component Model { get; }
    public AddEditComponentViewModel(Component? model = null)
    {
        Model = model ?? new Component();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}