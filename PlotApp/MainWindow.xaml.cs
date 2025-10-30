using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using PlotApp.Dialogs;
using PlotApp.Model;
using PlotApp.Util;
using ScottPlot;
using ScottPlot.AxisPanels;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
		private ViewModel _viewModel;
		int status = 0;
		public MainWindow()
		{
			InitializeComponent();
			_viewModel = new ViewModel(ref mainplot);
			mainplot.Plot.Legend.FontName = ClassParameters.FontName;
			mainplot.Plot.Legend.Alignment = Alignment.LowerCenter;
			mainplot.Plot.Legend.FontSize = ClassParameters.LegendFontSize;
			IEnumerable<IPlottable> scatters;
			mainplot.PointerMoved += (s, e) => 
			{
				scatters = mainplot.Plot.PlottableList.Where((i) => i.GetType() == typeof(Scatter));
				if (scatters.Count() > 0)
				{
					var point = e.GetCurrentPoint(mainplot).Position;
					Coordinates position = mainplot.Plot.GetCoordinates((float)point.X, (float)point.Y);
					DataPoint nearest = (mainplot.Plot.PlottableList[_viewModel.PlotIndex] as Scatter).GetNearest(position, mainplot.Plot.LastRender);
					if (nearest.IsReal)
					{
						_viewModel.SelectedPoint = $"X={nearest.X:0.##}, Y={nearest.Y:0.##}";
						
						mainplot.Refresh();
					}
						
				}

			};
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
			contentDialog.PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style;
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
			contentDialog.PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style;
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

							(mainplot.Plot.Axes.Bottom as BottomAxis).LabelText = content.EnteredText;
							(mainplot.Plot.Axes.Bottom as BottomAxis).LabelFontName = ClassParameters.FontName;
							break;
						}
					case "Y":
						{
							(mainplot.Plot.Axes.Left as LeftAxis).LabelText = content.EnteredText;
							(mainplot.Plot.Axes.Left as LeftAxis).LabelFontName = ClassParameters.FontName;
							break;
						}
					case "Series":
						{
							var a = mainplot.Plot.PlottableList[_viewModel.PlotIndex] as Scatter;
							a.LegendText = content.EnteredText;
							mainplot.Plot.PlottableList[_viewModel.PlotIndex] = a;
							break;
						}
				}
				mainplot.Refresh();
			}
		}

		private async void EnterName_Click(object sender, RoutedEventArgs e)
		{
			ContentDialog contentDialog = new ContentDialog();
			contentDialog.XamlRoot = (sender as MenuFlyoutItem).XamlRoot;
			contentDialog.Name = "Series";
			contentDialog.Title = "Введите название графика";
			contentDialog.Content = new EnterName();
			contentDialog.PrimaryButtonText = "OK";
			contentDialog.PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style;
			contentDialog.PrimaryButtonClick += EnterAxisName;
			contentDialog.SecondaryButtonText = "Отмена";
			await contentDialog.ShowAsync();
		}
	}


}
