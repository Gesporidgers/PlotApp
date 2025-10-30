using MathNet.Numerics.Interpolation;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotApp.Model
{
	internal class Grafik
	{
		public ObservableCollection<Coordinates> Coordinates { get; set; }
		public ObservableCollection<DataItem> Model { get; set; }
		public Color PlotColor { get; set; }
		public LinePattern Pattern { get; set; }
		public CubicSpline spline { get; set; }
	}
}
