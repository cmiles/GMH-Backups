# GMH Backups

This project has a number of small backup programs related to various sensor and system that we use at our house - GMH.

There is no intent to create releases/packages from this project. It is public to share with friends and, hopefully, to save someone a few minutes of time working with some of these APIs!

The backups currently produced by this program are flat files with limited time periods and minimal processing. This makes the backups unattractive as a data source for an API, but after experimenting with the APIs/data it seems clear that producing clean, easy to query, SQLite output would be too opinionated, take too much transformation and ultimately not be as good of a backup.

This project uses my [VaultfuscatedSettings](https://github.com/cmiles/VaultfuscatedSettings) project to encrypt settings files with a key stored in a local credential manager/vault. This provides ZERO security if an attacker gains full access to your user account!! But it also means that the files cannot be casually viewed, that they are encrypted securely from any attacker who doesn't have access to your local credential manager/vault and that the settings files can still be used for automated/no-human-interaction runs of the program.

**These small local programs do not run with 'Total Security' and the backups produced by this program are NOT encrypted or protected! You will have to decide for yourself if this is acceptable.**

## Background

I am completely confident that some of the backups I am making will never be useful (in anyway, to anyone, for anything!) - so why spend anytime writing this code?
 - I care about our house and where we live - I hope to be lucky enough to live here for a very long time, and I care about the data and our history in this place. The companies running the APIs these backups use appear to care about their customers and have great intentions!! But they don't care about my specific data nearly as much as I do.
 - My guess is that if any of this data proves interesting it won't be tomorrow or next week - rather it will be next year, or maybe ten/twenty/thirty years from now and it isn't reasonable to count on any company, API, website or product to still exist in 10/20/30 years. These simple flat file backups should be durable enough that they will be accessible decades from now.
 - Fun and Learning - decades into programming and I'm still excited! It is a beautiful challenge to create programs for yourself... 

If you have data you care about you need to own it, back it up and care for it. To me a great example of this is Strava - using Strava to view, present, analyze and share your data is a fun/smart use of time, but counting on Strava to store your data, keep it available and continue to offer reasonably priced access in 10/20/30 years isn't something I want to count on and I know from experience I do care about where I hiked 10+ years ago...

## Backup Tasks

[BirdNET-Pi](https://github.com/mcguirepr89/BirdNET-Pi) - an awesome project that does acoustic detection of birds on a Raspberry Pi and has a nice web interface where you can view the data. Unfortunately we already had our instance die an untimely death once (maybe related to the micro-sd card or an upgrade gone wrong?) - too bad because it this is definitely interesting data to see over time. This code uses SFTP to backup the recommended files and folders [backup the recommended files and folders](https://github.com/mcguirepr89/BirdNET-Pi/wiki/Backup-and-Restore-the-Database) - to keep the backups reasonable (imho) the database and a few other core files are backup up into versioned folders and other data (recordings for example) is kept in a single - non-versioned - folder. Because some files have names that are problematic on windows you will find that some file names are altered for the backup and may take some extra effort to restore.

[Tempest Weather System | WeatherFlow Tempest Inc.](https://weatherflow.com/tempest-home-weather-system/) - weather stations are not a hobby, passion or professional interest of mine and I can't offer any perspective on the Tempest Weather Station vs. other brands/products - but I can say we have enjoyed ours. Compact and without moving parts the station was very easy to setup and I enjoy the app, website and API.

[SensorPush](https://www.sensorpush.com/) - I like SensorPush's line of Temperature/Humidity/Barometric Pressure Sensors and appreciate that they can run either stand alone with data collected via an iOS/Android app or via an (optional) dedicated WiFi Gateway. SensorPush's Cloud API is free if you have a G1 Gateway. The backup pulls information from the API into files by day and device.

[Tucson Electric Power](https://www.tep.com/) - I am not aware of an API so this backup is powered by Playwright.

[Victron Remote (Solar) Monitoring](https://www.victronenergy.com/vrm-portal/vrm/downloads) - A backup of data from the [VRM API](https://vrm-api-docs.victronenergy.com/#/). Of interest to anyone who sees this is that the code in this project might save you a few minutes of time dealing with the stats API return -> it is constructed in a logical, but imho not C# friendly way... Of tangential interest might be my [PiSlicedDayPhotos: A C# .NET Core program to take photos with a Raspberry Pi at sunrise, sunset and intervals thru the day and night.](https://github.com/cmiles/PiSlicedDayPhotos) that runs off a small solar installation at our house!
