# KVK\_DataAccess

**KVK\_DataAccess** is a Windows Forms-based desktop application that automates battery charging and discharging based on live electricity tariffs, short-term weather forecasts, and real-time sensor data received from an EWS (Energy Web Service) interface. It connects to various APIs and updates a SQL Server database to reflect current battery activity, enabling data analysis and informed decision-making.

## 💡 Main Capabilities

* **Dynamic Electricity Pricing**: Pulls up-to-date electricity rates from the Nord Pool API and refreshes data every minute.
* **Weather-Based Behavior Adjustment**: Integrates with the Open-Meteo API to obtain a 3-day forecast, which influences battery management decisions.
* **Sensor Data via EWS**: Interfaces with a SOAP-based EWS service to fetch real-time ISS (instantaneous sensor) readings and battery metrics.
* **Automated Battery Management**:

  * Charges when the battery level is low or when energy prices fall below a user-defined limit.
  * Discharges during high-tariff periods or prior to a scheduled return home.
  * Takes into account time of day (e.g., night or afternoon) and forecasted weather conditions.
* **SQL Server Integration**: Logs current battery state and decision-making scenarios into the `BIP_Result` table used by the `Yellow_Team`.
* **Adjustable Parameters**: Allows users to configure the buying price threshold and expected home arrival time through the interface.

## 🗌 Requirements

* **Operating System**: Windows 10 or newer
* **.NET Framework**: 4.7.2 or later (or .NET Core 3.1+ if migrated)
* **IDE**: Visual Studio 2019 or 2022 with Windows Forms workload installed
* **Database**: SQL Server 2016 or higher available on your network
* **Network Connectivity**:

  * Access to the EWS server (e.g., `172.16.16.60`)
  * Internet access for `api.open-meteo.com` and `dashboard.elering.ee`

## 🔧 Setup & Configuration

1. **Clone the Repository**

   ```bash
   git clone https://github.com/your-org/KVK_DataAccess.git
   cd KVK_DataAccess
   ```

2. **Edit Database and Network Settings**

   * In `Form1.cs`, find the `MSSQL` method and update the `connectionString` with the correct server name, database, and credentials.
   * In the `GetPower()` method, modify the `NetworkCredential` settings if your EWS client uses authentication.

3. **UI Initialization**

   * Launch the form designer (`Form1`) and set default values for `ComingHomeTime` and `BuyingPrice` fields to suit your preferences.

4. **Modify API URLs if Needed**

   * **Open-Meteo**: Default location is set to `55.7033, 21.1443`. You can adjust this in the `Weather()` function.
   * **Nord Pool**: Uses the endpoint `https://dashboard.elering.ee/api/nps/price`, which requires no API key.

## 🚀 Running the Application

1. Open the solution in Visual Studio and go to **Build ▶ Build Solution**.
2. Start the app via **Debug ▶ Start Debugging** or by pressing **F5**.
3. The UI will show:

   * **ISS** – current instantaneous sensor reading
   * **Battery** – the battery's state-of-charge percentage
   * **Current Price** – latest electricity rate in EUR/kWh
   * **Battery Status** – indicates Charging, Discharging, or Idle
4. The application refreshes its data every minute and syncs with the database accordingly.

## 📂 Directory Structure

```
KVK_DataAccess/
├─ Form1.cs                # Main UI and logic controller
├─ Program.cs              # Application entry point
├─ KVK_DataAccess.csproj   # Project file
└─ README.md               # This documentation
```

## 🔄 Extending the Application

* **Forecast Range**: In `timer2_Tick`, change the argument in `Weather(3)` to adjust the number of forecast days.
* **Custom Battery Logic**: Add or modify rules inside `mainLoop()` to fit specific charging/discharging strategies.
* **Database Enhancements**: Extend the `BIP_Result` schema or create new tables to store additional metrics or logs.

## 🛠️ Troubleshooting Tips

* **EWS Connection Issues**: If the EWS service fails, the `GetPower()` method will trigger a message box with an error message.
* **SQL Server Access**: Ensure the database accepts remote connections and your credentials have the necessary permissions.
* **API Limits**: Be aware of request limits imposed by Open-Meteo and Elering; implement local caching if needed.

## 📄 License

This project is distributed under the MIT License. See [LICENSE](LICENSE) for full terms.
