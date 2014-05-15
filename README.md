anthologizer
============

Music Anthologizer and player for Mega Collections of Digitized Music.  Windows WCF server with Android and .NET clients.
Includes a decent randomizer able to play random selections quickly over huge collections, a downloader for Android, 
logic for playing local copies, smarts to figure out which tracks have been added to anthologies and which have not. 

Philosophy

I have mor emusic than I can ever listen to if I tried to work trough every track.  Not a problem, sicne I don't want to listen to every track, I only like some of it, but that changes over time as I discover new stuff and things I already had but didn't appreciate.  So I make anthologies from my archive, which is now about 1.3 terabytes and growing.  Each anthology is about 100 tracks, which I have found by experience is the right length for me.  

My general collection is organized tolerably well by folder.  The accuracy of the embedded tags inside mp3 files is hit and miss, and I prefer to browse using the folder structure.  Tools which force you to browse by tag have a special place in hell reserved for them (are you listening, Apple!) So I wanted a tool that lets me browse my collection remotely from (say) a mobile device, lets me play tracks, and then in one clik lets me add them to a current anthology.  I want the resultign anthology to contain copies of the tracks and not references.  The marginal cost of disk storage is now effectively zero, and that way all other tools can use the anthoogy just fine, they are just tracks like any other album.

Architecture

There is a server-side webservice, which lives on the machine that holds your music collection tracks.  You point the webservice at the root of your collection, and it simpyl follows the folder structure when you navigate it using a client.  When you see the tracks, it uses the tags inside those tracks if they are good, otherwise it uses the filename. 

There are currently two client.  A .NET client for windows use, and an Android client for mobile use.  The Android client is better in most respects, but the .Net client has some advantges of its own.  I use them both.  You need to tell the client which server to connect to, and multiple servers are supported.  

The Android client also support playing locally held music files and the mobiel itself, which shows up as a "server" in the list.  You can download whole albums/anthologies from the server to your mobiel in one click.  When you are browsing the server collection, the app is smart enough to know that it shoudl play the local copy of the file, otherwise ti streams it from the server.  It uses the Android media player, so the performance vries when streaming over slow network links.  Perhaps in the future this will be replaced or improved, but it has been good enough so far.