using CsvHelper.Configuration.Attributes;
using LiveChartsCore.Defaults;
using PlotApp.Util;

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
		public static implicit operator ObservablePoint(DataItem dataItem) => new ObservablePoint(dataItem.X, dataItem.Y);

	}
}
