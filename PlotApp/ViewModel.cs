
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using NLog.Extensions.Logging;
using PlotApp.Model;
using PlotApp.Util;
using ScottPlot;
using ScottPlot.DataSources;
using ScottPlot.Plottables;
using ScottPlot.WinUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PlotApp
{
	internal sealed class ViewModel : BindHelper
	{
		private enum UPDATE_MODE
		{
			Color,
			Pattern
		}
		/// <summary>
		/// Индекс текущего графика в списке графиков на самом графике ScottPlot
		/// </summary>
		private int indexInPlotList;
		private ObservableCollection<DataItem> _model;
		private Visibility _plotVisibility = Visibility.Collapsed;
		private WinUIPlot plot;
		private bool _isSmooth = false;
		private bool _toggleLegend = false;
		private bool _isEnabledOptions = false;
		private bool _canInterpolate = false;
		private string _selected;
		private string _selectedPoint;
		private int _selInd;
		private List<Grafik> plots = new List<Grafik>();

		private ILogger logger;

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
				plots[PlotIndex].isSmooth = value;
				OnPropertyChanged(nameof(isSmooth));
			}
		}
		public bool ToggleLegend
		{
			get => _toggleLegend;
			set
			{
				_toggleLegend = value;
				OnPropertyChanged(nameof(ToggleLegend));

			}
		}
		public bool IsEnabledOptions
		{
			get => _isEnabledOptions;
			set
			{
				_isEnabledOptions = value;
				OnPropertyChanged(nameof(IsEnabledOptions));
			}
		}
		public bool CanInterpolate
		{
			get => _canInterpolate;
			set
			{
				_canInterpolate = value;
				OnPropertyChanged(nameof(CanInterpolate));
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

		/// <summary>
		/// Индекс текущего графика в списке plots
		/// </summary>
		public int PlotIndex = 0;
		public int IndexInPlotList
		{
			get
			{
				FindIndexOfPlot();
				return indexInPlotList;
			}
		}
		/// <summary>
		/// Для текста во View, чтобы понимать какой график редактируется
		/// </summary>
		public string Selected
		{
			get => _selected;
			set
			{
				_selected = value;
				OnPropertyChanged(nameof(Selected));
			}
		}
		/// <summary>
		/// Для текста во View, чтобы видеть какая точка под курсором
		/// </summary>
		public string SelectedPoint
		{
			get => _selectedPoint;
			set
			{
				_selectedPoint = value;
				OnPropertyChanged(nameof(SelectedPoint));
			}
		}

		public void AddRow()
		{
			if (plots[PlotIndex].Model == null)
			{
				Model = new ObservableCollection<DataItem>();
				plots[PlotIndex].Model = new ObservableCollection<DataItem>();
				plots[PlotIndex].Coordinates = new ObservableCollection<Coordinates>();
				plots[PlotIndex].Coordinates.CollectionChanged += (s, e) =>
				{
					UpdatePlot();
				};
				PlotVisibility = Visibility.Visible;



			}
			Model.Add(new DataItem());          // добавим точку в view, а потом в модели
			plots[PlotIndex].Coordinates.Add(new DataItem());
			plots[PlotIndex].Model.Add(new DataItem());
			if (plots[PlotIndex].Model.Count > 3)
			{
				CanInterpolate = true;
				InitSpline();
			}
			IsEnabledOptions = true;
			Model[Model.Count - 1].PropertyChanged += (s, e) =>               // при изменении точки во view менять и в model
			{
				if (isSmooth)
					UnsetSmooth();
				plots[PlotIndex].Model[SelInd] = Model[SelInd];
				plots[PlotIndex].Coordinates[SelInd] = plots[PlotIndex].Model[SelInd];
				if (Model.Count > 3)
				{
					CanInterpolate = true;
					InitSpline();
				}

			};
			Selected = $"{PlotIndex + 1}/{plots.Count}";
		}

		public void DeleteRow(int index)
		{
			plots[PlotIndex].Model.RemoveAt(index);
			Model.RemoveAt(index);
			plots[PlotIndex].Coordinates.RemoveAt(index);
			UnsetSmooth();
			if (Model.Count > 3)
			{
				CanInterpolate = true;
				InitSpline();
			}
			else
			{
				plots[PlotIndex].spline = null;
				CanInterpolate = false;
			}
			if (Model.Count == 0)
				IsEnabledOptions = false;
		}

		// Надо бы прикрутить флаг для того чтобы не использовать эти функции когда количество точек меньше 4 (?)
		public async void SetSmooth()
		{
			// Найти индекс scatter по содержимому коллекции, а не по ссылке
			if (isSmooth) return;
			FindIndexOfPlot();
			plots[PlotIndex].Coordinates.Clear();
			List<Coordinates> coords = new List<Coordinates>();
			if (plots[PlotIndex].Model[0].X < plots[PlotIndex].Model[Model.Count - 1].X)
				await Task.Run(() =>
				{
					for (double i = plots[PlotIndex].Model[0].X; i <= plots[PlotIndex].Model[Model.Count - 1].X; i += 0.125)
					{
						double y = plots[PlotIndex].spline.Interpolate(i);
						coords.Add(new Coordinates(i, y));
					}
				});
			else
				await Task.Run(() =>
				{
					for (double i = plots[PlotIndex].Model[Model.Count - 1].X; i <= plots[PlotIndex].Model[0].X; i += 0.125)
					{
						double y = plots[PlotIndex].spline.Interpolate(i);
						coords.Add(new Coordinates(i, y));
					}
				});
			plots[PlotIndex].Coordinates = new ObservableCollection<Coordinates>(coords);
			plots[PlotIndex].Coordinates.CollectionChanged += (s, e) => { UpdatePlot(); };
			isSmooth = true;
			UpdatePlot();
		}

		public void UnsetSmooth()
		{
			if (!isSmooth) return;
			plots[PlotIndex].Coordinates.Clear();
			foreach (var item in plots[PlotIndex].Model)
			{
				plots[PlotIndex].Coordinates.Add(item);
			}
			isSmooth = false;
		}

		public void ChangeColor(object sender, ColorChangedEventArgs e)
		{
			FindIndexOfPlot();
			plots[PlotIndex].PlotColor = ScottPlot.Color.FromSKColor(SkiaSharp.SKColor.Parse(e.NewColor.ToString()));
			UpdatePlot(UPDATE_MODE.Color);

		}
		public void DashLine()
		{
			FindIndexOfPlot();
			plots[PlotIndex].Pattern = LinePattern.Dashed;
			UpdatePlot(UPDATE_MODE.Pattern);
		}
		public void DotLine()
		{
			FindIndexOfPlot();
			plots[PlotIndex].Pattern = LinePattern.Dotted;
			UpdatePlot(UPDATE_MODE.Pattern);
		}
		public void SolidLine()
		{
			FindIndexOfPlot();
			plots[PlotIndex].Pattern = LinePattern.Solid;
			UpdatePlot(UPDATE_MODE.Pattern);
		}

		// Будем делать импорт из CSV. Также можно и json (но вряд-ли нужно
		public async void Import(string path)
		{
			try
			{
				List<DataItem> data = CSVImporter.Import(path);
				if (data != null || data.Count == 0)
				{
					if (plots[PlotIndex].Model == null)
					{
						plots[PlotIndex].Model = new ObservableCollection<DataItem>(data);
						Model = new ObservableCollection<DataItem>(data);
						plots[PlotIndex].Coordinates = new ObservableCollection<Coordinates>(plots[PlotIndex].Model.Select(p => new Coordinates(p.X, p.Y)));
						plots[PlotIndex].Coordinates.CollectionChanged += (s, e) =>
						{
							UpdatePlot();
						};

						UpdatePlot();
					}
					else
					{
						FindIndexOfPlot();
						Model = new ObservableCollection<DataItem>(data);
						plots[PlotIndex].Model = new ObservableCollection<DataItem>(data);
						plots[PlotIndex].Coordinates.Clear();
						foreach (var item in plots[PlotIndex].Model)
						{
							plots[PlotIndex].Coordinates.Add(new Coordinates(item.X, item.Y));
						}
						UpdatePlot();
					}
					if (data.Count > 3)
					{
						InitSpline();
						CanInterpolate = true;
					}
					IsEnabledOptions = true;
					Selected = $"{PlotIndex + 1}/{plots.Count}";
				}
				else
				{
					logger.LogWarning("Попытка импорта пустого файла");
					ContentDialog contentDialog = new ContentDialog()
					{
						Title = "Ошибка импорта",
						Content = "Файл пуст",
						CloseButtonText = "ОК",
						XamlRoot = plot.XamlRoot
					};
					await contentDialog.ShowAsync();

				}
			}
			catch (Exception ex)
			{
				if (ex.Source != "CsvHelper")
				{
					logger.LogError(ex, "Ошибка обработки уже импортированных данных" + $"PlotIndex:{PlotIndex}");
					ContentDialog contentDialog = new ContentDialog()
					{
						Title = "Ошибка",
						Content = ex.Message,
						CloseButtonText = "ОК",
						XamlRoot = plot.XamlRoot,

					};
					await contentDialog.ShowAsync();
				}
				else
				{
					logger.LogWarning("Попытка импорта неправильного формата данных");
					ContentDialog contentDialog = new ContentDialog()
					{
						Title = "Ошибка импорта",
						Content = "Не удалось импортировать данные из файла. Проверьте правильность формата данных в файле.",
						CloseButtonText = "ОК",
						XamlRoot = plot.XamlRoot,

					};
					await contentDialog.ShowAsync();
				}
			}

		}


		public void NextPlot()
		{
			PlotIndex++;
			Selected = $"{PlotIndex + 1}/{plots.Count}";
			if (plots.Count - 1 < PlotIndex)        // если новый индекс больше количества графиков, то создаём новый элемент в списке
			{
				plots.Add(new Grafik());
				IsEnabledOptions = false;
				isSmooth = false;
			}
			if (plots[PlotIndex].Model != null)     // если график по этому индексу создан, то выводим во view все его данные
			{
				Model = plots[PlotIndex].Model;
				IsEnabledOptions = true;
				isSmooth = plots[PlotIndex].isSmooth;
				FindIndexOfPlot();
			}
			else                                    // если нет, то иничиализируем таблицу
			{
				Model = new ObservableCollection<DataItem>();
			}

		}
		public async void PrevPlot()
		{
			if (PlotIndex > 0)
			{
				PlotIndex--;
				Selected = $"{PlotIndex + 1}/{plots.Count}";
				Model = plots[PlotIndex].Model;
				isSmooth = plots[PlotIndex].isSmooth;
				try
				{
					IsEnabledOptions = plots[PlotIndex].Model == null || plots[PlotIndex].Model.Count > 0;
					FindIndexOfPlot();
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Error in PrevPlot method " + $"PlotIndex:{PlotIndex} " + $"Index in plotlist:{indexInPlotList}");
					ContentDialog contentDialog = new ContentDialog()
					{
						Title = "Error",
						Content = ex.Message,
						CloseButtonText = "ОК",
						XamlRoot = plot.XamlRoot,

					};
					await contentDialog.ShowAsync();
				}

			}
		}
		// Сделать проверку на удаление единственного графика
		// Сделать если удаление происходит первого графика из двух потому что индекс также будет -1
		public async void DeletePlot()
		{
			if (PlotIndex == 0 && plots.Count == 1)
			{
				try
				{
					plots.RemoveAt(PlotIndex);
					Model = null;
					plots.Add(new());
				}
				catch (IndexOutOfRangeException ex)
				{
					ContentDialog contentDialog = new ContentDialog()
					{
						Title = "Выход за пределы массива",
						Content = ex.Message,
						CloseButtonText = "ОК",
						XamlRoot = plot.XamlRoot,

					};
					await contentDialog.ShowAsync();
					logger.LogError(ex, "Выход за пределы массива " + $"PlotIndex:{PlotIndex}");
				}

			}
			else if (plots.Count > 1)
			{
				try
				{
					plots[PlotIndex] = plots[PlotIndex + 1];
					plots.RemoveAt(PlotIndex + 1);
					Selected = $"{PlotIndex + 1}/{plots.Count}";
					Model = plots[PlotIndex].Model;
					isSmooth = plots[PlotIndex].isSmooth;
					IsEnabledOptions = plots[PlotIndex].Model.Count > 0;
				}
				catch (IndexOutOfRangeException ex)
				{
					logger.LogError(ex, "Выход за пределы массива" + $"PlotIndex:{PlotIndex}");
					ContentDialog contentDialog = new ContentDialog()
					{
						Title = "Выход за пределы массива",
						Content = ex.Message,
						CloseButtonText = "ОК",
						XamlRoot = plot.XamlRoot,

					};
					await contentDialog.ShowAsync();
				}

			}
			try
			{
				plot.Plot.PlottableList.RemoveAt(indexInPlotList);
				plot.Refresh();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Невозможно удалить из списка отрисованных " + $"Index in plotlist:{indexInPlotList}");
			}


		}

		// Также не забыть про копию графика в буфер обмена
		public ViewModel(ref WinUIPlot plot)
		{
			using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddNLog());
			var config = new NLog.Config.LoggingConfiguration();
			var logfile = new NLog.Targets.FileTarget("logfile") { FileName = $"{DateTime.Now.ToString("s")}.log" };
			config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);
			NLog.LogManager.Configuration = config;
			logger = factory.CreateLogger(typeof(ViewModel));
			//logger.LogInformation("Program started");
			this.plot = plot;
			plots.Add(new Grafik());
			Selected = $"{PlotIndex + 1}/{plots.Count}";
			indexInPlotList = 1;
		}

		/// <summary>
		/// Полная перерисовка графика при изменении точек
		/// </summary>
		private void UpdatePlot()
		{
			var scatters = plot.Plot.PlottableList.Where((i) => i.GetType() == typeof(Scatter)).ToList();
			if (scatters.Count == 0 || scatters.Count - 1 < PlotIndex)
			{
				var scat = plot.Plot.Add.ScatterLine(plots[PlotIndex].Coordinates.ToArray());
				scat.LineWidth = 2;
				plots[PlotIndex].PlotColor = scat.Color;
				plots[PlotIndex].Pattern = LinePattern.Solid;
			}
			else
			{

				var scat = new Scatter(new ScatterSourceCoordinatesArray(plots[PlotIndex].Coordinates.ToArray()));
				scat.LineWidth = 2;
				scat.MarkerSize = 0;
				scat.Color = plots[PlotIndex].PlotColor;
				scat.LinePattern = plots[PlotIndex].Pattern;
				plot.Plot.PlottableList[indexInPlotList] = scat;
			}
			plot.Refresh();
		}

		/// <summary>
		/// Перерисовка графика при изменении определённого параметра в графике
		/// </summary>
		/// <param name="mode">Режим изменения. Цвет или стиль</param>
		private void UpdatePlot(UPDATE_MODE mode)
		{
			switch (mode)
			{
				case UPDATE_MODE.Color:
					{
						(plot.Plot.PlottableList[indexInPlotList] as Scatter).Color = plots[PlotIndex].PlotColor;
						break;
					}
				case UPDATE_MODE.Pattern:
					{
						(plot.Plot.PlottableList[indexInPlotList] as Scatter).LinePattern = plots[PlotIndex].Pattern;
						break;
					}
			}
			plot.Refresh();
		}

		void InitSpline()
		{
			var xs = plots[PlotIndex].Model.Select(p => (double)p.X);
			var ys = plots[PlotIndex].Model.Select(p => (double)p.Y);
			plots[PlotIndex].spline = (CubicSpline)Interpolate.CubicSpline(xs, ys);
		}
		/// <summary>
		/// Поиск индекса текущего графика в списке графиков ScottPlot по содержимому
		/// </summary>
		void FindIndexOfPlot()
		{
			if (plots[PlotIndex].Model == null) return;
			indexInPlotList = plot.Plot.PlottableList.FindIndex(i =>
			{
				var scatter = i as Scatter;
				var dataPoints = (scatter?.Data as ScatterSourceCoordinatesArray)?.GetScatterPoints();
				if (dataPoints == null) return false;

				return dataPoints.SequenceEqual(plots[PlotIndex].Coordinates);
			});
		}
	}
}
