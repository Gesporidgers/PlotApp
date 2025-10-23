using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using PlotApp.Dialogs;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PlotApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private ViewModel _viewModel = new ViewModel();
		public MainWindow()
        {
            InitializeComponent();
		}

		private void dataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
		{
            if (e.Key == Windows.System.VirtualKey.Delete)
            {
                _viewModel.DeleteRow(dataGrid.SelectedIndex);
            }
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			FileOpenPicker picker = new FileOpenPicker((sender as Button).XamlRoot.ContentIslandEnvironment.AppWindowId);
			picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
			picker.FileTypeFilter.Add(".csv");

			var file = await picker.PickSingleFileAsync();
			_viewModel.Import(file.Path);
		}

		private async void EnterX_Click(object sender, RoutedEventArgs e)
		{
			ContentDialog contentDialog = new ContentDialog();
			contentDialog.XamlRoot = (sender as MenuFlyoutItem).XamlRoot;
			contentDialog.Name = "X";
			contentDialog.Title = "Введите название оси X";
			contentDialog.Content = new EnterName();
			contentDialog.PrimaryButtonText = "OK";
			contentDialog.PrimaryButtonClick += EnterAxisName;
			contentDialog.SecondaryButtonText = "Отмена";
			await contentDialog.ShowAsync();
		}
		private async void EnterY_Click(object sender, RoutedEventArgs e)
		{
			ContentDialog contentDialog = new ContentDialog();
			contentDialog.XamlRoot = (sender as MenuFlyoutItem).XamlRoot;
			contentDialog.Name = "Y";
			contentDialog.Title = "Введите название оси Y";
			contentDialog.Content = new EnterName();
			contentDialog.PrimaryButtonText = "OK";
			contentDialog.PrimaryButtonClick += EnterAxisName;
			contentDialog.SecondaryButtonText = "Отмена";
			await contentDialog.ShowAsync();
		}

		private void EnterAxisName(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			if (sender is ContentDialog dialog)
			{
				var content = dialog.Content as EnterName;
				switch (dialog.Name)
				{
					case "X":
						{
							ICartesianAxis[] a = _viewModel.XAxis.ToArray();
							a[0].Name = content.EnteredText;
							_viewModel.XAxis = a;
							break;
						}
					case "Y":
						{
							ICartesianAxis[] a = _viewModel.YAxis.ToArray();
							a[0].Name = content.EnteredText;
							_viewModel.YAxis = a;
							break;
						}
				}
			}
		}
	}

    
}
