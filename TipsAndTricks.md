# Introduction #

This page contains some of the less obvious features of WGiBeat. Most players will benefit from knowing this information. More will be added as features are implemented or updated.

## Drop in/Drop out ##
Since v0.7, it is possible for players to join or leave without having to return to the Main Menu to do so. It can be done from any screen that displays the "Press START To Join" message.

![http://wgibeat.googlecode.com/svn/wiki/JoinInIndicator.png](http://wgibeat.googlecode.com/svn/wiki/JoinInIndicator.png)

Simply press START to join in. To leave, hold the SELECT key and press START.

## Blazing Mode ##
Blazing mode is a gameplay feature available in most game modes that can earn players huge amounts of points while it is active. It is, however, a risky mode to activate, as it makes arrows more difficult to hit successfully.

To activate Blazing Mode, a player must have more than 100 life. It can then be activated at any time during a song by pressing the SELECT key. While active, the player's life will gradually drain. Once the player's life drops below 100, Blazing Mode deactivates automatically. The player's life bar will glow while in Blazing Mode.

While in Blazing Mode, some of the arrows will be red. In order to hit these arrows, the player must press the opposite direction (a red left arrow is hit by pressing RIGHT). Red arrows are worth significantly more points than normal arrows.

![http://wgibeat.googlecode.com/svn/wiki/BlazingMode.png](http://wgibeat.googlecode.com/svn/wiki/BlazingMode.png)

In Co-op mode, players share life, and as such they also share the ability to use Blazing Mode. Each player has the ability to activate Blazing Mode individually, which costs shared life. When multiple players use Blazing Mode simultaneously in this mode, an additional 'unison multiplier' is awarded to all points earned. Players must have an average above 100 life to use Blazing Mode in Coop mode and it will remain active until this is no longer the case.

NOTE: Blazing Mode cannot be used in VS CPU mode, as the CPU does not use this mode.
NOTE: Since Blazing Mode requires more than 100 life to use, it is only available to players with a profile with level 2 or higher.

## Changing difficulty or beatline speed easily ##
The beatline speed and difficulty for each player is displayed on many screens at the bottom of the screen, along with the associated profile name (if any).

![http://wgibeat.googlecode.com/svn/wiki/LabelledPOF.png](http://wgibeat.googlecode.com/svn/wiki/LabelledPOF.png)

When this frame is visible, press and hold the SELECT key. The current difficulty and beatline speed will then be displayed as text next to the icons. While holding the SELECT key, press LEFT or RIGHT to change your difficulty, or UP and DOWN to change your beatline speed.

![http://wgibeat.googlecode.com/svn/wiki/POFAdjustMode.png](http://wgibeat.googlecode.com/svn/wiki/POFAdjustMode.png)

This can be done from any screen that displays the Player Options Frame, including the Song Select and Mode Select screens.

## Changing the song sort order ##
The song sort order is displayed near the top right corner of the song select screen. By default, songs are sorted alphabetically by title. To change this, hold the BEATLINE key, and press left or right. Any player can do this. The song list can be sorted by Title, Artist, BPM or Length (in Transcendence Alpha 3 or later).

![http://wgibeat.googlecode.com/svn/wiki/SongSortAdjustMode.png](http://wgibeat.googlecode.com/svn/wiki/SongSortAdjustMode.png)

## Song List Quick Navigation ##
WGiBeat v0.7 has introduced a new feature to allow for quicker song list navigation. It is used in combination with the song sort order. When the sort is set to title or artist, pressing and holding down the BEATLINE key will display a list of letters. Press UP or DOWN to select a letter from this list, the song list will jump to the first song that begins with the selected letter. Songs that begin with numbers are represented by the '0-9' item, and songs that begin with symbols are represented by the 'Sym' item.

When the song sort order is set to BPM, a list of common BPM's will be displayed in a similar fashion. Selecting one will cause the song list to jump to the first song that has a BPM equal to or faster than the selected BPM.

## Adding Menu Music ##
WGiBeat can play any provided audio as menu music during the game's various screens. Since v1.0, menu music is included with all installations of the game. To add your own menu music, edit the MusicList.txt file inside the MenuMusic folder. This file defines which audio file should be played for each screen. The format of this file is as follows:

```
#MUSICLIST-1.0;
ScreenName=FileName;
```
Where 'FileName' is the name of the audio file that should be used, and 'ScreenName' is the name of the screen where this audio file should play. A list of valid screen names is given in the default MusicList.txt file.

For example, to have "MyBackgroundMusic.mp3" play during the Main Menu Screen, use this line:
```
MainMenu=MyBackgroundMusic.mp3;
```
Similar to game songs, .mp3, .ogg, .wma or .wav files can be used for menu music. The same audio can be used for any number of screens.

## Adding Menu Sound Effects ##
WGiBeat can also play any provided audio as sound effects during the game's various screens, since v0.9. Since v1.0, menu sounds are included in all installations of the game. To add your own sound effects, edit the MenuSounds.txt file inside the Content\SoundEffects\Default folder. This file defines which audio file should be played for each event. The format of this file is as follows:

```
#MENUSOUNDS-1.0;
ActionName=FileName;
```
Where 'FileName' is the name of the audio file that should be used, and 'ActionName' is the name of the event when this audio file should play. Valid action names are given in the default MenuSounds.txt file. These are not case sensitive.

For example, to have WGiBeat play 'sound.ogg' every time the user picks a main menu option, use this line:
```
MainMenuDecide=sound.ogg;
```
Similar to menu music, .mp3, .ogg, .wma or .wav files can be used for menu sound effects. The same audio can be used for any number of sound effects.

## Profile System ##
WGiBeat supports a profile system for returning players. Profiles can be used to store a player's preferred gameplay options (such as Beatline speed and difficulty), and are also used to keep gameplay stats.

![http://wgibeat.googlecode.com/svn/wiki/ProfileSelect.png](http://wgibeat.googlecode.com/svn/wiki/ProfileSelect.png)

To create a profile, select the "Create New" option after selecting the "Start Game" option from the main menu. A keyboard is then displayed to facilitate name entry. Once created, a profile can be selected from the profile list shown on this screen. Profiles are completely optional for gameplay - and each player can use a profile if desired.

In addition to the above, players are also awarded with EXP when playing with a profile. When enough EXP is earned, the player's profile level increases, which provides certain benefits, such as increased maximum life. A player's profile level is displayed in the player options frame, located at the bottom of the screen.

![http://wgibeat.googlecode.com/svn/wiki/ProfileDisplay.png](http://wgibeat.googlecode.com/svn/wiki/ProfileDisplay.png)

Finally, WGiBeat also records gameplay stats for players that use profiles. To view these gameplay stats, select the "Stats" option from the main menu.

## Song Debugging ##
Song debugging is a useful feature that allows players to change the offset, BPM or length of a song on-the-fly while it is being played. This is most useful to fine-tune a song that is not synchronized properly. By default, song debugging is disabled, and must be enabled from the options menu by setting "Song Debugging" to "On".

Once enabled, the song's current BPM, length, offset and phrase number (position) will be displayed in the middle frame of the screen during gameplay.

![http://wgibeat.googlecode.com/svn/wiki/SongDebugDisplay.png](http://wgibeat.googlecode.com/svn/wiki/SongDebugDisplay.png)

The following keys can be used to adjust the BPM, length or offset of the song. These are the same as the keys used during the "Tuning" step in WGiEdit. Changes are saved once the song gameplay is completed (when the evaluation screen is displayed).

  * F3 - Decrease BPM by 0.01
  * F4 - Increase BPM by 0.01
  * F5 - Decrease BPM by 0.1
  * F6 - Increase BPM by 0.1
  * F7 - Decrease Offset by 0.1
  * F8 - Increase Offset by 0.1
  * F9 - Decrease Offset by 0.01
  * F10 - Increase Offset by 0.01
  * F11 - Decrease Length by 0.1
  * F12 - Increase Length by 0.1

IMPORTANT: Once changes are made to a song's BPM, length or offset, all of the song's high scores will be invalidated. This is by design to avoid cheating.

## Fail, Fault or Miss? ##
WGiBeat has three different types of player mistakes.

  * If a player presses the wrong arrow key, this counts as a fault. A fault breaks the player's hit counter, resets the player's progress towards completing a set or arrow hits, and costs a small amount of life.
  * If a player ignores a beatline, this counts as a miss. A miss costs life, but does not reset a player's arrow set progress, hit counter or beatline streak.
  * If a player attempts to hit a beatline before hitting all shown arrows, this counts as a Fail. A fail costs life (depending on the number of incomplete arrows) and resets a player's arrow progress and beatline streak.

Normally, when having to choose between the three, it is better to hit arrows slower but more accurately, and accepting a miss. This gives the player extra time to complete the arrow set, before the next beatline. Note that amount of damage dealt by the three types of mistakes is higher for higher difficulty levels.

## Multiple song folders ##
WGiBeat supports loading songs from multiple folders. By default, songs are loaded from the "Songs" subfolder. This can be changed by editing the settings.txt file. To load from multiple folders, separate them with a "|". For example, to load songs from the "Songs" subfolder as well as from "D:\Stepmania\Songs", change the SongFolder line to this:

`SongFolder=Songs|D:\Stepmania\Songs;`

## Handling .sm and .dwi files ##
Since WGiBeat can load .sm and .dwi files from multiple folders, it is possible to load them directly from an existing Stepmania installation. To do this, add the Stepmania songs folder to the SongFolder setting as shown above. Since WGiBeat needs to modify these files for them to work correctly, two options are provided, which affect how these files are treated.

The first option, "Allow Problematic Songs", forces WGiBeat to load any .sm or .dwi file, even if they are known to be problematic - such as if they contain negative BPMs. This feature is useful to bypass the restrictions normally placed on these songs by WGiBeat, although it is likely that such songs will not work correctly.

The second option, "Convert Files to .sng", will convert any song found in .sm or .dwi format to the native .sng format. The original .sm and .dwi files are not modified, and are ignored if a corresponding .sng file exists. If this option is disabled, then all .sm and .dwi songs are loaded in Read Only mode, and no lasting changes can be made to them using song debugging (since doing so would cause these songs to play incorrectly in Stepmania). This option must be enabled to allow such changes to be persisted, while preserving them for other programs such as Stepmania.

Both of the above options are available from the Options Menu.