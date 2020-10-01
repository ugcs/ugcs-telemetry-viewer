# UgCS Telemetry Viewer #

## What is Telemetry Viewer?

It is a desktop app that connects to a UgCS server and display user chosen telemetry for the vehicle that is currently selected in UgCS 3D Client.

### Concept ###

The main window of the app is a canvas for widgets. A widget responsible for displaying telemetry somehow. There can be many types of widgets to displaying the same telemetry in different ways (text, indicators, etc). By placing widgets on the canvas the user makes his own telemetry layout.

### Features ###

* Connecting to a UgCS Server:
  * Autodiscovering the UgCS server in the network;
  * Ability to provide the UgCS server address and port if the UgCS server isn't discovered automatically;
  * Blocking UI with a warning when the UgCS server goes offline;
  * Automatically connection restorsion with the UgCS server when the UgCS server goes online.

* Adding widgets on the canvas:
  * Quick search telemetry by a code from the fields, registered by the VSM;
  * Provide additionnal widget-specific data.

* Displaying telemetry for the vehicle selected in the UgCS Desktop Client;
* Widgets reordering by clicking on the arrow buttons.
* Storing layout configuration for each vehicle separately.

### Widgets ###
* Numeric telemetry basic widget:
  * Displaying a number rounded to a specified number of fractional digits with Caption, Units and Thresholds values (if provided by user);
  * Highlighting the value when out of range.

## Why this app was developed?

* For UgCS users who has to control some specific telemetry that is not displayed in UgCS 3D Clientd telemetry panel.
* For developers who use UgCS dotnet-sdk in their projects, as an example of UgCS dotnet-sdk usage.  

## Restrictions

Only numeric<sup>1</sup> telemetry currently supported. 

## Layout examples
Resize the main window to change column count.

| One column | Two columns | Three columns |
|---|---|---|
| ![One column layout](assets/layout-1c.png) | ![One column layout](assets/layout-2c.png) | ![One column layout](assets/layout-3c.png) |

### How do I build? ###

* Prerequisites: [.Net Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1).
* To build the solution execute `dotnet build /p:Configuration=Release;Version=<version>;FileVersion=<version>;AssemblyVersion=<version>` from `/src/` directory. Where `<version>` should be replaced with the actual build version.
* If you want .Net Core dependencies being included into the build, then use `dotnet publish TelemetryViewer.sln -r win-x64 --self-contained true  -p:Configuration=Release;Version="<version>";FileVersion="<version>";AssemblyVersion="<version>"` from `/src/` directory. Where `<version>` should be replaced with the actual build version.

---
<sup>1</sup> - By numeric mean any telemetry field registered by the VSM with the one of following semantics: `S_GPS_FIX_TYPE`, `S_CONTROL_MODE`, `S_ADSB_MODE`, `S_BOOL`, `S_STRING`, `S_ENUM`, `S_FLIGHT_MODE`, `S_LIST`, `S_AUTOPILOT_STATUS`, `S_ICAO`, `S_SQUAWK`, `S_ANY`.