using CsvHelper.Configuration.Attributes;
using PlotApp.Util;
using ScottPlot;

namespace PlotApp.Model
{
	public class DataItem : BindHelper
	{
		private float _x, _y;
		[Index(0)]
		public float X
		{
			get { return _x; }
			set { _x = value; OnPropertyChanged(nameof(X)); }
		}
		[Index(1)]
		public float Y
		{
			get { return _y; }
			set { _y = value; OnPropertyChanged(nameof(Y));}
		}
		public DataItem()
		{
			X = 0;
			Y = 0;
		}
		public static implicit operator Coordinates(DataItem dataItem) => new Coordinates(dataItem.X, dataItem.Y);

	}
}
