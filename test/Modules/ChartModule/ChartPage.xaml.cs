using System;
using System.Windows.Navigation;
using Visifire.Charts;

namespace ChartModule {

	public partial class ChartPage {

		public ChartPage() {
			InitializeComponent();
		}

		// Executes when the user navigates to this page.
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			// Create a new Visifire Chart
			CreateChart();
		}

		/// <summary>
		/// Function to create a chart
		/// </summary>
		public void CreateChart() {
			// Create a new instance of Chart
			Chart chart = new Chart();

			// Set the chart width and height
			//chart.Width = 500;
			//chart.Height = 300;

			// Create a new instance of Title
			Title title = new Title();

			// Set title property
			title.Text = "Visifire Sample Chart";

			// Add title to Titles collection
			chart.Titles.Add(title);

			// Create a new instance of DataSeries
			DataSeries dataSeries = new DataSeries();

			// Set DataSeries property
			dataSeries.RenderAs = RenderAs.Column;

			// Create a DataPoint
			DataPoint dataPoint;

			for (int i = 0; i < 5; i++) {
				// Create a new instance of DataPoint
				dataPoint = new DataPoint();

				// Set YValue for a DataPoint
				dataPoint.YValue = rand.Next(10, 100);

				// Add dataPoint to DataPoints collection
				dataSeries.DataPoints.Add(dataPoint);
			}

			// Add dataSeries to Series collection.
			chart.Series.Add(dataSeries);

			// Add chart to LayoutRoot
			LayoutRoot.Children.Add(chart);
		}

		/// <summary>
		/// Create a random class variable
		/// </summary>
		Random rand = new Random(DateTime.Now.Millisecond);
	}
}
