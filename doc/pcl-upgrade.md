# Install error on PCL upgrades

> Could not install package 'Nito.AsyncEx 4.\*'. You are trying to install this package into a project that targets '.NETPortable,Version=v\*,Profile=Profile\*', but the package does not contain any assembly references or content files that are compatible with that framework. For more information, contact the package author.

Make note of your profile number; e.g., `.NETPortable,Version=v4.5,Profile=Profile111` indicates that your PCL project is using `Profile111`.

## Why?

AsyncEx v4 no longer uses Visual Studio 2012 to compile its supporting binaries. This means that older platforms *cannot* be supported. These platforms are: Silverlight 4.0, Windows (store) 8.0, and Windows Phone Silverlight 7.1/7.5.

Unfortunately, this means that PCLs using AsyncEx will also need to upgrade so that they no longer target outdated platforms.

## PCL Upgrade Table

This table lists the PCL profiles that were supported in AsyncEx v3 that are no longer supported in AsyncEx v4. Look up your PCL's profile number in the left column, and the right column will give you your upgrade strategy.

**Note** that if the "Upgraded profiles" column contains multiple PCLs, then your PCL project will need to be split into *multiple* PCL projects in order to support the same platforms. If you do not wish to do this, then see below the table.

Profiles no longer supported in v4 | Upgraded profiles
--- | ---
Profile2 (.NET Framework 4, Silverlight 4, Windows 8, Windows Phone Silverlight 7) | Profile14 (.NET Framework 4, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile3 (.NET Framework 4, Silverlight 4) | Profile14 (.NET Framework 4, Silverlight 5)
Profile4 (.NET Framework 4.5, Silverlight 4, Windows 8, Windows Phone Silverlight 7) | Profile24 (.NET Framework 4.5, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile5 (.NET Framework 4, Windows 8) | Profile44 (.NET Framework 4.5.1, Windows 8.1)
Profile6 (.NET Framework 4.0.3, Windows 8) | Profile44 (.NET Framework 4.5.1, Windows 8.1)
Profile7 (.NET Framework 4.5, Windows 8) | Profile44 (.NET Framework 4.5.1, Windows 8.1)
Profile18 (.NET Framework 4.0.3, Silverlight 4) | Profile19 (.NET Framework 4.0.3, Silverlight 5)
Profile23 (.NET Framework 4.5, Silverlight 4) | Profile24 (.NET Framework 4.5, Silverlight 5)
Profile36 (.NET Framework 4, Silverlight 4, Windows 8, Windows Phone Silverlight 8) | Profile14 (.NET Framework 4, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile37 (.NET Framework 4, Silverlight 5, Windows 8) | Profile14 (.NET Framework 4, Silverlight 5)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)
Profile41 (.NET Framework 4.0.3, Silverlight 4, Windows 8) | Profile19 (.NET Framework 4.0.3, Silverlight 5)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)
Profile42 (.NET Framework 4.0.3, Silverlight 5, Windows 8) | Profile19 (.NET Framework 4.0.3, Silverlight 5)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)
Profile46 (.NET Framework 4.5, Silverlight 4, Windows 8) | Profile24 (.NET Framework 4.5, Silverlight 5)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)
Profile47 (.NET Framework 4.5, Silverlight 5, Windows 8) | Profile24 (.NET Framework 4.5, Silverlight 5)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)
Profile78 (.NET Framework 4.5, Windows 8, Windows Phone Silverlight 8) | Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile88 (.NET Framework 4, Silverlight 4, Windows 8, Windows Phone Silverlight 7.5) | Profile14 (.NET Framework 4, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile92 (.NET Framework 4, Windows 8, Windows Phone 8.1) | Profile151 (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)
Profile95 (.NET Framework 4.0.3, Silverlight 4, Windows 8, Windows Phone Silverlight 7) | Profile19 (.NET Framework 4.0.3, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile96 (.NET Framework 4.0.3, Silverlight 4, Windows 8, Windows Phone Silverlight 7.5) | Profile19 (.NET Framework 4.0.3, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile102 (.NET Framework 4.0.3, Windows 8, Windows Phone 8.1) | Profile151 (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)
Profile104 (.NET Framework 4.5, Silverlight 4, Windows 8, Windows Phone Silverlight 7.5) | Profile24 (.NET Framework 4.5, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile111 (.NET Framework 4.5, Windows 8, Windows Phone 8.1) | Profile151 (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)
Profile136 (.NET Framework 4, Silverlight 5, Windows 8, Windows Phone Silverlight 8) | Profile14 (.NET Framework 4, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile143 (.NET Framework 4.0.3, Silverlight 4, Windows 8, Windows Phone Silverlight 8) | Profile14 (.NET Framework 4, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile147 (.NET Framework 4.0.3, Silverlight 5, Windows 8, Windows Phone Silverlight 8) | Profile14 (.NET Framework 4, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile154 (.NET Framework 4.5, Silverlight 4, Windows 8, Windows Phone Silverlight 8) | Profile24 (.NET Framework 4.5, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile158 (.NET Framework 4.5, Silverlight 5, Windows 8, Windows Phone Silverlight 8) | Profile24 (.NET Framework 4.5, Silverlight 5)<br/>Profile31 (Windows 8.1, Windows Phone Silverlight 8.1)<br/>Profile44 (.NET Framework 4.5.1, Windows 8.1)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)
Profile225 (.NET Framework 4, Silverlight 5, Windows 8, Windows Phone 8.1) | Profile14 (.NET Framework 4, Silverlight 5)<br/>Profile151 (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)
Profile240 (.NET Framework 4.0.3, Silverlight 5, Windows 8, Windows Phone 8.1) | Profile19 (.NET Framework 4.0.3, Silverlight 5)<br/>Profile151 (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)
Profile255 (.NET Framework 4.5, Silverlight 5, Windows 8, Windows Phone 8.1) | Profile24 (.NET Framework 4.5, Silverlight 5)<br/>Profile151 (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)
Profile259 (.NET Framework 4.5, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8) | Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)<br/>Profile151 (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)<br/>Profile157 (Windows 8.1, Windows Phone 8.1, Windows Phone Silverlight 8.1)
Profile328 (.NET Framework 4, Silverlight 5, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8) | Profile14 (.NET Framework 4, Silverlight 5)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)<br/>Profile151 (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)<br/>Profile157 (Windows 8.1, Windows Phone 8.1, Windows Phone Silverlight 8.1)
Profile336 (.NET Framework 4.0.3, Silverlight 5, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8) | Profile19 (.NET Framework 4.0.3, Silverlight 5)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)<br/>Profile151 (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)<br/>Profile157 (Windows 8.1, Windows Phone 8.1, Windows Phone Silverlight 8.1)
Profile344 (.NET Framework 4.5, Silverlight 5, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8) | Profile24 (.NET Framework 4.5, Silverlight 5)<br/>Profile49 (.NET Framework 4.5, Windows Phone Silverlight 8)<br/>Profile151 (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)<br/>Profile157 (Windows 8.1, Windows Phone 8.1, Windows Phone Silverlight 8.1)

## Alternatives

If you don't want to split your PCL project into multiple projects (totally understandable), then you can consider dropping support for one or more platforms.

Use the [Portable Class Library Targets app](http://pcltargets.apps.stephencleary.com/) to determine which platforms make sense to support. **Be sure** to **check** the "My project requires platform-specific binaries" checkbox first!
