=Basics=
In order for a song to be playable in WGiBeat, certain information must be provided. This information is typically stored in a .sng file alongside the song itself, although the information can also be extracted from a .sm or .dwi file if the song is already playable in Stepmania. The format of the .sng file is as follows:

#SONG-1.0;
ARTIST=Artist goes here;
TITLE=Song Title goes here;
SUBTITLE=(Optional) Second title line goes here;
BPM=BPM of song goes here;
OFFSET=The starting point offset, in seconds, goes here;
LENGTH=The length of the song, in seconds goes here;
AUDIOFILE=The name of the song file (such as mysong.mp3) goes here.;
AUDIOFILEMD5=(Optional) The MD5 of the song file goes here.;
AUDIOSTART=(Optional) The starting position of audio playback goes here.;
BACKGROUND=(Optional) The name of the background image (such as mybackground.png) goes here.;

Note that each line *must* end in a semicolon, and the file must start with #SONG-1.0; or #SONG-1-1; The 1.1 .sng format was introduced in version 0.75 to support BPM changes and Stops. Either format can be used for songs that have no BPM changes or Stops.

Place the song file (can be .mp3, .wma or .ogg) and its .sng file in the same folder. Multiple songs can be placed in the same folder.

The easiest way to create a .sng file is to use the built-in song editor, WGiEdit. It will also calculate the MD5 automatically. See the SongEditorTutorial wiki page for more information.

Once the .sng file is created, the BPM, offset and length attributes (necessary to properly synchronize the game with the music) can be
adjusted on the fly from inside WGiBeat itself. See the TipsAndTricks wiki page for more information.

IMPORTANT: Any changes are saved to the song once it is finished playing. If any changes are made, the identity of the song will also
change, which will reset the high scores of that song. This is by design.

==Version 1.1 Changes==
The three changes are as follows:

1) Instead of starting with #SONG-1.0; start with #SONG-1.1;

2) Instead of defining a single BPM as in version 1.0, BPMs are defined as follows:

{{{
BPM=Phrase:Amount,Phrase:Amount,Phrase:Amount;
}}}
Where 'Phrase' is the phrase when the BPM comes into effect, and 'Amount' is the BPM to use at that point in the song. Phrase 0 marks the first beatline of the song,
and beatlines are spaced 1 phrase apart. A BPM should *always* be defined at phrase 0. Both the Phrase and Amount can be decimal.

Example:

{{{
Bpm=0:64,9:128;
}}}
This will start the song with 64 BPM, and change it to 128 at phrase 9.

3) Similarly, for stops, use the STOPS field, like this:
{{{
STOPS=Phrase:Amount,Phrase:Amount,Phrase:Amount;
}}}
Where 'Phrase' is the phrase when the Stop should occur, and 'Amount' is the duration of the Stop, in decimal seconds. A song can have any number of BPM changes and Stops.

=Examples=
==Version 1.0 Format==

#SONG-1.0;
Title=IM BROKEN;
Subtitle=(On the way back home mix);
Artist=George Ellinas;
Bpm=130;
Offset=29.76;
AudioStart=20;
Length=213.1;
AudioFile=George_Ellinas_-_I_M_BROKEN_(On_the_way_back_home_mix).mp3;
AudioFileMD5=1E3667C4E6E12FAB69626D9B1F53C67F;


==Version 1.1 Format==

#SONG-1.1;
Title=video out e;
Subtitle=;
Artist=Vospi;
Bpm=0:218,16:54.5,20:218,52:54.5,60:218,109:109;
Offset=10.44;
AudioStart=0;
Length=185.9;
AudioFile=Vospi - video out e.mp3;
AudioFileMD5=B311E63BD67F3EEACE5DF2547B387D28;
Stops=60:1.079;
