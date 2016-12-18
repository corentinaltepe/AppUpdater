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

 Read the Getting Started guide for details on installing and using AppUpdater.
 
## AppUpdaterClient
 Checking for updates:
 ```c#
 // Create an updater client instance. 
 // Give the address and port (xxxx) of your server hosting AppUpdaterService
 AppUpdater updater = new AppUpdater("http://my-server.com:xxxx/api/");
 
 // Check for update and call HandleNewerAppResponse when response is received
 AppUpdater.CheckNewerVersionAvailableAsync(
                updateAvailable => HandleNewerAppResponse(updateAvailable));
				
				
// Called when an answer is receive from the API, whether a newer app is available or not
private void HandleNewerAppResponse(bool updateAvailable)
{
	if (updateAvailable)
	{
		// NewerApp contains only metadata about your app.
		// It was not downloaded yet.
		var app = AppUpdater.NewerApp;
		
		Console.WriteLine("Name: " + app.Name);
		Console.WriteLine("VersionStr: " + app.VersionStr);
		Console.WriteLine("Version: " + app.Version);
		Console.WriteLine("Sha256: " + app.Sha256);
		Console.WriteLine("Filename: " + app.Filename);
		Console.WriteLine("Filesize: " + app.Filesize);
	}
	else
		Console.WriteLine("No update currently available.");
}
 ```

 Downloading update (if any available):
 ```c#
// Start the download
Updater.DownloadAsync(downloaded => Updater_AppDownloaded(downloaded));

// Application was downloaded in tmp files and verified if appDownloaded is true.
private void Updater_AppDownloaded(bool appDownloaded)
{
	if(appDownloaded)
		// This call will kill the running application's process.
		// Make sure you have saved your work before calling InstallUpdate()
		// or prompt the user with a Yes/No dialog
		Updater.InstallUpdate();
	else
		Console.WriteLine("Error occured during download.");
}
 ```
 
More info to come
