
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using PlotApp.Model;
using PlotApp.Util;
using ScottPlot;
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
		private ObservableCollection<DataItem> _model;
		private ObservableCollection<Coordinates> _points;
		private Visibility _plotVisibility = Visibility.Collapsed;
		private CubicSpline spline;
		private WinUIPlot plot;
		private bool _isSmooth = false;
		private string _selected;
		private int _selInd;

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
			if (Model == null)
			{
				Model = new ObservableCollection<DataItem>();
				_points = new ObservableCollection<Coordinates>();
				_points.CollectionChanged += (s, e) =>
				{
					UpdatePlot();
				};
				PlotVisibility = Visibility.Visible;
				if (Model.Count > 2)
					InitSpline();
				
			}
			Model.Add(new DataItem());
			_points.Add(new DataItem());
			Model[Model.Count - 1].PropertyChanged += (s, e) =>
			{
				if (isSmooth)
					UnsetSmooth();
				_points[SelInd] = Model[SelInd];
				InitSpline();
			};
		}

		public void DeleteRow(int index)
		{
			Model.RemoveAt(index);
			_points.RemoveAt(index);
		}

		
		public void SetSmooth()
		{
			_points.Clear();
			for (double i = Model[0].X; i <= Model[Model.Count - 1].X; i += 0.125)
			{
				double y = spline.Interpolate(i);
				_points.Add(new Coordinates(i, y));
			}
			isSmooth = true;
			UpdatePlot();
		}

		public void UnsetSmooth()
		{
			_points.Clear();
			foreach (var item in Model)
			{
				_points.Add(item);
			}
			isSmooth = false;
		}

		public void ChangeColor(object sender, ColorChangedEventArgs e)
		{
			(plot.Plot.PlottableList[0] as Scatter).Color = ScottPlot.Color.FromSKColor(SkiaSharp.SKColor.Parse(e.NewColor.ToString()));
			plot.Refresh();
		}
		public void DashLine()
		{
			(plot.Plot.PlottableList[0] as Scatter).LinePattern = LinePattern.Dashed;
			plot.Refresh();
		}
		public void UndashLine()
		{
			//Paint strk = (Series[0] as LineSeries<ObservablePoint>).Stroke;
			//SolidColorPaint newStyle = strk as SolidColorPaint;
			//newStyle.PathEffect = new DashEffect(new float[2]);
			//(Series[0] as LineSeries<ObservablePoint>).Stroke = newStyle;
		}

		// Будем делать импорт из CSV. Также можно и json (но вряд-ли нужно
		public void Import(string path)
		{
			List<DataItem> data = CSVImporter.Import(path);
			if (data != null)
			{
				if (Model == null)
				{
					Model = new ObservableCollection<DataItem>(data);
					_points = new ObservableCollection<Coordinates>(Model.Select(p => new Coordinates(p.X, p.Y)));
					_points.CollectionChanged += (s, e) =>
					{
						UpdatePlot();
					};
					InitSpline();
					UpdatePlot();
				}
				else
				{
					Model = new ObservableCollection<DataItem>(data);
					_points.Clear();
					foreach (var item in Model)
					{
						_points.Add(new Coordinates(item.X, item.Y));
					}
					UpdatePlot();
				}
			}

		}


		// Также не забыть про копию графика в буфер обмена
		public ViewModel(ref WinUIPlot plot)
		{
			this.plot = plot;
		}

		private void UpdatePlot()
		{
			plot.Plot.Clear();
			plot.Plot.Add.Scatter(_points.ToArray()).MarkerSize = 0;
			plot.Refresh();
		}

		private void InitSpline()
		{
			var xs = Model.Select(p => (double)p.X);
			var ys = Model.Select(p => (double)p.Y);
			spline = (CubicSpline)Interpolate.CubicSpline(xs, ys);
		}
	}
}
