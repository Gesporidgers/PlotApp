using PlotApp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace PlotApp.Util
{
	internal class CSVImporter
	{
		public static List<DataItem> Import(string path)
		{
			var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = false
			};
			using (StreamReader reader = new StreamReader(path))
				using (CsvReader csv = new CsvReader(reader, config))
				{
					return csv.GetRecords<DataItem>().ToList();
				}
		}
	}
}
