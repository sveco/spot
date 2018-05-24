using SpotifyAPI.Local;
using SpotifyAPI.Local.Models;
using System;
using System.Drawing;

namespace spot
{
	class Program
	{
		private static void ShowHelp()
		{
			var h = @"Spotify cli interface. Requires Spotify to be running locally.
usage: spot [options]
  options:
    -pp, --pause         Pauses or unpauses Spotify
    -p, --prev           Previous track
    -n, --next           Plays next track (skip)
    -v, --volume <0-100> Sets Spotify volume (Windows7+)
    -u, --url <url>      Plays Spotify url
    -h, --help           Shows help
    -g, --goto <a|b>     Goto (a) Artist, (b) alBum
    -s, --status         Shows current Spotify status, also if no arguments are entered
    -e, --export <path>  Exports current album art to specified folder as album.jpg
";

			Console.WriteLine(h);
		}

		static void Main(string[] args)
		{
			ArgumentParser parser = new ArgumentParser(args);

			if (parser.GetArgValue<bool>(new string[] { "h", "-help" }))
			{
				ShowHelp();
			}

			var spotifyHandler = new SpotifyHandler();

			if (spotifyHandler.IsConnected)
			{
				spotifyHandler.RefreshStatus();

				if (parser.GetArgValue<bool>(new string[] { "-pause", "pp" }))
				{
					spotifyHandler.PlayPause();
				}

				if (parser.GetArgValue<bool>(new string[] { "-prev", "p" }))
				{
					spotifyHandler.Previous();
				}

				if (parser.GetArgValue<bool>(new string[] { "-next", "n" }))
				{
					spotifyHandler.Next();
				}

				var e = parser.GetArgValue<string>(new string[] { "-export", "e" });
				if (!String.IsNullOrEmpty(e))
				{
					spotifyHandler.ExportAlbumArt(e);
				}

				var v = parser.GetArgValue<int>(new string[] { "v", "-volume" }, -1);
				if (v >= 0 && v <= 100)
				{
					spotifyHandler.SetSpotifyVolume(v);
				}

				var u = parser.GetArgValue<string>(new string[] { "u", "-url" });
				if (!String.IsNullOrEmpty(u))
				{
					spotifyHandler.PlayURL(u);
				}

				var g = parser.GetArgValue<string>(new string[] { "g", "-goto" }, string.Empty);
				if (!String.IsNullOrEmpty(g))
				{
					spotifyHandler.GoToUrl(g);
				}

				//Status should be last
				if (
				  args.Length == 0 ||
				  parser.GetArgValue<bool>(new string[] { "s", "-status" }))
				{
					spotifyHandler.WriteStatus();
				}
			}
		}
	}
}
