All contributions, discussions, suggestions are warmly welcome.

# AppUpdater
 C# ASP.Net service API, client and bootloader enabling desktop application field updates.
 Do you need your .Net desktop applications to automatically update while the users seat and relax? This project is for you. 
 
 
# Specifications
 * Compatible .Net Framework 4.5
 * For all desktop applications: WPF, Windows Forms, and Console (and ASP.Net?)
 * AppUpdaterService: ASP.Net Web Api 2.0. Compatible Linux using Mono. Runs well on Beaglebone Black (Raspberry Pi like board).
 * Encryption of sensitive information between Service and Client to protect your apps from unwanted downloads
 * No GUI for the client. It's up to you to build the GUI, and decide whether the checks and updates are enforced, executed in the background, or require user's authorization.
 
# Installing
 All you need is:
 1. A server running AppUpdaterService: ASP.Net 2.0 Web API. Compatible with Mono for Linux servers. Tested on Beaglebone black.
 2. Adding AppUpdaterClient, AppLib & Bootloader.exe to your project.

## AppUpdaterClient
 Add the latest release of the client to your project root: Booloader.exe, AppLib.dll & AppUpdaterClient.dll. Make sure Bootloader.exe is copied to binary folder at build. Add a reference to AppLib.dll & AppUpdaterClient.dll in your project.
 
 Create App.xml
 More to come
 
 ```c#
 // Create an updater client instance. 
 // Give the address and port (xxxx) of your server hosting AppUpdaterService
 AppUpdater updater = new AppUpdater("http://my-server.com:xxxx/api/");
 
 ```
 
More info to come
