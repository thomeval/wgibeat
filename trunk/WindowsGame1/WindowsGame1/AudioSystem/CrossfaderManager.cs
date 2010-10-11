using System;
using System.Threading;

namespace WGiBeat.AudioSystem
{
    /// <summary>
    /// A manager that handles playing crossfading of any FMOD sound channel. Audio such as Song 
    /// Previews are played using a SongManager, as a stream. The song is played from the GameSong's 
    /// 'offset' point (representing the first playable beat of the song), for the duration specified.
    /// Crossfading between playing previews is also provided.
    /// </summary>
    public class CrossfaderManager : IDisposable
    {
        public CrossfaderManager()
        {
            _myTimer = new Timer(UpdatePreviews,null, 0, 100);
            PreviewDuration = 10;
        }
        public SongManager SongManager { get; set; }
        public int PreviewDuration { get; set; }

        private int _channelIndexCurrent = -1;
        private int _channelIndexPrev = -1;
        private float _channelPrevVolume = 1.0f;
        private float _channelCurrentVolume = 1.0f;

        public int ChannelIndexPrevious
        {
            get { return _channelIndexPrev; }
            set
            {
                _channelIndexPrev = value;
                _channelPrevVolume = SongManager.GetChannelVolume(_channelIndexPrev);
            } 
        }

        public int ChannelIndexCurrent
        {
            get { return _channelIndexCurrent; }
        }

        private GameSong _currentSong;
        private double _previewTime;

        private readonly Timer _myTimer;

        /// <summary>
        /// Properly disposes the CrossfaderManager by stopping the playing previews,
        /// and timer used.
        /// </summary>
        public void Dispose()
        {
            _myTimer.Dispose();
            if (_channelIndexCurrent != -1)
            {
                SongManager.StopChannel(_channelIndexCurrent);
            }
            if (_channelIndexPrev != -1)
            {
                SongManager.StopChannel(_channelIndexPrev);
            }
        }

        /// <summary>
        /// Changes the currently playing song preview to the GameSong provided.
        /// The provided song will fade in. If another song preview is playing, it
        /// will be crossfaded out.
        /// </summary>
        /// <param name="song">The GameSong to preview.</param>
        public void SetPreviewedSong(GameSong song)
        {
            if (_currentSong == song)
            {
                return;
            }
            SetNewChannel(SongManager.PlaySoundEffect(song.Path + "\\" + song.SongFile));
            SongManager.SetPosition(_channelIndexCurrent, song.Offset);
            _currentSong = song;
        }

        /// <summary>
        /// Changes the currently playing sound channel to a new one. This method
        /// should be used if a non-GameSong sound is to be used and crossfaded out.
        /// </summary>
        /// <param name="channelId"></param>
        public void SetNewChannel(int channelId)
        {
            _currentSong = null;
            if (_channelIndexPrev > -1)
            {
                SongManager.StopChannel(_channelIndexPrev);
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
            
            //Indicate that the current channel index is invalid.
            int tempIdx = _channelIndexCurrent;
            _channelIndexCurrent = -1;
            SongManager.StopChannel(tempIdx);

            _channelIndexCurrent = SongManager.PlaySoundEffect(_currentSong.Path + "\\" + _currentSong.SongFile);
            SongManager.SetPosition(_channelIndexCurrent, _currentSong.Offset);
            SetVolumes();
        }

        private bool _updateInProgress;
        /// <summary>
        /// Timer method. Adjusts the preview time and checks whether the volume should be adjusted.
        /// </summary>
        /// <param name="state">Not used.</param>
        private void UpdatePreviews(object state)
        {
            if (!_updateInProgress)
            {
                _updateInProgress = true;
                _previewTime = (_previewTime + 0.1);

                if ((PreviewDuration > 0 )&&(_previewTime >= PreviewDuration))
                {
                    _previewTime -= PreviewDuration;
                    ReplaySameSong();
                }
                _channelPrevVolume = Math.Max(0.0f, _channelPrevVolume - 0.05f);
                SetVolumes();
                _updateInProgress = false;
            }
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
                SongManager.SetChannelVolume(_channelIndexCurrent, _channelCurrentVolume);
            }
            if (_channelIndexPrev != -1)
            {
                SongManager.SetChannelVolume(_channelIndexPrev, _channelPrevVolume);
            }
        }

    }
}
