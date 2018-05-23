# spot

Spotify cli interface. Requires Spotify to be running locally.  
usage: spot [options]  

options:  
    -pp, --pause          Pauses or unpauses Spotify  
    -p, --prev            Previous track  
    -n, --next            Plays next track (skip)  
    -v, --volume <0-100>  Sets Spotify volume (Windows7+)  
    -u, --url <url>       Plays Spotify url  
    -h, --help            Shows help  
    -g, --goto <a|b>      Goto (a) Artist, (b) alBum  
    -s, --status          Shows current Spotify status, also if no arguments are entered  
    -e, --export <path>   Exports current album art to specified folder as album.jpg  

##Acknowledgments

Uses https://github.com/JohnnyCrazy/SpotifyAPI-NET.