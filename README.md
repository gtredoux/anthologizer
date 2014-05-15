anthologizer
============

Music Anthologizer and player for Mega Collections of Digitized Music.  Windows WCF server with Android and .NET clients.
Includes a decent randomizer able to play random selections quickly over huge collections, a downloader for Android, 
logic for playing local copies, smarts to figure out which tracks have been added to anthologies and which have not. 

Philosophy

I have more music than I can ever listen to if I tried to work trough every track.  Not a problem, sicne I don't want to listen to every track, I only like some of it, but that changes over time as I discover new stuff and things I already had but didn't appreciate.  So I make anthologies from my archive, which is now about 1.3 terabytes and growing.  Each anthology is about 100 tracks, which I have found by experience is the right length for me, but you can make yours as large as you like.  

My general collection is organized tolerably well by folder.  The accuracy of the embedded tags inside mp3 files is hit and miss, and I prefer to browse using the folder structure.  Tools which force you to browse by tag have a special place in hell reserved for them (are you listening, Apple!) So I wanted a tool that lets me browse my collection remotely from (say) a mobile device, lets me play tracks, and then in one click lets me add them to a current anthology on the server.  I want the resulting anthology to contain copies of the tracks and not references.  The marginal cost of disk storage is now effectively zero, and that way all other tools can use the anthoogy just fine, they are just tracks like any other album.

An important need is to know whih tracks have previously been anthologized when you are browsing you collection yo cherry pick from the mass of material.  Then you can have more confidence that you are not revisiting material over and over again, and choose new stuff to try out in your anthology.  The system does this by calculating digests to identify tracks regarless of their filename or location.  SO copies of the same file will have the same digest, and then copies in the Anthologies can be traced back to the files they were copied from.  There is no attempt here to idenfity the same song in different digital version, the slightest byte difference will mean it is treated as a distinct track.  Thsi has proved to be a very useful feature in compiling the anthologies.

Architecture

There is a server-side webservice, which lives on the machine that holds your music collection tracks.  You point the webservice at the root of your collection, and it simply follows the folder structure when you navigate it using a client.  When you see the tracks, it uses the tags inside those tracks if they are good, otherwise it uses the filename. There is also an Indexer app you use to index your collection in advance, generally you only have to do that once.

There are currently two clients.  A .NET client for Windows use, and an Android client for mobile use.  The Android client is better in most respects, but the .Net client has some advantages of its own.  I use them both.  You need to tell the client which server to connect to, and multiple servers are supported.  

The Android client also supports playing locally held music files on the mobile itself, which shows up as a "server" in the list.  You can download whole albums/anthologies from the server to your mobile in one click.  When you are browsing the server collection, the app is smart enough to know that it should play the local copy of the file, otherwise it streams it from the server.  It uses the Android media player, so the performance varies when streaming over slow network links.  Perhaps in the future this will be replaced or improved, but it has been good enough so far.

Setup

You will need a database on the server and IIS.  I use SQLEXPRESS and it works fine.  Your webservice will need permission to write to the database, so you can set that up any way you want, say by creating a special user for the purpose and running the webservice as that user, then giving the account permission to write to the database.  The service also needs to eb able to write to the Anthologies folder in your music collection, since it copies files back and forth there, so give it that permission.

The webservice has a web.config file with important settings determining where your music collection lives.  It is best to place your Anthologies folder under the root of that collection.  In the future this might be more flexible but right now you can't really have it anywhere else.  My own collection is organized like so

k:\mp3
k:\mp3\Anthologies
k:\mp3\Blues
k:\mp3\Jazz

etc.

You will also need to create a virtual directory in IIS to serve the music from when the music is played by a player.  The webservice simply forms URLs pointing at the virtual directory, for which there is a config line in web.config.  I use say "Music" as a virtual directory for K:\mp3





