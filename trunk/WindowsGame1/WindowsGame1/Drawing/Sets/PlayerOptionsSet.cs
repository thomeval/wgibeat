using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using WGiBeat.Players;

namespace WGiBeat.Managers
{
    class PlayerOptionsSet : DrawableObject
    {

        private readonly List<PlayerOptionsFrame> _optionsFrames;
        private Sprite _optionsFrameAttract;
        private readonly Vector2 _offScreen = new Vector2(-1000, -1000);
        public bool DrawAttract { get; set; }
        public bool StackableFrames { get; set; }

        public Player[] Players { get; set; }
        public Vector2[] Positions { get; set; }
        public GameType CurrentGameType { get; set; }

        public event EventHandler GameTypeInvalidated;

        public PlayerOptionsSet()
        {
            _optionsFrames = new List<PlayerOptionsFrame>();
            InitSprites();
  
        }

        private void InitSprites()
        {
            _optionsFrameAttract = new Sprite
                                       {
                                           SpriteTexture = TextureManager.Textures("PlayerOptionsFrameAttract"),
                                           ColorShading = new Color(255, 255, 255, 128)
                                       };
        }

        public void CreatePlayerOptionsFrames()
        {
            CheckNumberOfPlayers();
            _optionsFrames.Clear();
            for (int x = 0; x < 4; x++)
            {
                //Absolute positions for each player options frame.
                _optionsFrames.Add(new PlayerOptionsFrame { Player = Players[x], PlayerIndex = x });
                _optionsFrames[x].Position = (Positions[x]);
            }

            if (StackableFrames)
            {
                CreatePlayerOptionsFramesStack();
                return;
            }   

            if (DrawAttract && (from e in _optionsFrames where !e.Player.Playing select e).Any())
            {
                var freeSlot = (from e in _optionsFrames where !e.Player.Playing select e.PlayerIndex).FirstOrDefault();
                _optionsFrameAttract.Position = Positions[freeSlot];
            }
            else
            {
                _optionsFrameAttract.Position = _offScreen;
            }
   
        }

        private void CreatePlayerOptionsFramesStack()
        {
            var frameCount = 0;
 
            //Stack each frame on top of each other - position is dependant on how many active players there are.
            for (int x = 3; x >= 0; x--)
            {

                _optionsFrames[x].Position = (Positions[frameCount]);
                if (Players[x].Playing)
                {
                    frameCount++;
                }
            }

            //Draw the attract one if it is enabled and there are less than 4 players.
            if (DrawAttract && (frameCount < 4))
            {
                _optionsFrameAttract.Position = (Positions[frameCount]);
            }
            else
            {
                _optionsFrameAttract.Position = _offScreen;
            }
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (PlayerOptionsFrame pof in _optionsFrames.Where(e => e.Player.Playing))
            {       
                pof.Draw(spriteBatch);
            }
            _optionsFrameAttract.Draw(spriteBatch);
        }

        public bool PerformAction(InputAction inputAction)
        {
            var playerIdx = inputAction.Player - 1;
            var playerOptions = (from e in _optionsFrames where e.PlayerIndex == playerIdx select e).SingleOrDefault();

            //Ignore inputs from players not playing EXCEPT for players joining in.
            if ((inputAction.Player == 0) || (!Players[inputAction.Player - 1].IsHumanPlayer))
            {
                if (inputAction.Action == "START")
                {
                    JoinPlayer(inputAction.Player);
                    return true;
                }
                return false;
            }

            //Ignore input if the player's options frame isn't in option change mode.
            if (!playerOptions.OptionChangeActive)
            {
                return false;
            }
 
            switch (inputAction.Action)
            {
                case "LEFT":
                        playerOptions.AdjustDifficulty(-1);
                    break;
                case "RIGHT":
                        playerOptions.AdjustDifficulty(1);
                    break;
                case "UP":
                        playerOptions.AdjustSpeed(1);
                    break;
                case "DOWN":
                        playerOptions.AdjustSpeed(-1);
                    break;
                case "START":
                    LeavePlayer(inputAction.Player);
                    break;
            }
            return true;

        }

        private void LeavePlayer(int player)
        {
            Players[player - 1].Playing = false;
            CheckNumberOfPlayers();
            CreatePlayerOptionsFrames();
   
        }

        private void CheckNumberOfPlayers()
        {
            var playerCount = (from e in Players where e.IsHumanPlayer select e).Count();
            switch (CurrentGameType)
            {
                case GameType.NORMAL:
                case GameType.VS_CPU:
                    //Anything is allowed here.
                    break;
                    case GameType.COOPERATIVE:
                    case GameType.TEAM:
                    if (playerCount < 2)
                    {
                        RequestReturnToMainMenu();
                    }
                    break;
            }
        }

        private void RequestReturnToMainMenu()
        {
            if (GameTypeInvalidated != null)
            {
                GameTypeInvalidated(this, null);
            }
        }

        private void JoinPlayer(int player)
        {
            //Prevent a fourth player from joining in VS CPU mode.
            var activePlayers = (from e in Players where e.Playing select e).Count();
            if (activePlayers == 4)
            {
                return;
            }

            //Shift the CPU player to another slot
            if (Players[player-1].IsCPUPlayer)
            {
                ShiftCPUPlayer(player);
            }

            Players[player - 1].ResetStats();
            Players[player - 1].Profile = null;
            Players[player - 1].Playing = true;
            Players[player - 1].ApplyDefaultOptions();

            if (CurrentGameType == GameType.TEAM)
            {
                AssignTeam(player);
            }
            if (CurrentGameType == GameType.VS_CPU)
            {
                Players[player - 1].Team = 1;
            }
            CreatePlayerOptionsFrames();
          
        }

        private void AssignTeam(int player)
        {
            Players[player - 1].Team = 0;
            var team1Count = (from e in Players where e.Team == 1 select e).Count();
            var team2Count = (from e in Players where e.Team == 2 select e).Count();
            if (team1Count > team2Count)
            {
                Players[player - 1].Team = 2;
            }
            else
            {
                Players[player - 1].Team = 1;
            }
        }

        private void ShiftCPUPlayer(int player)
        {
            Players[player-1].CPU = false;
            for (int x = 0; x < 4; x++)
            {
                if (!Players[x].Playing)
                {
                    Players[x].Playing = true;
                    Players[x].CPU = true;
                    Players[x].Profile = null;
                    Players[x].Team = 2;
                    return;
                }            
            }
        }

        public void SetChangeMode(int player, bool enabled)
        {
            var pof = (from e in _optionsFrames where e.PlayerIndex == player - 1 select e).SingleOrDefault();
            
            if (pof != null)
            {
                pof.OptionChangeActive = enabled;
            }
        }

        public void CheckSyncDifficulty()
        {

            if ((from e in Players where e.IsHumanPlayer select e).Count() == 0)
            {
                return;
            }

            var lowestDifficulty = (from e in Players where e.IsHumanPlayer select e.PlayerOptions.PlayDifficulty).Min();

            foreach (Player player in (from e in Players select e))
            {
                player.PlayerOptions.PlayDifficulty = lowestDifficulty;
            }
        }
    }
}
