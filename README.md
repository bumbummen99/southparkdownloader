# SouthPark Downloader
[![Seasons](https://img.shields.io/badge/Seasons-22-brightgreen.svg)](https://github.com/bumbummen99/southparkdownloader)

A small C# application to download episodes from southpark.de using ffmpeg and youtube-dl.
The application will download the neccesary dependencies and provides easy to use commands.

Most of the time the ```download``` command will be enough as it will download the episode parts
and merge the resulting video files during process for you.

If you encounter any issues while downloading / merging an episode the programm should continue fine. You can
run the command ```download``` again to redownload incomplete episodes or ```process``` to merge unmerged parts.

If you don't encounter any bugs there is no need to update the software every time you use it, you should definitely if you are going for a full download.
In order to update the episode list / database simply use the command ```index update```. If the current index is broken or outdated (which i hope is not) you can fix 
the index file manually and re-index it with the command ```index```.

Feel free to kep the index up-to-date by creating a pull-request.

# Requirements
- .Net 4.6.1+

# Commands

## download
Downloads all episodes in the episodes list. The command will download all parts and merge them accordingly, deleting unecessary files.

## process
Merges unmerged video parts. Use this command if the software encounters a problem during the download process.

## index
Reloads the index by reading the local index file.

## index update
Replaces the local index with an up-to-date version from this repository and reloads it.
