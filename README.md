# KVK\_DataAccess

KVK\_DataAccess is a Windows Forms application designed to automate battery charging and discharging decisions based on real-time electricity prices, weather forecasts, and sensor data from an EWS (Energy Web Service) client. The application integrates with external APIs and updates a SQL Server database to reflect the current battery status for further analysis.

## 💡 Key Features

* **Real-Time Electricity Pricing**: Fetches current electricity prices from the Nord Pool API and updates the application every minute.
* **Weather Forecast Integration**: Retrieves a 3-day weather forecast from the Open-Meteo API to influence battery behavior under changing weather conditions.
* **EWS Data Retrieval**: Connects to a SOAP-based EWS service to read current ISS (instantaneous sensor) and battery values.
* **Automated Battery Logic**:

  * Charges when battery level is low or when prices are below a user-defined threshold.
  * Discharges during peak price periods or when returning home is imminent.
  * Considers time of day (night, afternoon) and weather conditions.
* **SQL Server Updates**: Writes current battery status and scenario data to the `BIP_Result` table for the `Yellow_Team`.
* **Configurable Settings**: Adjust buying price threshold and expected return time directly in the UI.

## 📋 Prerequisites

* **Operating System**: Windows 10 or later
* **.NET Framework**: 4.7.2 or higher (or .NET Core 3.1+ if ported)
* **Development Environment**: Visual Studio 2019/2022 with Windows Forms workload
* **Database**: SQL Server 2016+ accessible at your network
* **Network Access**:

  * EWS service endpoint (e.g., `172.16.16.60`)
  * Internet access to `api.open-meteo.com` and `dashboard.elering.ee`

## 🔧 Configuration

1. **Clone the Repository**

   ```bash
   git clone https://github.com/your-org/KVK_DataAccess.git
   cd KVK_DataAccess
   ```

2. **Update Connection Strings and Credentials**

   * In `Form1.cs`, locate the `MSSQL` method and update the `connectionString` variable with your server, database, and credentials.
   * In `GetPower()`, update the `NetworkCredential` for the EWS client if needed.

3. **Set UI Defaults**

   * Open `Form1` in the Designer.
   * Set the default values for `ComingHomeTime` and `BuyingPrice` textboxes to your desired thresholds.

4. **API Endpoints**

   * **Open-Meteo**: Latitude/Longitude currently set to `55.7033, 21.1443`. Modify the URL in `Weather()` if needed.
   * **Nord Pool**: Endpoint is `https://dashboard.elering.ee/api/nps/price`. No additional credentials required.

## 🚀 Running the Application

1. **Build** the solution in Visual Studio: **Build ▶ Build Solution**.
2. **Run** the application: **Debug ▶ Start Debugging** (or press F5).
3. The main window will display:

   * **ISS**: Instantaneous sensor reading.
   * **Battery**: Current battery percentage.
   * **Current Price**: Latest electricity price (EUR/kWh).
   * **Battery Status**: Charging, Discharging, or Inactive.
4. The app will auto-refresh data every minute and update the database accordingly.

## 📂 Project Structure

```
KVK_DataAccess/
├─ EWS_PME/                # Proxy classes for EWS SOAP client
├─ Form1.cs                # Main logic and UI controller
├─ Program.cs              # Application entry point
├─ KVK_DataAccess.csproj   # Project file
└─ README.md               # Project documentation
```

## 🔄 Customization & Extensibility

* **Forecast Days**: Change the parameter in `timer2_Tick` (calls `Weather(3)`) to adjust the number of forecast days.
* **Charging Logic**: Modify `mainLoop()` to add new conditions or thresholds.
* **Database Schema**: Extend the `BIP_Result` table or add new tables to capture more data points.

## 🛠️ Troubleshooting

* **Service Errors**: Catch blocks in `GetPower()` will show a message box if the EWS client fails.
* **Database Connection**: Ensure SQL Server accepts remote connections and the user has appropriate permissions.
* **API Rate Limits**: Be mindful of open-meteo and elering.ee rate limits; consider caching if necessary.

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

Developed by the KVK Team. For questions or contributions, please open an issue or submit a pull request on GitHub.
