using System;
using System.Threading;

namespace WGiBeat.AudioSystem
{
    public class SongPreviewManager : IDisposable
    {

        public SongPreviewManager()
        {
            myTimer = new Timer(UpdatePreviews,null, 0, 25);
            
        }
        public SongManager SongManager { get; set; }

        private int _channelIndexCurrent = -1;
        private int _channelIndexPrev = -1;
        private float _channelPrevVolume = 1.0f;
        private float _channelCurrentVolume = 1.0f;
        private GameSong _currentSong;
        private double _previewTime;

        private Timer myTimer;

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
        private void ReplaySameSong()
        {
            SongManager.StopChannel(_channelIndexCurrent);
            _channelIndexCurrent = SongManager.PlaySoundEffect(_currentSong.Path + "\\" + _currentSong.SongFile);
            SongManager.SetPosition(_channelIndexCurrent, _currentSong.Offset);
            SetVolumes();
        }
        private void UpdatePreviews(object state)
        {
            _previewTime = (_previewTime + 0.025);

            if (_previewTime >= 15)
            {
                _previewTime -= 15;
                ReplaySameSong();
            }
            _channelPrevVolume = Math.Max(0.0f, _channelPrevVolume - 0.025f);
            SetVolumes();
        }

        private void SetVolumes()
        {
            if (_previewTime <= 1)
            {
                _channelCurrentVolume = (float)_previewTime;
            }
            else if (_previewTime >= 14)
            {
                _channelCurrentVolume = (float)(15 - _previewTime);
            }

            if (_channelIndexCurrent != -1)
            {
                SongManager.SetChannelVolume(_channelIndexCurrent, _channelCurrentVolume);
            }
            if (_channelIndexPrev != -1)
            {
                SongManager.SetChannelVolume(_channelIndexPrev, _channelPrevVolume);
            }
            if (_channelPrevVolume <= 0.0f)
            {
            }
        }
    }
}
