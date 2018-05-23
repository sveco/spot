using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;

namespace spot
{
  class Program
  {
    private static SpotifyLocalAPIConfig _config;
    private static SpotifyLocalAPI _spotify;
    private static Track _currentTrack;
    private static StatusResponse _currentStatus;
    private static Bitmap _currentArt = null;

    private static async Task GetAlbumArt()
    {
      var art = _currentTrack.AlbumResource != null ? await _currentTrack.GetAlbumArtAsync(AlbumArtSize.Size640, _config.ProxyConfig) : null;
      var art2 = new Bitmap(art);
      _currentArt = art2;
    }
    public static void Connect()
    {
      if (!SpotifyLocalAPI.IsSpotifyRunning())
      {
        Console.WriteLine(@"Spotify isn't running!");
        return;
      }
      if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
      {
        Console.WriteLine(@"SpotifyWebHelper isn't running!");
        return;
      }

      bool successful = _spotify.Connect();
      if (successful)
      {
        //Console.WriteLine(@"Connection to Spotify successful");
      }
      else
      {
        Console.WriteLine(@"Couldn't connect to the spotify client. Retry (Y/n)");
        var res = Console.ReadKey();
        if (res.KeyChar == 'Y' || res.KeyChar == 'y')
          Connect();
      }
    }

    private static void RefreshStatus()
    {
      _currentStatus = _spotify.GetStatus();
      if (_currentStatus != null)
      {
        _currentTrack = _currentStatus.Track;
      }
    }
    static void Main(string[] args)
    {
      ArgumentParser parser = new ArgumentParser(args);

      _config = new SpotifyLocalAPIConfig
      {
        ProxyConfig = new ProxyConfig()
      };

      _spotify = new SpotifyLocalAPI(_config);

      Connect();

      RefreshStatus();

      if (parser.GetArgValue<bool>(new string[] { "-pause", "pp" }))
      {
        if (_currentStatus.Playing)
        {
          _spotify.Pause();
        } else {
          _spotify.Play();
        }
      }

      if (parser.GetArgValue<bool>(new string[] { "-prev", "p" }))
      {
        if (_currentStatus.Playing)
        {
          _spotify.Previous();
          _spotify.Previous();
        }
        else
        {
          _spotify.Previous();
          _spotify.Play();
        }
      }

      if (parser.GetArgValue<bool>(new string[] { "-next", "n" }))
      {
        if (_currentStatus.Playing)
        {
          _spotify.Skip();
        }
        else
        {
          _spotify.Skip();
          _spotify.Play();
        }
      }

      var e = parser.GetArgValue<string>(new string[] { "-export", "e" });
      if (!String.IsNullOrEmpty(e))
      {
        try
        {
          var path = Path.GetFullPath(e);
          if (_currentTrack != null)
          {
            GetAlbumArt().Wait();
            if (_currentArt != null)
            {
              _currentArt.Save(path + "album.jpg", ImageFormat.Jpeg);
            }
          }
        }
        catch (NotSupportedException)
        {
          Console.WriteLine("Path not supproted.");
        }
      }

      var v = parser.GetArgValue<int>(new string[] { "v", "-volume" }, -1);
      if (v >= 0 && v <= 100)
      {
        _spotify.SetSpotifyVolume(v);
      }

      var u = parser.GetArgValue<string>(new string[] { "u", "-url"});
      if (!String.IsNullOrEmpty(u))
      {
        _spotify.PlayURL(u);
      }

      if (parser.GetArgValue<bool>(new string[] { "h", "-help" }))
      {
        ShowHelp();
      }

      var g = parser.GetArgValue<string>(new string[] { "g", "-goto" }, string.Empty);
      if (!String.IsNullOrEmpty(g))
      {
        switch (g.ToLower())
        {
          case "a":
          case "artist":
            _spotify.PlayURL(_currentTrack.ArtistResource?.Uri);
            break;

          case "b":
          case "album":
            _spotify.PlayURL(_currentTrack.AlbumResource?.Uri);
            break;
        }
      }

      //Status should be last
      if (
        args.Length == 0 ||
        parser.GetArgValue<bool>(new string[] { "s", "-status" }))
      {
        string STATUS = "";
        string CURRENT_TRACK = "No track";
        string CURRENT_ARTIST = "";
        const int TICKS_PER_SECOND = 10000000;

        RefreshStatus();

        if (_currentTrack != null)
        {
          CURRENT_TRACK = _currentTrack.TrackResource?.Name;
          CURRENT_ARTIST = _currentTrack.ArtistResource?.Name;
        }

        if (_currentStatus.Playing)
        {
          TimeSpan p = new TimeSpan((long)(_currentStatus.PlayingPosition * TICKS_PER_SECOND));
          TimeSpan l = new TimeSpan((long)(_currentTrack.Length * TICKS_PER_SECOND));
          if (l > new TimeSpan(1, 0, 0))
          {
            STATUS = $"[PLAYING@{p.ToString(@"hh\:mm\:ss")}\\{l.ToString(@"hh\:mm\:ss")}]";
          }
          else
          {
            STATUS = $"[PLAYING@{p.ToString(@"mm\:ss")}\\{l.ToString(@"mm\:ss")}]";
          }
        }
        else
        {
          STATUS = "[STOPPED]";
        }

        Console.WriteLine($"{CURRENT_TRACK} - {CURRENT_ARTIST} {STATUS}");
      }

    }

    private static void ShowHelp()
    {
      var h = @"Spotify cli interface. Requires Spotify to be running locally.
usage: spot [options]
  options:
    -pp, --pause        Pauses or unpauses Spotify
    -p, --prev          Previous track
    -n, --next          Plays next track (skip)
    -v, --volume        Sets Spotify volume (Windows7+)
    -u, --url <url>     Plays Spotify url
    -h, --help          Shows help
    -g, --goto <a|b>    Goto (a) Artist, (b) alBum
    -s, --status        Shows current Spotify status, also if no arguments are entered
    -e, --export <path> Exports current album art to specified folder as album.jpg
";

      Console.WriteLine(h);
    }
  }
}
