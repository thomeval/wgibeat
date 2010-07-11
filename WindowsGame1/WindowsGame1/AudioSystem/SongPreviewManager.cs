using System;
using System.Threading;

namespace WGiBeat.AudioSystem
{
    /// <summary>
    /// A manager that handles playing previews of GameSongs. Previews are played using 
    /// a SongManager, as a stream. The song is played from the GameSong's 'offset' point
    /// (representing the first playable beat of the song), for the duration specified.
    /// Crossfading between playing previews is also provided.
    /// </summary>
    public class SongPreviewManager : IDisposable
    {
        public SongPreviewManager()
        {
            myTimer = new Timer(UpdatePreviews,null, 0, 25);
            PreviewDuration = 10;
        }
        public SongManager SongManager { get; set; }
        public int PreviewDuration { get; set; }

        private int _channelIndexCurrent = -1;
        private int _channelIndexPrev = -1;
        private float _channelPrevVolume = 1.0f;
        private float _channelCurrentVolume = 1.0f;
        
        private GameSong _currentSong;
        private double _previewTime;

        private readonly Timer myTimer;

        /// <summary>
        /// Properly disposes the SongPreviewManager by stopping the playing previews,
        /// and timer used.
        /// </summary>
        public void Dispose()
        {
            myTimer.Dispose();
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
            _currentSong = song;
            SongManager.StopChannel(_channelIndexPrev);
            _channelIndexPrev = _channelIndexCurrent;
            _channelPrevVolume = _channelCurrentVolume;
            _channelIndexCurrent = SongManager.PlaySoundEffect(song.Path + "\\" + song.SongFile);
            SongManager.SetPosition(_channelIndexCurrent, song.Offset);
            _previewTime = 0.0;
            SetVolumes();
        }

        /// <summary>
        /// Restarts the preview of the current song. Used when the preview is completed
        /// but not stopped.
        /// </summary>
        private void ReplaySameSong()
        {
            SongManager.StopChannel(_channelIndexCurrent);
            _channelIndexCurrent = SongManager.PlaySoundEffect(_currentSong.Path + "\\" + _currentSong.SongFile);
            SongManager.SetPosition(_channelIndexCurrent, _currentSong.Offset);
            SetVolumes();
        }

        /// <summary>
        /// Timer method. Adjusts the preview time and checks whether the volume should be adjusted.
        /// </summary>
        /// <param name="state">Not used.</param>
        private void UpdatePreviews(object state)
        {
            _previewTime = (_previewTime + 0.025);

            if (_previewTime >= PreviewDuration)
            {
                _previewTime -= PreviewDuration;
                ReplaySameSong();
            }
            _channelPrevVolume = Math.Max(0.0f, _channelPrevVolume - 0.025f);
            SetVolumes();
        }

        /// <summary>
        /// Adjusts the channel volumes of the previewed song(s) according to
        /// preview time. This facilitates fade in and crossfade out.
        /// </summary>
        private void SetVolumes()
        {
            if (_previewTime <= 1)
            {
                _channelCurrentVolume = (float)_previewTime;
            }
            else if (_previewTime >= PreviewDuration -1)
            {
                _channelCurrentVolume = (float)(PreviewDuration - _previewTime);
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
