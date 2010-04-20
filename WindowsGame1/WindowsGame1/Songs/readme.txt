Song files go in this folder. They can also be placed in separate subfolders.

Each song needs a .sng file to be playable in the game. The format of the .sng file is as follows:

#SONG-1.0;
ARTIST=Artist goes here;
TITLE=Song Title goes here;
BPM=BPM of song goes here;
OFFSET=The starting point offset, in seconds, goes here;
LENGTH=The length of the song, in seconds goes here.;
SONGFILE=The name of the song file (such as mysong.mp3) goes here.;

Note that each line MUST end in a semicolon, and the file must start with #SONG-1.0;

Place the song file (can be .mp3 or .ogg) and its .sng file in the same folder.