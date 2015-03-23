# Version History #

> ## 2.0 Alpha 3 ([r654](https://code.google.com/p/wgibeat/source/detail?r=654)) "Inappropriate Allocation" ##

  * Resolution upgrade! WGiBeat is now designed for 16:9 aspect ratio, and supports up to 1920x1080 resolution! Previous 4:3 resolutions are still available.
  * The resolution can now be changed again in the Options Screen.
  * A new look for overcharge in normal and coop lifebars.
  * Alt + Enter now works to toggle full screen mode (Thanks eXeCutor18, MarkStockwell)
  * Added feature to add or remove arbitrary beatline notes in a song, using the AddNotes and RemoveNotes fields.
  * Added highly experimental Super Beatline Notes. Twice as many arrows must be hit before these notes. This uses the SuperNotes field.
  * Arrow sequence progress bars are now animated.
  * Tweaked groove momentum and ideal streak scoring.
  * Added the Song Timeline to the Main Game Screen to show the progress of the current song.
  * Blazing Bass Boost is now dynamic based on the number of players blazing, and their average life.
  * The number of reverse arrows is now more dynamic based on the player's current life. There can now be up to 75% reverse arrows per beatline.
  * New sound events for MENU\_INVALID\_DECIDE, PLAYER\_JOIN and PLAYER\_LEAVE.
  * Many graphical items can now be resized by editing the Metrics file (in addition to their position). This is still a work in progress.
  * Added ability to sort by song length in the Song Select Screen.
  * Added a level up indicator and menu sound on the Evaluation Screen.
  * BPM Meter now displays a warning icon for very fast songs.
  * Profiles now record the highest ever ideal streak and hit chains per player. It can be viewed in the Stats Screen.
  * Bugfix: Fixed Numpad . not working for text entry in WGiEdit (Thanks MarkStockwell)
  * Bugfix: Song Offsets must now be at least 1 second to be considered valid. (Thanks MarkStockwell)
  * Bugfix: Fixed sound effects triggering incorrectly on menus (such as LEFT/RIGHT when a menu item doesn't have options).
  * Bugfix: Fixed sound effects triggering incorrectly on player options frames (such as when a player leaves).
  * Bugfix: Fixed BPM Meter showing incorrect level in WGiEdit, step 2.
  * Bugfix: Fixed 'jumpy' animation in Coop lifebars when life is lost.
  * Bugfix: Fixed life graph not showing scores and levels correctly for Sync Plus and Sync Pro modes.

## 2.0 Alpha 2 ([r623](https://code.google.com/p/wgibeat/source/detail?r=623)) - "Continued Habit" ##
  * The evaluation screen now displays the overall performance percentage in Normal Mode, the Peak Groove Momentum in Coop mode, and Win/Lose judgements in Team and VS CPU modes.
  * The evaluation screen can now display life, score and level graphs. Cycle using the BEATLINE key.
  * During gameplay, a player's performance can now be tracked in real time against the current high score. A comparison gauge will display when any player is close to the recorded score.
  * Higher difficulty levels no longer start at level 1 at the beginning of a song. Medium will start with 2, Hard with 3, etc.
  * New menu sounds for MAIN\_MENU\_DECIDE and MENU\_DECIDE, which suck slightly less than the previous ones.
  * The particle field background will now react to the beat in the Song Select Screen.
  * Updated the How to Play instructions to reflect the WGiBeat Transcendence layout.
  * Screen header graphics and main menu screen graphics have been updated.
  * A new Credits screen has been added. It is accessible from the Main Menu.
  * Added an indicator next to each song listed in the Song Select Screen that indicates the difficulty of that song's current high score.
  * A new animation has been added to the beatline frames when players activate Blazing Mode.
  * Bugfix: Arrow sequences are now shortened properly in Sync Plus mode after a MISS judgement.
  * Bugfix: Songs created in WGiEdit will now have their high scores saved properly when played immediately after creation.
  * Bugfix: FAIL grades are now awarded correctly in Coop mode.
  * Bugfix: Improved handling of present but blank ID3 tags in WGiEdit.
  * Bugfix: Stats screen will now show total play times above 24 hours correctly.
  * Bugfix: Fixed highscores randomly disappearing, due to a song hashcode bug. Unfortunately, this fix will invalidate existing high scores.

## 2.0 Alpha 1 ([r604](https://code.google.com/p/wgibeat/source/detail?r=604)) - "Able Engineering" ##
  * The UI of the main gameplay screen has been completely redesigned. In addition, Sync Mode has received its own UI redesign.
  * Arrows now scroll as they are completed and are drawn on different height depending on their direction, making sequences much easier to read.
  * Beatlines are now displayed underneath the arrows, instead of the middle of the screen, to make keeping track of both easier.
  * A new animated background / visualization will be drawn in the background during gameplay. Its intensity will grow depending on the current performance of the player. It can also be made more intense or disabled entirely in the Options Screen.
  * A new game mode: Sync Plus mode! This mode is similar to the Sync mode in v1.0, except that players can assist each other to complete arrows. This mode has its own high score table.
  * With the introduction of Sync Plus mode, the existing Sync mode has been renamed to Sync Pro mode.
  * A new 'Blazing Bass Boost' mechanic, which turns up the bass of songs during gameplay while Blazing Mode is active. The intensity can be adjusted in the Options Screen.
  * Several graphics in the game can be scaled arbitrarily by themes in their metrics.txt files. The conversion to this new system is still a work in progress, and will be introduced gradually to the game's graphics elements as development progresses.
  * All line drawing in WGiBeat has been migrated to the [RoundLine library](http://roundline.codeplex.com/), developed by Michael D. Anderson. As a result, the lines drawn in the life graph are much smoother.
  * A MISS judgement now cancels streak.
  * A new animation for Lifebars (both normal and coop) when players gain or lose life.
  * Added an option to disable extra life normally awarded to experienced players, Start Game screen. Only relevant to players that use profiles.
  * Coop Mode now has a larger Groove Momentum gauge that goes to 10x. Sufficiently skilled teams of players will still be able to exceed the bounds of this gauge!
  * Improved spectrum display in the Song Select Screen.
  * The BPM meter has a new blinking animation in the Song Select Screen.
  * Locked songs are now previewed distorted in the Song Select Screen.
  * A new diffculty level, strictly for the most talented players. Can you unlock it?
  * Bugfix: Several bugs in Sync Pro mode have been fixed.

## 1.0 ([r580](https://code.google.com/p/wgibeat/source/detail?r=580)) - "Exceptional Review" ##
  * Red arrows are now more effective at raising Groove Momentum in Coop mode.
  * Slight changes to the Coop lifebar rules. Players can now earn more than their individual maximums if the lifebar has space.
  * CPU Players are now shown with a gray line in the life graph during the evaluation screen, to reduce confusion. (Thanks to XcepticZP for the feedback)
  * The life graph now has a legend for the colours used (Thanks to XcepticZP for the feedback).
  * Note bars now flash red when their player faults or drops a level.
  * Level bars are now more animated.
  * Added a shrinking animation to displayed judgements.
  * Player options frames are now visible on the New Game and Evaluation screens. It is now possible to determine teams and difficulties used during the Evaluation screen as a result.
  * Added a scale/speed indicator to beatlines. This provides a good sense of speed, as well as indicating the beat of the song. Especially useful during Coop mode.
  * The instruction screen now has a fifth page, which explains the factors that affect the difficulty of the game (thanks to Dawkirst for the feedback).
  * The instruction screen now has a real, animated beatline in Page 2.
  * WGiBeat no longer requires V-Sync! It can be switched on or off on the Options Screen.
  * Song Length display now displays the length of the playable section of the song, not the position of the ending marker.
  * Menu sound effects have been created and included with the game.
  * Added an unlock system to songs. Songs can be marked as unlockable by using the RequiredLevel field in the .sng file. This specifies the minimum profile level needed to play the song.
  * WGiEdit: Song backgrounds can now be selected and changed in Step 2.
  * WGiEdit: Several slightly misaligned graphics in Step 2 have been fixed.
  * WGiEdit: Song timeline is now displayed in realtime during the measurement tools.
  * WGiEdit: During measurement, the song can now be rewinded or fast forwarded by 3 seconds by pressing LEFT or RIGHT. The SELECT button no longer rewinds.
  * WGiEdit/Bugfix: An AudioStart later than a song Offset is now considered invalid.
  * WGiEdit/Song Debug: Added keys to change a song's BPM by 0.01 during Step 3 and song debugging. Use F3 and F4 for this.
  * WGiEdit: The playing song can be rewinded and fast fowarded in Step 3.
  * WGiEdit: The time remaining is displayed for a song during Step 3.
  * WGiEdit: The BACK Key (Escape and Xbox Back button) now works in more places in WGiEdit.
  * Bugfix: Menus now display disabled items correctly.
  * Bugfix: More issues relating to cultural hostility (running the game in a non-english region environment) have been resolved (Thanks to SoonDead for the bug report).
  * Bugfix: Fixed long profile names overflowing in the Stats Screen.
  * Bugfix: Fixed graphics glitch with beatline note judgements.

## 0.9 ([r545](https://code.google.com/p/wgibeat/source/detail?r=545)) - "Positive Trend" ##
  * Introducing a brand new game mechanic exclusively for Coop mode: Groove Momentum. The current Groove Momentum is displayed during gameplay and increases or decreases based on player performance. It affects both the points awarded, and beatline speeds.
  * Added support for sound effects for menus. However, you will need to supply your own audio files for this to work. For more information on how to add your own sound effects, see [this page](TipsAndTricks.md).
  * Added an animation to the level bar when full.
  * Added an animation to the score displays when points are awarded.
  * Song Select Screen now remembers the last chosen song played when it is interrupted.
  * Song Select Screen now remembers the last used sort mode.
  * Fixed several issues with deleting song files in WGiEdit. Deleting multiple files is now somewhat easier.
  * Blazing mode now scales depending on the player's current Life.
  * Added a button to WGiBeat's website on the main menu.
  * Beatlines now react gradually to changing speed. This is most evident in the new Coop mode, and in WGiEdit.
  * Note bars will no longer have three or more consecutive arrows with the same direction.
  * Installer now more clearly indicates that WGiBeat needs .NET 3.5 SP1. Replaced outdated Microsoft website link with a new one. (thanks to eXeCutor18 for the feedback)
  * Installer now recommends against installing to Program Files (thanks to eXeCutor18 for the feedback)
  * Bugfix: CPU Players now use the default beatline direction settings.
  * Bugfix: Players that join in after the profile selection screen will now correctly use the default profile options.
  * Bugfix: Beatline notes are no longer slightly misaligned when playing with Right-scrolling beatlines.
  * Bugfix: Lifebars will no longer have visual glitches near 200 life.

## 0.8 ([r518](https://code.google.com/p/wgibeat/source/detail?r=518)) - "Patient Fury" ##
  * A new cooperative game mode is now playable: Sync Mode! Please report any bugs found when playing this mode to the development team.
  * Support for [Beat Up Mania](http://www.stepmania.com/forums/showthread.php?22227-Beat-Up-Mania) simfiles! (It's only logical)
  * Beatlines have recevied several minor updates, such as a background, fading in of notes, and new pulse graphics. They have also been made longer.
  * Added option to have beatlines scroll from left to right, to make them easier to follow for P2 and P4. Any player can enable this individually.
  * Support for song background graphics (.sng and .sm support only - the background must be declared in the definition file to be recognized).
  * Several graphics updates to the main game screen to increase visibility when using song background graphics.
  * WGiBeat no longer freezes temporarily while loading the song selected from the Song Select Screen.
  * When a miss or fault causes a player to drop a level, the number of arrows in the player's current note bar is reduced accordingly. (AKA Mercy Rule).
  * Improvements to the update checker to handle errors more gracefully.
  * Song sort menu now fades in and out correctly.

## 0.75 ([r481](https://code.google.com/p/wgibeat/source/detail?r=481)) - "Functional Mixture" ##

  * BPM Change and Stop support for song files! This includes:
  * Support for BPM changes and stops in .sng, .dwi or .sm format. The .sng format has changed slightly to incorporate these.
  * As a result, many previously broken .sm and .dwi files are now playable. Results will vary depending on how exactly the simfile was authored.
  * Support for saving files in .sng format with Stops and BPM changes.
  * Min and Max BPM is now displayed in the Song select screen.
  * BPM changes and Stops are indicated on Beatlines to warn players.
  * Song and Audio type is now displayed in the Song Select Screen.
  * Better handling of missing textures or metrics definitions.
  * WGiBeat can now automatically check for updates from the internet. This option must be turned on in the Options Menu.
  * Bugfix: BPMs higher than 999 no longer crash the Song Sorter.

## 0.7 ([r443](https://code.google.com/p/wgibeat/source/detail?r=443)) - "Colour-blind Unionist" ##

  * Song total is now displayed in the Song Select screen.
  * Players can now join and leave independently from any screen that has Player Options Frames (mode, team and song select screens). To join if possible, press START. To leave, hold SELECT and press START.
  * EXP and player level system for profiles. Player level and EXP bar is shown on Player Options Frames.
  * Depending on player profile level, a player can have up to 300 maximum life. The normal lifebar has been updated to reflect this.
  * Player starting life is dependent on player profile level.
  * Lifebars now indicate when they are full by having a coloured text background.
  * Coop mode now has overcharge, and a completely new blazing mode system.
  * CPU players now miss beatlines more believably.
  * Momentum increases are now dependent on beatline accuracy (better judgements award more momentum).
  * Player difficulty icons have been updated.
  * Blazing mode now has graphical effects when active (in both Normal and Coop modes).
  * Improved song navigation by using headings similar to those found in RB2 and RB3. Hold the BEATLINE key and press UP or DOWN to use this during Song Select.
  * Percentages are now displayed in the Evaluation Screen.
  * The game window can now be changed to higher resolutions (this does NOT mean better graphics, it only stretches the window's contents). Intended for players that play without full screen mode, but have a large desktop.
  * WGiEdit: Added ability to rewind the song when using a measurement tool.
  * WGiEdit: Added support for AudioStart field.
  * WGiEdit: A song timeline is now displayed in Step 2.
  * WGiEdit: Song validity message now handles multiple lines correctly.
  * WGiEdit: Updated song details display in Step 2.
  * WGiEdit: Added feature to import individual .sm and .dwi files for editing and conversion.
  * Bugfix: Several crashes and songs failing to load due to cultural differences have been resolved (thanks to john\_reactor for helping with this)
  * Bugfix: Duplicate BPM or Stops definitions no longer cause a crash.
  * Bugfix: WGiBeat no longer crashes when pressing an unmapped key or button on certain screens (thanks to eXeCutor18 for the bug report)
  * Bugfix: On Screen Keyboard now in the correct location for P3 and P4.
  * Bugfix: BPM meter now animates correctly, even with VBR audio files.

## 0.65 ([r391](https://code.google.com/p/wgibeat/source/detail?r=391)) - "Executive Dancer" ##
  * Support for .sm and dwi files! Note that the results can vary between perfect and completely unplayable depending on how the particular stepfile was made. Also, there is no support for BPM changes or stops.
  * Option to allow/disallow problematic .sm and .dwi files
  * Song title and artist display in Song Select screen now resize long titles.
  * Small graphics fix to normal life bar.
  * WGiBeat will no longer crash if a song is deleted, then chosen in the Song Select screen. However, it will punish such incidents.
  * Support for multiple song folders defined in settings.txt (separate with '|')
  * Bugfix: Coop lifebar doesn't overflow anymore when full.
  * Bugfix: Main game screen no longer freezes when a song's audio is longer than its length field.

## 0.6 ([r374](https://code.google.com/p/wgibeat/source/detail?r=374)) - "Exotic Economist" ##
  * (Includes WGiEdit v1.1)
  * When selecting "Start Game" from the main menu, the player that pressed Start joins automatically in the next screen.
  * CPU players are indicated in beatlines, player options frames and score bars.
  * Icons are displaed for controller buttons, and controller numbers.
  * Textures are no longer stored as .xnb files, and can be editted normally.
  * WGiBeat now supports themes! Please see [this page](ThemeTutorial.md) for more information.
  * Removed case sensitivity of texture files.
  * The Key Configuration screen has been revamped with revised graphics.
  * Added graphics to Menus, Text Entry and On Screen Keyboard.
  * Updated all heading graphics to use the correct font.
  * Missing textures or metrics no longer cause WGiBeat to crash.
  * Graphics update to evaluation screen (especially the life graph).
  * IIDX style gradual lifegraph entrance.
  * key bindings can now be deleted more individually from the Key Configuration screen.
  * The hits counter flashes when reaching certain milestones.
  * The Log manager has been improved to include datestamps. In addition, the logging level can be changed from the options menu.
  * The Option Screen now has descriptions for each option.
  * A song's audio playback starting position can be adjusted by using the AudioStart field (for songs with very late Offset positions).
  * Profiles stats can now be viewed from the Stats screen, available from the Main Menu.
  * Internal update on how input is handled in the game. In the best case scenario, this should not be noticable by end users.
  * Externalized explanatory text in Mode Select, WGiEdit and Options to external file.
  * Player numbers are now shown in the Evaluation Screen.
  * Mode select screen now has preview graphics for each mode.
  * WGiBeat now has a unique program icon, which is also now displayed on the created start menu shortcut.
  * WGiEdit: A message is now displayed explaining why is song is marked 'invalid'.
  * WGiEdit: When creating a song, the title and artist are read from the audio file's ID tag.
  * Bugfix - WGiEdit: Missing beatline notes have been resolved in the Tuning step.
  * Bugfix - WGiEdit: Streaks now reset correctly.
  * Bugfix - Song previews now play correctly when entering song select for the 2nd time.

## 0.5 ([r290](https://code.google.com/p/wgibeat/source/detail?r=290)) - "Handicapped Doubt" ##
  * Song audio validation by using MD5. This makes it easier to ensure that the correct audio (.mp3) file is used when playing a song, especially when song files (.sng) are distributed.
  * Song Editor: named WGiEdit. Using WGiEdit, it is possible to create a song file (.sng) from scratch, edit existing song files, or delete them. WGiEdit also features measurement tools for determining a song's offset, length and BPM.
  * A Performance indicator is now displayed during gameplay, which provides a graphical display of a player's beatline accuracy (based on judgement ratings).
  * Bugfix: Playing VS CPU mode with 3 players is now possible.

## 0.4 ([r255](https://code.google.com/p/wgibeat/source/detail?r=255)) - "Temporary Laugh" ##

  * New Game Mode: VS CPU Mode. Various skill levels available.
  * CPU Skill levels can be customized by editing the CPUSkill.txt file.
  * Pressing BACK in Song Select Screen returns to ModeSelect.
  * Improved logging. During the start-up process, the initialization of all subsystems are logged. If necessary, additional logging can be added to these subsystems later.
  * "Welcome to WGiBeat" firstrun message.
  * How to play screen for new players.
  * Scalable life graph.
  * Updates to song info display on Song Select screen.
  * Song length displayed on Song Select screen.
  * Profile system. Users can create profiles by entering a name in-game. The profile stores gameplay options such as player name, beatline speed and difficulty. Profiles are optional.
  * Scrolling Mode Select screen.
  * Bugfix: Playing two songs in a row without aborting no longer causes problems.
  * Bugfix: High scores always having too high grade when playing VS CPU.
  * Bugfix: Song length is now calculated properly.
  * Bugfix: Button presses and releases are now handled properly when using a controller.

## 0.3 ([r222](https://code.google.com/p/wgibeat/source/detail?r=222)) - "Delicious Disappointment" ##

  * Song list can be sorted by name, artist or BPM (hold the beatline key in SongSelectScreen).
  * Player status/options indicators.
  * Difficulty and Beatline speed can be changed from SongSelectScreen (hold the select key).
  * Name entry. High scores now save player names as well.
  * Improved High score display
  * Icon for Team mode
  * Tug of war bar for team mode
  * New user-customizable system for menu music. Menu music can now be configured for each screen.
  * Spectrum analyzer in Song Select Screen.
  * Updated layout for Mode Select Screen, which now includes mode descriptions.
  * Song stops if everyone KO'ed
  * Menu music loops now.
  * Options loading has been improved and is more robust.
  * Player Label for beatlines.
  * Initial loading screen for songs, which will display any errors encountered.
  * Enhance song loading - badly written song files, missing audio files and similar errors will no longer crash the game mysteriously.
  * Critical error handling - error message is now displayed and a error dump is written to file.
  * Beatline scaling with 1 or 2 players. Where possible, a "large" beatline is used instead.
  * Menu music crossfades, either to silence, to song previews or to another menu music audio.
  * Team select screen when using Team Mode.

## 0.2 ([r190](https://code.google.com/p/wgibeat/source/detail?r=190))- "Communist Garage" ##

  * New game mode: Team Mode
  * Installer that works.
  * Project now licensed by the BSD license.
  * Project now hosted on Google Code.
  * Display subtitle on SongSelectScreen
  * Full screen mode
  * Framerate / smoothness improvement.
  * Prevent coop mode with 1 player.
  * Blocky Life bar, black background. Also implemented for all modes.
  * Key bindings can now be reset.
  * Other refactors and code enhancements that should not affect players.