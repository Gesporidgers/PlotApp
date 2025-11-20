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
			bool nonStandartFormat = File.ReadAllText(path).Contains(';');
			CsvConfiguration config;
			config = nonStandartFormat ? new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture)
			{
				HasHeaderRecord = false,
				Delimiter = ";",
			} : new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = false,
				Delimiter = ",",
			};
			using (StreamReader reader = new StreamReader(path))
				using (CsvReader csv = new CsvReader(reader, config))
				{
					return csv.GetRecords<DataItem>().ToList();
				}
		}
	}
}
