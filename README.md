# KVK\_DataAccess

**KVK\_DataAccess** is a Windows Forms desktop application designed to control battery charging and discharging using real-time sensor input, electricity prices, and weather forecasts. The system integrates data from a SOAP-based EWS (Energy Web Service), Nord Pool electricity price API, and Open-Meteo weather API, updating a SQL Server database to reflect battery status and decision context.

## 💡 Key Features

* **Electricity Pricing Integration**: Periodically retrieves electricity prices from the Nord Pool API and uses them to determine cost-effective energy usage.
* **Weather-Based Adjustments**: Queries Open-Meteo to get a 3-day weather forecast and adjusts battery logic depending on weather conditions.
* **EWS Data Collection**: Communicates with a SOAP EWS service to obtain current sensor values (ISS) and battery level.
* **Automated Decision Logic**:

  * Charges if battery is low or price is below a defined threshold.
  * Discharges during peak hours or prior to a configured return time.
  * Adapts behavior based on time of day and forecasted weather.
* **SQL Server Logging**: Updates the `BIP_Result` table with the current battery status and selected logic path for the `Yellow_Team`.
* **User Inputs**: Parameters like `BUYING_PRICE` and `COMING_HOME_TIME` are configured through the application UI.

## 📋 Prerequisites

* **Operating System**: Windows 10 or later
* **.NET Framework**: 4.7.2 or higher
* **IDE**: Visual Studio 2019 or 2022 with Windows Forms workload
* **Database**: SQL Server 2016+ with accessible connection
* **Network Access**:

  * SOAP-based EWS server (e.g., `172.16.16.60`)
  * Internet access to `api.open-meteo.com` and `dashboard.elering.ee`

## 🔧 Setup Instructions

1. **Clone the Repository**

   ```bash
   git clone https://github.com/your-org/KVK_DataAccess.git
   cd KVK_DataAccess
   ```

2. **Update Configuration**

   * In `Form1.cs`, modify the connection string inside `MSSQL()` to match your SQL Server credentials.
   * Adjust EWS credentials in `GetPower()` if necessary.

3. **Set UI Defaults**

   * Open `Form1` in the Windows Forms designer.
   * Configure default values for `ComingHomeTime` and `BuyingPrice`.

4. **Verify API Coordinates and Endpoints**

   * **Weather**: Coordinates in `Weather()` default to `55.7033, 21.1443`. Update if needed.
   * **Nord Pool**: Data is fetched from `https://dashboard.elering.ee/api/nps/price`.

## 🚀 Running the Application

1. Build the solution using **Build ▶ Build Solution**.
2. Start the app with **Debug ▶ Start Debugging** or **F5**.
3. The interface will show:

   * **ISS**: Instantaneous sensor value
   * **Battery**: Battery percentage
   * **Current Price**: Electricity rate in EUR/kWh
   * **Battery Status**: Charging / Discharging / Inactive
4. Timers refresh data periodically to maintain real-time state.

## 📂 Project Structure

```
KVK_DataAccess/
├─ EWS_PME/                # Proxy classes for EWS SOAP service
├─ Form1.cs                # Core logic and UI
├─ Program.cs              # Entry point
├─ KVK_DataAccess.csproj   # Project definition
└─ README.md               # Project documentation
```

## 🔄 Customization

* **Forecast Length**: Change the parameter in `timer2_Tick` (calls `Weather(3)`) to modify how many days of weather are retrieved.
* **Decision Rules**: Adjust `mainLoop()` to reflect different thresholds or operational logic.
* **Data Schema**: Expand or adapt the `BIP_Result` table to capture additional details.

## 🛠️ Troubleshooting

* **Connection Errors**: If EWS communication fails, a message box will display the exception.
* **Database Issues**: Verify SQL Server access permissions and remote connection settings.
* **API Quotas**: Be cautious of request limits for Open-Meteo and Elering APIs.

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for more details.

---

Maintained by the KVK Team. Contributions and suggestions are welcome via GitHub pull requests or issues.
