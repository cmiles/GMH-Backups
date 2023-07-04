# GMH Workshop

Personal experiments related to my house. There is no intent to create anything here other than personal a personal project - it is made public to share with friends and anyone who finds the code useful or interesting!

A major motivator for this project is backing up the data produced by the house. I am completely confident that some of this data will never be useful (in anyway, to anyone, for anything!) - but I am also confident that I don't know which data might be useful, interesting or entertaining at some point in the future!

My guess is that if any of this data proves interesting it won't be tomorrow or next week - rather it will be next year, or maybe ten/twenty/thirty years from now and it isn't reasonable to count on any company, API, website or product to still exist in 10/20/30 years. If you have data you care about you need to own it, back it up and care for it. To me a great example of this is Strava - using Strava to view, present, analyze and share your data is a fun/smart use of time, but counting on Strava to store your data, keep it available and continue to offer reasonably priced access in 10/20/30 years isn't something I want to count on...

## Backup Tasks

[BirdNET-Pi](https://github.com/mcguirepr89/BirdNET-Pi) - an awesome project that does acoustic detection of birds on a Raspberry Pi and has a nice web interface where you can view the data. Unfortunately we already had our instance die an untimely death once (maybe related to the micro-sd card or an upgrade gone wrong?) so this code uses SFTP to backup the recommended files and folder [backup the recommended files and folders](https://github.com/mcguirepr89/BirdNET-Pi/wiki/Backup-and-Restore-the-Database). This backup includes quite a few files/folders and quite a bit of data - nice to have it all but the way this backup is setup probably most appropriate for weekly rather than nightly backups.

[Tempest Weather System | WeatherFlow Tempest Inc.](https://weatherflow.com/tempest-home-weather-system/) - weather stations are not a hobby, passion or professional interest of mine and I can't offer any perspective on the Tempest Weather Station vs. other brands/products - but I can say we have enjoyed ours. Compact and without moving parts the station was very easy to setup and I enjoy the app, website and API. Daily data is backed up to files.

[SensorPush](https://www.sensorpush.com/) - I like SensorPush's line of Temperature/Humidity/Barometric Pressure Sensors and appreciate that they can run either stand alone with data collected via an iOS/Android app or via an (optional) dedicated WiFi Gateway. SensorPush's Cloud API is free if you have a G1 Gateway. The backup pulls information from the API into files by day and device.

** Flat Files - Many of the backups above write serialized JSON to a file - you may have expected backup to a database and some backups may also do that, but I believe that while the file may be a bit more work to deal with they will also be more durable over the long term and a more useful format for humans if you just need to quickly open a file.