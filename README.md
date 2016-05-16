This is a portable version of https://github.com/flagbug/YoutubeExtractor

I've simplified the API, though. Some examples:

## Example code

**Get the download URLs**

```c#

// Our test youtube link
string link = "insert youtube link";

/*
 * Get the available video formats.
 * We'll work with them in the video and audio download examples.
 */
IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

```

**Download video**

```c#

//construct downloader
var downloader = new VideoDownloader();

//download video infos
IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync("https://www.youtube.com/watch?v=vxMxYgkUcdU");

//Select best suited video (highest resolution)
VideoInfo video =  downloader.ChooseBest(videoInfos);
  
//If the video has a decrypted signature, decipher it
if (video.RequiresDecryption)
    DownloadUrlResolver.DecryptDownloadUrl(video);

// Register the any events
downloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);

//Execute the video downloader.
var stream = await downloader.Execute(videoInfo);

//save stream to file, the correct extension is saved in video.

```

**Download audio**

```c#

//construct downloader
var downloader = new AudioDownloader();

//download video infos
IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync("https://www.youtube.com/watch?v=vxMxYgkUcdU");

//Select best suited video (highest resolution)
VideoInfo video =  downloader.ChooseBest(videoInfos);
  
//If the video has a decrypted signature, decipher it
if (video.RequiresDecryption)
    DownloadUrlResolver.DecryptDownloadUrl(video);

// Register the any events
downloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);

//Execute the video downloader.
var stream = await downloader.Execute(videoInfo);

//save stream to file, the correct extension is saved in video.

```
