using ElectronicsInventoryApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
public class MainViewModel : INotifyPropertyChanged
{
    private readonly ComponentService _service;

    public ObservableCollection<ElectronicPart> ElectronicParts { get; } = new();
    private string _search = string.Empty;
    public string Search { get => _search; set { _search = value; OnPropertyChanged(); Refresh(); } }

    public MainViewModel(ComponentService service)
    {
        _service = service;
        Refresh();
    }

    public void Refresh()
    {
        ElectronicParts.Clear();
        foreach (var c in _service.Search(Search))
            ElectronicParts.Add(c);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}