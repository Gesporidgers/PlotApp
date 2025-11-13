
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotApp.Util
{
	internal static class ClassParameters
	{
		public static readonly SKColor s_gray = new(195, 195, 195);
		public static readonly SKColor s_gray1 = new(160, 160, 160);
		public static readonly SKColor s_gray2 = new(90, 90, 90);
		public static readonly SKColor s_dark3 = new(60, 60, 60);
		public static readonly string FontName = "Times New Roman";
		/// <summary>
		/// Размер шрифта легенды и названия осей
		/// </summary>
		public static readonly int FontSize = 20;
		/// <summary>
		/// Размер шрифта отметок на осях
		/// </summary>
		public static readonly int TicksFontSize = 14;
		
}
}
