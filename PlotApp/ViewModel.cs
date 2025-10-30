
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using PlotApp.Model;
using PlotApp.Util;
using ScottPlot;
using ScottPlot.DataSources;
using ScottPlot.Plottables;
using ScottPlot.WinUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlotApp
{
	internal sealed class ViewModel : BindHelper
	{
		private enum UPDATE_MODE
		{
			Color,
			Pattern
		}

		private ObservableCollection<DataItem> _model;
		private ObservableCollection<Coordinates> _points;
		private Visibility _plotVisibility = Visibility.Collapsed;
		private CubicSpline spline;
		private WinUIPlot plot;
		private bool _isSmooth = false;
		private string _selected;
		private int _selInd;
		private List<Grafik> plots = new List<Grafik>();
		public ObservableCollection<DataItem> Model
		{
			get => _model;
			set
			{
				_model = value;
				OnPropertyChanged(nameof(Model));
			}
		}


		public Visibility PlotVisibility
		{
			get => _plotVisibility;
			set
			{
				_plotVisibility = value;
				OnPropertyChanged(nameof(PlotVisibility));
				OnPropertyChanged(nameof(WarningVisibility));
			}
		}

		public Visibility WarningVisibility
		{
			get => _plotVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
		}
		public bool isSmooth
		{
			get => _isSmooth;
			set
			{
				_isSmooth = value;
				OnPropertyChanged(nameof(isSmooth));
			}
		}
		public int SelInd
		{
			get => _selInd;
			set
			{
				_selInd = value;
				OnPropertyChanged(nameof(SelInd));
			}
		}
		public int PlotIndex = 0;
		public string Selected
		{
			get => _selected;
			set
			{
				_selected = value;
				OnPropertyChanged(nameof(Selected));
			}
		}

		public void AddRow()
		{
			if (plots[PlotIndex].Model == null)
			{
				Model = new ObservableCollection<DataItem>();
				plots[PlotIndex].Model = new ObservableCollection<DataItem>();
				plots[PlotIndex].Coordinates = new ObservableCollection<Coordinates>();
				plots[PlotIndex].Coordinates.CollectionChanged += (s, e) =>
				{
					UpdatePlot();
				};
				PlotVisibility = Visibility.Visible;
				if (plots[PlotIndex].Model.Count > 2)
					InitSpline();

			}
			Model.Add(new DataItem());
			plots[PlotIndex].Coordinates.Add(new DataItem());
			plots[PlotIndex].Model.Add(new DataItem());
			Model[Model.Count - 1].PropertyChanged += (s, e) =>
			{
				if (isSmooth)
					UnsetSmooth();
				plots[PlotIndex].Model[SelInd] = Model[SelInd];
				plots[PlotIndex].Coordinates[SelInd] = plots[PlotIndex].Model[SelInd];
				if (Model.Count > 2)
					InitSpline();
			};
		}

		public void DeleteRow(int index)
		{
			plots[PlotIndex].Model.RemoveAt(index);
			Model.RemoveAt(index);
			plots[PlotIndex].Coordinates.RemoveAt(index);
		}


		public void SetSmooth()
		{
			plots[PlotIndex].Coordinates.Clear();
			for (double i = plots[PlotIndex].Model[0].X; i <= plots[PlotIndex].Model[Model.Count - 1].X; i += 0.125)
			{
				double y = plots[PlotIndex].spline.Interpolate(i);
				plots[PlotIndex].Coordinates.Add(new Coordinates(i, y));
			}
			isSmooth = true;
			UpdatePlot();
		}

		public void UnsetSmooth()
		{
			plots[PlotIndex].Coordinates.Clear();
			foreach (var item in plots[PlotIndex].Model)
			{
				plots[PlotIndex].Coordinates.Add(item);
			}
			isSmooth = false;
		}

		public void ChangeColor(object sender, ColorChangedEventArgs e)
		{
			plots[PlotIndex].PlotColor = ScottPlot.Color.FromSKColor(SkiaSharp.SKColor.Parse(e.NewColor.ToString()));
			UpdatePlot(UPDATE_MODE.Color);
			
		}
		public void DashLine()
		{
			plots[PlotIndex].Pattern = LinePattern.Dashed;
			UpdatePlot(UPDATE_MODE.Pattern);
		}
		public void UndashLine()
		{
			plots[PlotIndex].Pattern = LinePattern.Solid ;
			UpdatePlot(UPDATE_MODE.Pattern);
		}

		// Будем делать импорт из CSV. Также можно и json (но вряд-ли нужно
		public void Import(string path)
		{
			List<DataItem> data = CSVImporter.Import(path);
			if (data != null)
			{
				if (plots[PlotIndex].Model == null)
				{
					plots[PlotIndex].Model = new ObservableCollection<DataItem>(data);
					Model = new ObservableCollection<DataItem>(data);
					plots[PlotIndex].Coordinates = new ObservableCollection<Coordinates>(plots[PlotIndex].Model.Select(p => new Coordinates(p.X, p.Y)));
					plots[PlotIndex].Coordinates.CollectionChanged += (s, e) =>
					{
						UpdatePlot();
					};
					InitSpline();
					UpdatePlot();
				}
				else
				{
					plots[PlotIndex].Model = new ObservableCollection<DataItem>(data);
					plots[PlotIndex].Coordinates.Clear();
					foreach (var item in plots[PlotIndex].Model)
					{
						plots[PlotIndex].Coordinates.Add(new Coordinates(item.X, item.Y));
					}
					UpdatePlot();
				}
			}

		}


		// Также не забыть про копию графика в буфер обмена
		public ViewModel(ref WinUIPlot plot)
		{
			this.plot = plot;
			plots.Add(new Grafik());
		}

		/// <summary>
		/// Полная перерисовка графика при изменении точек
		/// </summary>
		private void UpdatePlot()
		{
			if (plot.Plot.PlottableList.Count == 0 || plot.Plot.PlottableList.Count - 1 < PlotIndex)
			{
				var scat = plot.Plot.Add.Scatter(plots[PlotIndex].Coordinates.ToArray());
				plots[PlotIndex].PlotColor = scat.Color;
				plots[PlotIndex].Pattern = LinePattern.Solid;
			}
			else
			{
				var scat = new Scatter(new ScatterSourceCoordinatesArray(plots[PlotIndex].Coordinates.ToArray()));
				scat.Color = plots[PlotIndex].PlotColor;
				scat.LinePattern = plots[PlotIndex].Pattern;
				plot.Plot.PlottableList[PlotIndex] = scat;
			}
			plot.Refresh();
		}

		private void UpdatePlot(UPDATE_MODE mode)
		{
			switch (mode)
			{
				case UPDATE_MODE.Color:
					{
						(plot.Plot.PlottableList[PlotIndex] as Scatter).Color = plots[PlotIndex].PlotColor;
						break;
					}
				case UPDATE_MODE.Pattern:
					{
						(plot.Plot.PlottableList[PlotIndex] as Scatter).LinePattern = plots[PlotIndex].Pattern;
						break;
					}
			}
			plot.Refresh();
		}

		private void InitSpline()
		{
			var xs = plots[PlotIndex].Model.Select(p => (double)p.X);
			var ys = plots[PlotIndex].Model.Select(p => (double)p.Y);
			plots[PlotIndex].spline = (CubicSpline)Interpolate.CubicSpline(xs, ys);
		}
	}
}
