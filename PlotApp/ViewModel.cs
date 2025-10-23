using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using PlotApp.Model;
using PlotApp.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlotApp
{
	internal sealed class ViewModel : BindHelper
	{
		private ObservableCollection<DataItem> _model;
		private ObservableCollection<ObservablePoint> _points;
		private Visibility _plotVisibility = Visibility.Collapsed;
		private ISeries[] _series;
		private IEnumerable<ICartesianAxis> _xAxis, _yAxis;
		private int _selInd;
		public ISeries[] Series { get => _series; set { _series = value; OnPropertyChanged(nameof(Series)); } }
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

		public IEnumerable<ICartesianAxis> XAxis
		{
			get => _xAxis;
			set
			{
				_xAxis = value;
				OnPropertyChanged(nameof(XAxis));
			}
		}
		public IEnumerable<ICartesianAxis> YAxis
		{
			get => _yAxis;
			set
			{
				_yAxis = value;
				OnPropertyChanged(nameof(YAxis));
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

		public void AddRow()
		{
			if (Model == null)
			{
				Model = new ObservableCollection<DataItem>();
				_points = new ObservableCollection<ObservablePoint>();
				Series = [
					new LineSeries<ObservablePoint>{
					Values = _points,Fill = null
					}
				];

				PlotVisibility = Visibility.Visible;
			}
			Model.Add(new DataItem());
			Model[Model.Count - 1].PropertyChanged += (s, e) =>
			{
				_points[SelInd] = Model[SelInd];
			};
			_points.Add(Model.Last());

		}

		public void DeleteRow(int index)
		{
			Model.RemoveAt(index);
			_points.RemoveAt(index);
		}

		public void SetSmooth()
		{
			if (Series != null)
				(Series[0] as LineSeries<ObservablePoint>).LineSmoothness = .65;
		}

		public void UnsetSmooth() => (Series[0] as LineSeries<ObservablePoint>).LineSmoothness = 0;
		public void ChangeColor(object sender, ColorChangedEventArgs e)
		{
			(Series[0] as LineSeries<ObservablePoint>).Stroke = new SolidColorPaint(SkiaSharp.SKColor.Parse(e.NewColor.ToString()), 4);
			(Series[0] as LineSeries<ObservablePoint>).GeometryStroke = new SolidColorPaint(SkiaSharp.SKColor.Parse(e.NewColor.ToString()), 4);
		}

		// Будем делать импорт из CSV. Также можно и json (но вряд-ли нужно
		public void Import(string path)
		{
			List<DataItem> data = CSVImporter.Import(path);
			if (data != null)
			{
				Model = new ObservableCollection<DataItem>(data);
				_points = new ObservableCollection<ObservablePoint>(Model.Select((i) => new ObservablePoint(i.X, i.Y)));
				PlotVisibility = Visibility.Visible;
				Series = [
					new LineSeries<ObservablePoint>{
					Values = _points,Fill = null
					}
				];
			}

		}

		// Также не забыть про копию графика в буфер обмена
		public ViewModel()
		{
			XAxis = new Axis[]
			{
				new Axis
				{
					SeparatorsPaint = new SolidColorPaint(SkiaSharp.SKColors.LightBlue),
				}
			};
			YAxis = new Axis[]
			{
				new Axis
				{
					SeparatorsPaint = new SolidColorPaint(SkiaSharp.SKColors.LightBlue),
				}
			};
		}
	}
}
