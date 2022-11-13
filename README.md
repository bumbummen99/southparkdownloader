# SouthPark Downloader
[![Build](https://github.com/bumbummen99/southparkdownloader/actions/workflows/Build.yml/badge.svg)](https://github.com/bumbummen99/southparkdownloader/actions/workflows/Build.yml)
[![Seasons](https://img.shields.io/badge/Seasons-24-brightgreen.svg)](https://github.com/bumbummen99/southparkdownloader)
[![NetCore](https://img.shields.io/badge/NetCore-3.1-green.svg)](https://github.com/bumbummen99/southparkdownloader)
[![Homepage](https://img.shields.io/badge/homepage-skyraptor.eu-informational.svg?style=flat&logo=appveyor)](https://skyraptor.eu)


A small C# application to download all episodes from [southpark.de](http://www.southpark.de/) using [ffmpeg](https://www.ffmpeg.org/) and [youtube-dl](https://rg3.github.io/youtube-dl/).
Feel free to kep the index up-to-date by creating a pull-request.

## Important
This software is just a fancy wrapper around YouTubeDL and FFMPEG specifically created to download episoded from SouthPark.de, correctly sort the part files, stitch them together losslessly and then create a final episode file with the correct media tags and filename.
This software does depend on the functionality of YouTubeDL and FFMPEG, YouTubeDL can break **anytime** due to it's nature in being dependet itself on the site you are trying to download from i.e. if SouthPark.de changes YouTubeDL can be incompatible. For these situations you might want to use the YouTubeDL Community Fork, a fork that incorporates fixes for YouTubeDL much faster. You can add the parameter `ytdlc` to the startup of SouthParkDL in order to use YouTubeDL Community instead of the official version on Windows. For other platforms, please follow the installation notes (just replace youtube-dl with youtube-dlc).

## Requirements
- [Latest VC Redist x64 and x86](https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170) (Windows)
- youtube-dl (Linux / OSX)
- ffmpeg (Linux / OSX)

## Installation
### Windows
The software requires [Latest VC Redist x64 and x86](https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170) to be installed in order to function properly.
Simply unzip the latest release and run the included executable, the software will automatically download and install its dependencies (portable) including youtube-dl and ffmpeg.

You can also use the GUI package which provides an easy to use installer and user interface.

### Linux
You need to install ffmpeg and youtube-dl manually as the software depends on it.

```
sudo apt-get install ffmpeg youtube-dl
```

Download the latest release for your operating system and unzip it. You can run the software with the following command:
```
./SouthParkDownloaderNetCore
```

### OSX
You need to install ffmpeg and youtube-dl as the software depends on it.

- [Youtube-DL](https://rg3.github.io/youtube-dl/download.html)
- [FFMpeg](https://evermeet.cx/ffmpeg/)

Download the latest release for your operating system and unzip it. You can run the software with the following command:
```
./SouthParkDownloaderNetCore
```

## Commands
Most of the time the ```download``` command will be enough as it will download the episode parts
and merge the resulting video files during process for you.

If you encounter any issues while downloading / merging an episode the programm should continue fine. You can
run the command ```download``` again to redownload incomplete episodes or ```process``` to merge unmerged parts.

If you don't encounter any bugs there is no need to update the software every time you use it, you should definitely if you are going for a full download.
In order to update the episode list / database simply use the command ```index update```. If the current index is broken or outdated (which i hope is not) you can fix 
the index file manually and re-index it with the command ```index```.

### download
Downloads all episodes in the episodes list. The command will download all parts and merge them accordingly, deleting unecessary files.

### process
Merges unmerged video parts. Use this command if the software encounters a problem during the download process.

### index
Reloads the index by reading the local index file.

### index update
Replaces the local index with an up-to-date version from this repository and reloads it.

## Collaborators

<!-- readme: collaborators -start -->
<table>
<tr>
    <td align="center">
        <a href="https://github.com/bumbummen99">
            <img src="https://avatars.githubusercontent.com/u/4533331?v=4" width="100;" alt="bumbummen99"/>
            <br />
            <sub><b>Patrick</b></sub>
        </a>
    </td></tr>
</table>
<!-- readme: collaborators -end -->

## Contributors

<!-- readme: contributors -start -->
<table>
<tr>
    <td align="center">
        <a href="https://github.com/bumbummen99">
            <img src="https://avatars.githubusercontent.com/u/4533331?v=4" width="100;" alt="bumbummen99"/>
            <br />
            <sub><b>Patrick</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/PPrzemko">
            <img src="https://avatars.githubusercontent.com/u/38755500?v=4" width="100;" alt="PPrzemko"/>
            <br />
            <sub><b>TheWhiteSheep</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/NicoVIII">
            <img src="https://avatars.githubusercontent.com/u/3983345?v=4" width="100;" alt="NicoVIII"/>
            <br />
            <sub><b>NicoVIII</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/TH42">
            <img src="https://avatars.githubusercontent.com/u/24369300?v=4" width="100;" alt="TH42"/>
            <br />
            <sub><b>TH42</b></sub>
        </a>
    </td></tr>
</table>
<!-- readme: contributors -end -->
