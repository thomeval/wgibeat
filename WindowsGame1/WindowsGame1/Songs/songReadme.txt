Song files go in this folder. They can also be placed in separate subfolders.

Each song needs a .sng file to be playable in the game. The format of the .sng file is as follows:

#SONG-1.0;
ARTIST=Artist goes here;
TITLE=Song Title goes here;
SUBTITLE=(Optional) Second title line goes here;
BPM=BPM of song goes here;
OFFSET=The starting point offset, in seconds, goes here;
LENGTH=The length of the song, in seconds goes here;
AUDIOFILE=The name of the song file (such as mysong.mp3) goes here.;
AUDIOSTART=(Optional) The starting position of audio playback goes here.;
BACKGROUND=(Optional) The name of the background image (such as mybackground.png) goes here.;

Note that each line MUST end in a semicolon, and the file must start with #SONG-1.0;

Place the song file (can be .mp3, .wma or .ogg) and its .sng file in the same folder. Multiple songs can be placed in the same folder.

Once the .sng file is created, the BPM, offset and length attributes (necessary to properly synchronize the game with the music) can be
adjusted on the fly from inside WGiBeat itself. To do this, enable "Song Debugging" from the options menu. Then, when playing a song,
use the following keys to fine-tune the song:

F5 - Decrease BPM by 0.1
F6 - Increase BPM by 0.1
F7 - Decrease Offset by 0.1
F8 - Increase Offset by 0.1
F9 - Decrease Offset by 0.01
F10 - Increase Offset by 0.01
F11 - Decrease Length by 0.1 (note that this, together with BPM and Offset determine the position of the last note in a song)
F12 - Increase Length by 0.1

It is also possible to use the built in song editor (WGiEdit) to create and edit song files.

IMPORTANT: Any changes are saved to the song once it is finished playing. If any changes are made, the identity of the song will also
change, which will reset the highscores of that song. This is by design.

The .sng format also supports BPM changes and stops, but these can only be used with version 1.1 of the .sng format. The only changes are as follows:

1) Instead of starting with #SONG-1.0; start with #SONG-1.1;

2) Instead of defining a single BPM as in version 1.0, BPMs are defined as follows:
BPM=Phrase:Amount,Phrase:Amount,Phrase:Amount;
Where 'Phrase' is the phrase when the BPM comes into effect, and Amount is the BPM to use at that point in the song. Phrase 0 marks the first beatline of the song,
and beatlines are spaced 1 phrase apart. A BPM should *always* be defined at phrase 0. Both the Phrase and Amount can be decimal.
Example:
Bpm=0:64,9:128;
This will start the song with 64 BPM, and change it to 128 at phrase 9.

3) Similarly, for stops, use the STOPS field, like this:
STOPS=Phrase:Amount,Phrase:Amount,Phrase:Amount;
A song can have any number of stops.

Alternatively, .sm or .dwi files can be used instead.
