using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotApp.Model
{
	internal class Plot
	{
		public Coordinates[] Coordinates { get; set; }
		public Color PlotColor { get; set; }
		public LinePattern Pattern { get; set; }
	}
}
