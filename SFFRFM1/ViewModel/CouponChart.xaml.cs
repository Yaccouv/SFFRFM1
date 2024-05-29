using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using MySqlConnector;
using System.Linq;
using OxyPlot.Xamarin.Forms;
using SFFRFM1.Model;

namespace SFFRFM1.ViewModel
{
    public partial class CouponChart : ContentPage
    {
        public PlotModel PlotModel { get; set; }

        public CouponChart()
        {
            InitializeComponent();
            InitializeUI();
        }

        private async void InitializeUI()
        {
            try
            {
                PlotModel = await GeneratePlotModel();
                BindingContext = this; // Set the BindingContext
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task<PlotModel> GeneratePlotModel()
        {
            var plotModel = new PlotModel
            {
                Title = "Coupon Distribution Report",
                IsLegendVisible = true
            };

            try
            {
                var regionYearlyDistribution = await FetchRegionYearlyDistribution();

                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Left, // Change to AxisPosition.Left for Y-axis
                    Title = "Region",
                    TickStyle = TickStyle.Crossing
                };

                var linearAxis = new LinearAxis
                {
                    Position = AxisPosition.Bottom, // Change to AxisPosition.Bottom for X-axis
                    Title = "Number of Coupons",
                    MinimumPadding = 0.1,
                    MaximumPadding = 0.1
                };

                foreach (var kvp in regionYearlyDistribution)
                {
                    string region = kvp.Key;
                    var yearlyDistribution = kvp.Value;

                    var barSeries = new BarSeries
                    {
                        Title = region,
                        FillColor = OxyColor.Parse(GetRandomHexColor())
                    };

                    foreach (var distribution in yearlyDistribution)
                    {
                        int distributeYear = distribution.Key;
                        int couponCount = distribution.Value;

                        barSeries.Items.Add(new BarItem { Value = couponCount });
                        categoryAxis.Labels.Add(distributeYear.ToString());
                    }

                    plotModel.Series.Add(barSeries);
                }

                plotModel.Axes.Add(linearAxis); // Add LinearAxis to X-axis
                plotModel.Axes.Add(categoryAxis); // Add CategoryAxis to Y-axis
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating plot model: " + ex.Message);
            }

            return plotModel;
        }


        private async Task<Dictionary<string, Dictionary<int, int>>> FetchRegionYearlyDistribution()
        {
            var regionYearlyDistribution = new Dictionary<string, Dictionary<int, int>>();

            using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
            {
                connection.Open();

                string query = "SELECT Region, DistributeYear, COUNT(*) AS CouponCount FROM Coupon GROUP BY Region, DistributeYear";
                MySqlCommand command = new MySqlCommand(query, connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string region = reader.GetString("Region");
                        int distributeYear = reader.GetInt32("DistributeYear");
                        int couponCount = reader.GetInt32("CouponCount");

                        if (!regionYearlyDistribution.ContainsKey(region))
                        {
                            regionYearlyDistribution[region] = new Dictionary<int, int>();
                        }

                        regionYearlyDistribution[region].Add(distributeYear, couponCount);
                    }
                }
            }

            return regionYearlyDistribution;
        }

        private string GetRandomHexColor()
        {
            Random rand = new Random();
            return String.Format("#{0:X6}", rand.Next(0x1000000));
        }
    }
}
