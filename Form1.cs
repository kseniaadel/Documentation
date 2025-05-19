using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

using KVK_DataAccess.EWS_PME;
using Newtonsoft.Json.Linq;

namespace KVK_DataAccess
{
    /// <summary>
    /// Represents the possible battery statuses for the energy management system.
    /// </summary>
    enum BatteryStatus
    {
        Inactive,
        Charging,
        Discharging
    }

    /// <summary>
    /// Main form class for the KVK Data Access application.
    /// Handles UI updates, API calls, and database updates related to battery and pricing.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Current electricity price in EUR/kWh.
        /// </summary>
        double CurrentPrices = 0.1;

        /// <summary>
        /// Weather forecast descriptions for upcoming days.
        /// </summary>
        List<string> Forecast = new List<string>();

        /// <summary>
        /// Weather codes used for determining weather conditions.
        /// </summary>
        List<int> m_Codes = new List<int>();

        /// <summary>
        /// Current status of the battery (Inactive, Charging, Discharging).
        /// </summary>
        BatteryStatus CurrentBatteryStatus = BatteryStatus.Inactive;

        /// <summary>
        /// Maximum price threshold for buying electricity.
        /// </summary>
        double BUYING_PRICE = 0;

        /// <summary>
        /// Hour of the day when the user is expected to return home.
        /// </summary>
        int COMING_HOME_TIME = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class,
        /// sets initial values from UI inputs, and starts the main loop.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            COMING_HOME_TIME = Convert.ToInt32(ComingHomeTime.Text);
            BUYING_PRICE = Convert.ToDouble(BuyingPrice.Text);
            mainLoop();
        }

        /// <summary>
        /// Timer tick event handler that updates data and UI elements periodically.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        void timer1_Tick(object sender, EventArgs e)
        {
            mainLoop();

            CurrentPricesValue.Text = $"{CurrentPrices} EUR/kWh";
            BatteryStatusValue.Text = CurrentBatteryStatus.ToString();
        }

        /// <summary>
        /// Core logic loop that retrieves power data, evaluates conditions, and updates database accordingly.
        /// </summary>
        void mainLoop()
        {
            List<double> list = GetPower();
            NordPool();
            double battery = list[1];
            double ISS = list[0];

            ISSValue.Text = ISS.ToString();
            BatteryValue.Text = battery.ToString();

            int currentTime = DateTime.Now.Hour;

            if (battery < 15)
            {
                Console.WriteLine("charging");
                MSSQL(BatteryStatus.Charging, 5);
                return;
            }

            bool isNight = currentTime < 6 || currentTime >= 23;
            if (isNight)
            {
                Console.WriteLine(m_Codes[0] < 3 ? "sunny and discharging" : "bad weather and inactive");
                MSSQL(m_Codes[0] < 3 ? BatteryStatus.Discharging : BatteryStatus.Inactive, 5);
                return;
            }

            if (currentTime > COMING_HOME_TIME - 2 && currentTime < COMING_HOME_TIME)
            {
                Console.WriteLine(battery < 99 ? "we are charging" : "inactive");
                MSSQL(battery < 99 ? BatteryStatus.Charging : BatteryStatus.Inactive, 5);
                return;
            }

            if (ISS > 0)
            {
                if (CurrentPrices < BUYING_PRICE)
                {
                    Console.WriteLine(battery < 55
                        ? "not enough electricity and low price - buying, we are charging"
                        : "too expensive we are inactive");
                    MSSQL(battery < 55 ? BatteryStatus.Charging : BatteryStatus.Inactive, 5);
                }
                else
                {
                    Console.WriteLine("discharging");
                    MSSQL(BatteryStatus.Discharging, 5);
                }
                return;
            }

            bool isAfternoon = currentTime > 12 && currentTime < 18;
            Console.WriteLine((isAfternoon && battery < 99) ? "charging" : "inactive");
            MSSQL((isAfternoon && battery < 99) ? BatteryStatus.Charging : BatteryStatus.Inactive, 5);
        }

        /// <summary>
        /// Retrieves weather forecast data for a specified number of days using the Open-Meteo API.
        /// Populates internal weather code list and builds forecast descriptions.
        /// </summary>
        /// <param name="daysToForecast">Number of days to retrieve forecast for.</param>
        async void Weather(int daysToForecast)
        {
            string url = "https://api.open-meteo.com/v1/forecast?latitude=55.7033&longitude=21.1443&daily=temperature_2m_max,temperature_2m_min,weathercode&timezone=Europe%2FRiga";

            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(url);

            JsonDocument doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            var dates = root.GetProperty("daily").GetProperty("time");
            var codes = root.GetProperty("daily").GetProperty("weathercode");

            Dictionary<int, string> weatherCodeMap = new Dictionary<int, string>
            {
                {0, "Clear sky"},
                {1, "Mainly clear"},
                {2, "Partly cloudy"},
                {3, "Cloudy"},
                {45, "Fog"}, {48, "Depositing rime fog"},
                {51, "Drizzle (light)"}, {53, "Drizzle (moderate)"}, {55, "Drizzle (heavy)"},
                {56, "Freezing drizzle (light)"}, {57, "Freezing drizzle (heavy)"},
                {61, "Rain (light)"}, {63, "Rain (moderate)"}, {65, "Rain (heavy)"},
                {66, "Freezing rain (light)"}, {67, "Freezing rain (heavy)"},
                {71, "Snowfall (light)"}, {73, "Snowfall (moderate)"}, {75, "Snowfall (heavy)"},
                {77, "Snow grains"}, {80, "Rain showers (light)"}, {81, "Rain showers (moderate)"}, {82, "Rain showers (heavy)"},
                {85, "Snow showers (light)"}, {86, "Snow showers (heavy)"},
                {95, "Thunderstorm (light)"}, {96, "Thunderstorm (moderate)"}, {99, "Thunderstorm (heavy)"}
            };

            StringBuilder weatherInfo = new StringBuilder();

            m_Codes.Clear();
            for (int i = 0; i < daysToForecast; i++)
            {
                string date = dates[i].GetString();
                int code = codes[i].GetInt32();
                m_Codes.Add(code);
                string description = weatherCodeMap.ContainsKey(code) ? weatherCodeMap[code] : "Unknown";
                weatherInfo.AppendLine($"[{date}] Weather: {description}");
            }
        }

        /// <summary>
        /// Retrieves the latest electricity prices from the Nord Pool API and updates the current price field.
        /// </summary>
        async void NordPool()
        {
            var client = new HttpClient();
            string url = "https://dashboard.elering.ee/api/nps/price";

            var response = await client.GetStringAsync(url);
            JObject json = JObject.Parse(response);

            var prices = json["data"]["ee"];

            foreach (var item in prices)
            {
                long timestamp = item["timestamp"].Value<long>();
                CurrentPrices = item["price"].Value<double>() / 1000;
            }
        }

        /// <summary>
        /// Retrieves power values (ISS and battery) from the EWS service.
        /// </summary>
        /// <returns>
        /// A list containing two doubles: ISS value at index 0 and battery value at index 1.
        /// </returns>
        List<double> GetPower()
        {
            var client = new DataExchangeClient();
            client.ClientCredentials.HttpDigest.ClientCredential = new NetworkCredential("kvk", "KvK-DataAccess1", "172.16.16.60");
            client.ClientCredentials.HttpDigest.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

            try
            {
                var request = new GetValuesRequest
                {
                    GetValuesIds = new[] { "4@1042@V", "9@-685@V" },
                    version = "1.0"
                };

                var response = client.GetValues(request);

                if (response.GetValuesItems == null || response.GetValuesItems.Length == 0)
                {
                    return new List<double>();
                }

                var ISS = response.GetValuesItems[0];
                var battery = response.GetValuesItems[1];

                return new List<double>
                {
                    Convert.ToDouble(ISS.Value),
                    Convert.ToDouble(battery.Value)
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Service Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                client.Close();
            }

            return new List<double>();
        }

        /// <summary>
        /// Updates the SQL database with the current battery status and scenario for the Yellow Team.
        /// </summary>
        /// <param name="batteryStatus">The new status of the battery.</param>
        /// <param name="ElectricityScenario">The scenario identifier for electricity usage.</param>
        void MSSQL(BatteryStatus batteryStatus, int ElectricityScenario)
        {
            CurrentBatteryStatus = batteryStatus;
            string connectionString = "Server=172.16.16.60;Database=BIP_Project;User Id=kvk;Password=KvK-DataAccess1;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string updateQuery = "UPDATE BIP_Result SET BatteryStatus = @batteryStatus, Scene = @Scenario WHERE Team_Name = 'Yellow_Team'";
                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@batteryStatus", Convert.ToInt32(batteryStatus));
                    command.Parameters.AddWithValue("@Scenario", ElectricityScenario);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Timer tick event handler that refreshes the weather forecast at regular intervals.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            Weather(3);
        }
    }
}
