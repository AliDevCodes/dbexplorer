using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FastDbExplorer.Wpf.ViewModels;

namespace FastDbExplorer.Wpf.Views;

public partial class TableQueryView : UserControl
{
    private TableQueryViewModel? _viewModel;

    public TableQueryView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel is not null) _viewModel.ResultColumnsChanged -= RebuildColumns;
        _viewModel = e.NewValue as TableQueryViewModel;
        if (_viewModel is not null)
        {
            _viewModel.ResultColumnsChanged += RebuildColumns;
            RebuildColumns(_viewModel.ResultColumns);
        }
    }

    // Result columns are only known at run time, so the grid columns are created here (pure view logic).
    private void RebuildColumns(IReadOnlyList<string> names)
    {
        ResultGrid.Columns.Clear();
        var cellStyle = (Style)FindResource("CellText");
        for (var i = 0; i < names.Count; i++)
        {
            ResultGrid.Columns.Add(new DataGridTextColumn
            {
                Header = names[i],
                Binding = new Binding($"[{i}]"),
                ElementStyle = cellStyle,
                MinWidth = 90,
                MaxWidth = 420
            });
        }
    }
}
