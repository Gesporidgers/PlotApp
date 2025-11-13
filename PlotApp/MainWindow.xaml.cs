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
	public sealed partial class MainWindow : Window
	{
		private ViewModel _viewModel;
		/// <summary>
		/// Производим настройки окна с графиком и добавляем перекрестье в 0,0
		/// Также добавляем событие для отображения ближайших к курсору координат
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			_viewModel = new ViewModel(ref mainplot);
			mainplot.Plot.Legend.FontName = ClassParameters.FontName;
			mainplot.Plot.Axes.Left.TickLabelStyle.FontName = ClassParameters.FontName;
			mainplot.Plot.Axes.Bottom.TickLabelStyle.FontName = ClassParameters.FontName;
			mainplot.Plot.Axes.Left.TickLabelStyle.FontSize = ClassParameters.TicksFontSize;
			mainplot.Plot.Axes.Bottom.TickLabelStyle.FontSize = ClassParameters.TicksFontSize;
			mainplot.Plot.Legend.Alignment = Alignment.LowerCenter;
			mainplot.Plot.Legend.FontSize = ClassParameters.FontSize;
			mainplot.Plot.Axes.ContinuouslyAutoscale = true;
			Crosshair center = mainplot.Plot.Add.Crosshair(0, 0);
			center.LineColor = ScottPlot.Color.FromSKColor(ClassParameters.s_gray2);
			List<IPlottable> scatters;
			mainplot.PointerMoved += (s, e) =>
			{
				scatters = mainplot.Plot.PlottableList.Where((i) => i.GetType() == typeof(Scatter)).ToList();
				if (scatters.Count() > 0)
				{
					var point = e.GetCurrentPoint(mainplot).Position;
					Coordinates position = mainplot.Plot.GetCoordinates((float)point.X, (float)point.Y);
					DataPoint nearest = (scatters[_viewModel.PlotIndex] as Scatter).GetNearest(position, mainplot.Plot.LastRender);
					if (nearest.IsReal)
					{
						_viewModel.SelectedPoint = $"X={nearest.X:0.##}, Y={nearest.Y:0.##}";

						mainplot.Refresh();
					}

				}

			};
			AppWindow.TitleBar.PreferredTheme = Microsoft.UI.Windowing.TitleBarTheme.UseDefaultAppMode;
			mainplot.Menu?.Clear();
			mainplot.Menu?.Add("Вписать", (s) => { s.Axes.AutoScale(); });
			mainplot.Menu?.Add("Скопировать", (s) =>
			{
				SvgClipboardHelper.SetSvg(s.GetSvgHtml(900, _viewModel.ToggleLegend ? 610 : 540));
			});
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
			if (file != null)
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
			contentDialog.DefaultButton = ContentDialogButton.Primary;

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
			contentDialog.DefaultButton = ContentDialogButton.Primary;

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
							(mainplot.Plot.Axes.Bottom as BottomAxis).LabelFontSize = ClassParameters.FontSize;
							(mainplot.Plot.Axes.Bottom as BottomAxis).LabelBold = false;
							break;
						}
					case "Y":
						{
							(mainplot.Plot.Axes.Left as LeftAxis).LabelText = content.EnteredText;
							(mainplot.Plot.Axes.Left as LeftAxis).LabelFontName = ClassParameters.FontName;
							(mainplot.Plot.Axes.Left as LeftAxis).LabelFontSize = ClassParameters.FontSize;
							(mainplot.Plot.Axes.Left as LeftAxis).LabelBold = false;
							break;
						}
					case "Series":
						{
							var a = mainplot.Plot.PlottableList[_viewModel.IndexInPlotList] as Scatter;
							a.LegendText = content.EnteredText;
							mainplot.Plot.PlottableList[_viewModel.IndexInPlotList] = a;
							if (!_viewModel.ToggleLegend)
							{
								mainplot.Plot.ShowLegend(Edge.Bottom);
								_viewModel.ToggleLegend = true;
							}
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
			contentDialog.DefaultButton = ContentDialogButton.Primary;

			await contentDialog.ShowAsync();
		}
	}


}
