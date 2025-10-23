using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
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
		public static readonly DashEffect dashEffect = new([3, 3]);
		public static readonly DrawMarginFrame drawMarginFrame = new()
		{
			
			Stroke = new SolidColorPaint
			{
				Color = s_gray,
				StrokeThickness = 1
			}
		};
		public static readonly SolidColorPaint labelsPaint = new SolidColorPaint
		{
			Color = SKColors.Blue,
			FontFamily = "Times New Roman",
			SKFontStyle = new SKFontStyle(
					SKFontStyleWeight.ExtraBold,
					SKFontStyleWidth.Normal,
					SKFontStyleSlant.Italic)

		};
}
}
