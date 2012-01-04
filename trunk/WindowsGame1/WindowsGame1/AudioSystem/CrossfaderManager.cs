using System;
using WGiBeat.Managers;

namespace WGiBeat.AudioSystem
{
    /// <summary>
    /// A manager that handles playing crossfading of any FMOD sound channel. Audio such as Song 
    /// Previews are played using a AudioManager, as a stream. The song is played from the GameSong's 
    /// 'offset' point (representing the first playable beat of the song), for the duration specified.
    /// Crossfading between playing previews is also provided.
    /// </summary>
    public class CrossfaderManager : Manager
    {
        public CrossfaderManager(LogManager log, AudioManager audioManager)
        {
            Log = log;
            Log.AddMessage("Initializing Crossfader...", LogLevel.INFO);
            AudioManager = audioManager;
            PreviewDuration = 10;
        }
        public AudioManager AudioManager { get; set; }
        public int PreviewDuration { get; set; }

        private int _channelIndexCurrent = -1;
        private int _channelIndexPrev = -1;
        private float _channelPrevVolume = 1.0f;
        private float _channelCurrentVolume = 1.0f;
        private double _lastUpdate;

        public int ChannelIndexCurrent
        {
            get { return _channelIndexCurrent; }
        }

        private GameSong _currentSong;
        private double _previewTime;

        /// <summary>
        /// Properly disposes the CrossfaderManager by stopping the playing previews.
        /// </summary>
        public void Dispose()
        {
            if (_channelIndexCurrent != -1)
            {
                AudioManager.StopChannel(_channelIndexCurrent);
            }
            if (_channelIndexPrev != -1)
            {
                AudioManager.StopChannel(_channelIndexPrev);
            }
        }

        /// <summary>
        /// Changes the currently playing song preview to the GameSong provided.
        /// The provided song will fade in. If another song preview is playing, it
        /// will be crossfaded out.
        /// </summary>
        /// <param name="song">The GameSong to preview.</param>
        /// <param name="force">Whether the provided game song should be played regardless of whether it 
        /// was already playing.</param>
        public void SetPreviewedSong(GameSong song, bool force)
        {
            if ((_currentSong == song) && !force)
            {
                return;
            }
            var channelId = AudioManager.PlaySoundEffect(song.Path + "\\" + song.AudioFile,false,false,false,0);
            SetNewChannel(channelId);
            AudioManager.SetPosition(channelId, song.Offset * 1000);
            AudioManager.SetChannelVolume(channelId, 0.0f);
            _currentSong = song;
        }

        /// <summary>
        /// Changes the currently playing sound channel to a new one. This method
        /// should be used if a non-GameSong sound is to be used and crossfaded out.
        /// </summary>
        /// <param name="channelId">The ID of the channel to use as the new current Channel.</param>
        public void SetNewChannel(int channelId)
        {
            if (_channelIndexPrev > -1)
            {
                AudioManager.StopChannel(_channelIndexPrev);
            }
            _channelIndexPrev = _channelIndexCurrent;
            _channelPrevVolume = _channelCurrentVolume;
            _channelIndexCurrent = channelId;
            _previewTime = 0.0;
            SetVolumes();
            
        }

        /// <summary>
        /// Restarts the preview of the current song. Used when the preview is completed
        /// but not stopped.
        /// </summary>
        private void ReplaySameSong()
        {
            if (_channelIndexCurrent == -1)
            {
                return;
            }
            
            AudioManager.SetPosition(_channelIndexCurrent, _currentSong.Offset*1000);
            _previewTime = 0.0;
            SetVolumes();
        }

        /// <summary>
        /// Updates the CrossFader with the current GameTime. This needs to be called frequently for the Crossfader
        /// to be effective.
        /// </summary>
        /// <param name="gameTime">The total amount of time since the start of the program, in seconds.</param>
        public void Update(double gameTime)
        {
            var timeElapsed = gameTime - _lastUpdate;
            _lastUpdate = gameTime;
            UpdatePreviews(timeElapsed);
        }

        /// <summary>
        /// Adjusts the preview time and checks whether the volume for either channel should be adjusted.
        /// </summary>
        private void UpdatePreviews(double timePassed)
        {

                _previewTime = (_previewTime + timePassed);

                if ((PreviewDuration > 0 )&&(_previewTime >= PreviewDuration))
                {
                    _previewTime -= PreviewDuration;
                    ReplaySameSong();
                }
                _channelPrevVolume = Math.Max(0.0f, _channelPrevVolume - (float) (timePassed * 0.5));
                
                if ((_channelIndexPrev != -1 ) && (_channelPrevVolume == 0.0f))
                {
                    AudioManager.StopChannel(_channelIndexPrev);
                    _channelIndexPrev = -1;
                }
                SetVolumes();
        }

        /// <summary>
        /// Adjusts the channel volumes of the previewed song(s) according to
        /// preview time. This facilitates fade in and crossfade out.
        /// </summary>
        private void SetVolumes()
        {
            if (_previewTime <= 2)
            {
                _channelCurrentVolume = (float)_previewTime / 2;
            }
            else if ((PreviewDuration > 0) && (_previewTime >= PreviewDuration -2))
            {
                _channelCurrentVolume = (float)Math.Max(0,(PreviewDuration - _previewTime)/2);
            }

            if (_channelIndexCurrent != -1)
            {
                AudioManager.SetChannelVolume(_channelIndexCurrent, _channelCurrentVolume);
            }
            if (_channelIndexPrev != -1)
            {
                AudioManager.SetChannelVolume(_channelIndexPrev, _channelPrevVolume);
            }
        }

        /// <summary>
        /// Stops both the current channel, as well as the previous channel, instantly.
        /// </summary>
        public void StopBoth()
        {
            if (_channelIndexPrev != -1)
            {
                AudioManager.StopChannel(_channelIndexPrev);
                _channelIndexPrev = -1;
            }
            if (_channelIndexCurrent != -1)
            {
                AudioManager.StopChannel(_channelIndexCurrent);
                _channelIndexCurrent = -1;
            }
            _currentSong = null;
        }

    }
}
