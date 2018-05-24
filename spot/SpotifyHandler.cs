using SpotifyAPI;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spot
{
	public class SpotifyHandler
	{
		private bool isConnected;

		private SpotifyLocalAPIConfig _config;
		private SpotifyLocalAPI _spotify;
		private Track _currentTrack;
		private StatusResponse _currentStatus;
		private Bitmap _currentArt = null;

		public bool IsConnected { get => isConnected; set => isConnected = value; }

		private bool Connect()
		{
			if (!SpotifyLocalAPI.IsSpotifyRunning())
			{
				Console.WriteLine(@"Spotify isn't running!");
				return false;
			}
			if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
			{
				Console.WriteLine(@"SpotifyWebHelper isn't running!");
				return false;
			}

			bool successful = _spotify.Connect();
			if (successful)
			{
				//Console.WriteLine(@"Connection to Spotify successful");
				return true;
			}
			else
			{
				Console.WriteLine(@"Couldn't connect to the spotify client. Retry (Y/n)");
				var res = Console.ReadKey();
				if (res.KeyChar == 'Y' || res.KeyChar == 'y')
					Connect();
			}
			return true;
		}

		internal void RefreshStatus()
		{
			_currentStatus = _spotify.GetStatus();
			if (_currentStatus != null)
			{
				_currentTrack = _currentStatus.Track;
			}
		}

		internal void PlayPause()
		{
			if (_currentStatus.Playing)
			{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				_spotify.Pause();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
			else
			{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				_spotify.Play();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

		internal void PlayURL(string u)
		{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			_spotify.PlayURL(u);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		internal void SetSpotifyVolume(int v)
		{
			_spotify.SetSpotifyVolume(v);
		}

		internal async Task GetAlbumArt()
		{
			var art = _currentTrack.AlbumResource != null ? await _currentTrack.GetAlbumArtAsync(AlbumArtSize.Size640, _config.ProxyConfig) : null;
			var art2 = new Bitmap(art);
			_currentArt = art2;
		}

		internal void Previous()
		{
			if (_currentStatus.Playing)
			{
				_spotify.Previous();
				_spotify.Previous();
			}
			else
			{
				_spotify.Previous();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				_spotify.Play();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

		internal void Next()
		{
			if (_currentStatus != null && _currentStatus.Playing)
			{
				_spotify.Skip();
			}
			else
			{
				_spotify.Skip();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				_spotify.Play();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

		internal void ExportAlbumArt(string e)
		{
			try
			{
				var path = Path.GetFullPath(e);
				if (_currentTrack != null)
				{
					GetAlbumArt().Wait();
					if (_currentArt != null)
					{
						_currentArt.Save(path + "\\album.jpg", ImageFormat.Jpeg);
					}
				}
			}
			catch (NotSupportedException)
			{
				Console.WriteLine("Path not supported.");
			}
		}

		internal void GoToUrl(string g)
		{
			switch (g.ToLower())
			{
				case "a":
				case "artist":
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					_spotify.PlayURL(_currentTrack.ArtistResource?.Uri);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					break;

				case "b":
				case "album":
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					_spotify.PlayURL(_currentTrack.AlbumResource?.Uri);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					break;
			}
		}
		internal void WriteStatus()
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

		public SpotifyHandler()
		{
			_config = new SpotifyLocalAPIConfig
			{
				ProxyConfig = new ProxyConfig()
			};

			_spotify = new SpotifyLocalAPI(_config);

			isConnected = Connect();
		}
	}
}
