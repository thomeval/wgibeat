WGiBeat - v0.8 Readme
http://code.google.com/p/wgibeat/
wgibeat{at}gmail{period}com
By thomeval
============================================

Setup
--------------------------------------------
Before running this game, ensure that your system has the .NET Framework 3.5 SP 1, and XNA Framework Redistributable 3.1 installed. If either of them are not present, the game will fail to load. Note that the WGiBeat installer should detect whether these are installed for you. Due to size concerns, the .NET Framework is not included in the installer (a link will be provided instead). 
Alternatively, the prerequisites can be downloaded from the links below:

XNA Framework Redistributable 3.1:
http://www.microsoft.com/downloads/details.aspx?FamilyID=53867a2a-e249-4560-8011-98eb3e799ef2&displaylang=en

.NET Framework 3.5 SP1
http://www.microsoft.com/downloads/details.aspx?FamilyID=ab99342f-5d1a-413d-8319-81da479ab0d7&DisplayLang=en


Starting WGiBeat
---------------------------------------------
When WGiBeat is started for the first time, a tutorial will be displayed explaining the basic gameplay. This can be skipped by
pressing the Escape key. The tutorial can be viewed at any time by selecting "How To Play" from the main menu.

Note that WGiBeat requires songs to be playable. If no songs are installed, WGiBeat will display an error message when attempting to
start a new game. Songs can be created from the song editor, and sample songs are also available from the official website (listed
at the top of the readme). An exaplanation of the song format (.sng) is given in the readme inside the Songs folder. All songs should be
contained inside this folder.

Another important consideration for users running Windows Vista or 7: It is *strongly* recommended that WGiBeat be run with administrative privileages, particularly if WGiBeat is installed into the Program Files folder. This is because WGiBeat routinely reads and writes to files contained in the folder. If WGiBeat is not run in this way, expect unusual issues (such as reading successfully from files that don't seem to exist).

Usage and default keys
---------------------------------------------
Start the game by running WGiBeat.exe. A shortcut should be created automatically on the start menu if the installer is used.
The game is controlled entirely with either a keyboard or an Xbox 360 controller. The following keys are used by default:

Player 1:
W = Up
A = Left
S = Down
D = Right
Space = Beatline
Q = Start
E = Select

Player 2:
Arrow keys for up, left, down and right
Numpad 0 = Beatline
Numpad 1 = Start
Numpad 2 = Select

Player 3:
Numpad 8 = Up
Numpad 4 = Left
Numpad 2 = Down
Numpad 6 = Right
Insert = Beatline
Page Down = Start
Page Up = Select

Player 4 (Xbox 360 controller 1):
Y = Up
X = Left
A = Down
S = Right
RB or LB = Beatline
LT or RT = Select
Back = Back
Start = Start

All keys can be changed from by Keys option in the main menu. To reset the assigned keys to default, delete the keys.conf file created by the game in the same folder.

To start a game, first press select Start Game from the main menu, then press Start. On the next screen, every player that wishes to play must press start to join in. Once joined in, select your difficulty. Once ready, select the Decision option, and press Start.
When all players are ready, the select mode screen is displayed. Choose which mode to play, and press start again.
When the mode is chosen, a list of available songs is displayed. Select the song to be played, and press start again.


Gameplay Explanation
----------------------------------------------
For a tutorial on the basic gameplay of WGiBeat, please see the the How To Play screen, accessible from the main menu.

For more help, please see the official website, or contact the developers at the address given at the top of the readme.

Compiling the source code
----------------------------------------------
To compile your own version of WGiBeat from source, the same prerequisites are necessary as mentioned above, in addition to Visual Studio 2008, and XNA Game Studio 3.1. With these installed, the project can be loaded and compiled normally by opening the .sln file.
For more information, or to join the development team, please contact the developers using the address given at the top of the readme.
