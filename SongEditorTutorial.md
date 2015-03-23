# Introduction #

Since public release, WGiBeat has always had the ability to load user-created songs (since WGiBeat did not include any songs with it, this was a requirement). Earlier versions of WGiBeat required players to create song files manually by copying the relevant audio file into a folder, and creating a .sng file using a text editor. Although this is still possible, the introduction of the built-in song editor WGiEdit in v0.5 made adding and editing songs much easier. Later versions have added more functionality and fixes to WGiEdit.

Before explaining the use of WGiEdit, it is necessary to explain how WGiBeat itself works with songs. In order for WGiBeat to use any song, it needs three pieces of information:

  * What is the BPM (tempo) of the song?
  * When (relative to the song) should the gameplay begin?
  * When (relative to the song) should the gameplay end?
  * And optionally: Should the song be played from the beginning, or does it contain a long introduction that should be skipped partially?

These four pieces of information need to be provided by the player. They are referred to as the "BPM", "Offset", "Length", and "AudioStart" respectively. Other information, such as the title and artist of the song also need to be provided (so that the song displays correctly in the Song Select screen) but have minimal effect on gameplay.


![http://wgibeat.googlecode.com/svn/wiki/SongPartitionDiagram.png](http://wgibeat.googlecode.com/svn/wiki/SongPartitionDiagram.png)

In the above diagram, the song's audio is divided into four sections:
  * The audio in Section A occurs before the AudioStart point and is skipped by WGiBeat. Note that the AudioStart point can be zero (at the start of the song), meaning that no part of the song is skipped.
  * The audio in Section B occurs between the AudioStart and Offset points (hence the offset **must** be greater than the AudioStart point). This represents the audio that is played before the first beatline of the song (this is when the "Ready" message and countdown are displayed). It is suggested that this section be between 3 and 12 seconds long.
  * The audio in Section C represents the playable portion of the song, between the Offset and Length points. Beatlines are generated in this portion. Their frequency is determined by the song's BPM.
  * The audio in Section D represents the audio that is played after the song is considered completed. Since the gameplay has ended, no beatlines appear beyond this point, but the audio will continue playing (typically into the evaluation screen).

# What is needed #

In order to create a new song is WGiBeat, only one thing is needed, the audio file. This file can be an .mp3, .ogg, .wma or .wav file, and does not need to be in WGiBeat's Songs folder to be usable. WGiEdit allows users to create song files from scratch - and handles the moving process automatically.

# Starting the Editor #

WGiEdit can be started by selecting the 'Song Editor' option from the Main Menu. WGiEdit is only available in newer versions of WGiBeat. WGiEdit can be used to create a new song from scratch, edit an existing one, convert a .sm or .dwi song to .sng, or delete a song. A guide for creating a new song is provided below.

# Guide #
## Step 1 ##

First, select 'Create New Song' from the WGiEdit main menu. It will then ask for three pieces of information. The source file, the destination file, and destination folder. The source file is the audio that should be used for the new game song. When selecting this option, a file browser is displayed. Navigate to the audio file you want to use by using the controls displayed on screen, and press START. The destination file is the name of the .sng file that will be used to hold the gameplay information of this song (such as the Offset, BPM and Length, as discussed earlier). Simply type in the name of this file. The file name has no effect on gameplay. Finally, the destination folder is the name of the folder that will be created inside WGiBeat's Songs folder to hold both the source file and destination file. Once these three pieces of information are provided, select 'Next Step'. The destination folder will be created, the destination file will be created inside it, and the source file will be copied to this folder if necessary.

## Step 2 ##

![http://wgibeat.googlecode.com/svn/wiki/WGiEditStep2.png](http://wgibeat.googlecode.com/svn/wiki/WGiEditStep2.png)

During this step, the details of the song are requested. The title, subtitle and artist of the song are read from the audio file's metadata if possible. Either way, it can be entered by selecting the appropriate options. For BPM, AudioStart, Offset and Length, these values can either be entered manually (if you know them), or measured. In addition, the optional AudioStart and Background fields can be filled in as well. Use the instructions on screen when using a measurement tool. Whether a song is valid or not is displayed on the right side of the screen, along with a reason why the song is not valid, if appropriate. When the song is considered valid, the 'Next Step' option will become available.

## Step 3 ##

![http://wgibeat.googlecode.com/svn/wiki/WGiEditStep3.png](http://wgibeat.googlecode.com/svn/wiki/WGiEditStep3.png)

During this step, all of the information needed for a song to be playable has already been provided. This step is used to fine-tune the information given in the previous step - particularly the AudioStart, Offset, BPM and Length fields. To do this, the song gameplay is simulated using a beatline. The song is then played and beatline notes are shown as they would appear during actual gameplay. At any time, the BPM, Offset and Length can be adjusted by using the controls detailed at the bottom of this screen. It is also possible to increase the beatline speed for additional accuracy, or to restart the song.

To test whether the information given is accurate, try to hit the beatline notes using the BEATLINE key, and observe whether these appear to be hit late or early. The timing of each hit relative to the song will also be displayed, as well as the average timing. Use these timings to adjust the given information appropriately.

## Step 4 ##

Once the previous three steps are complete, the song creation process is complete. The song will then be playable by selecting it from the Song Select Screen.

# Editing an existing song #

A similar process is followed for editing an existing song, or importing a song from a different format, except that Step 1 is skipped when loading an existing .sng song. When importing from a different format, Step 1 is still used to specify the source file, destination file and destination folder. The import option is designed to allow for easy importing of a single desired .sm or .dwi song. For converting an entire batch of songs at once, consider using the "Convert songs to .sng" option instead.