﻿using ModernFlyouts.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Media;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace ModernFlyouts
{
    public partial class SessionControl : UserControl
    {

        #region Properties

        private GlobalSystemMediaTransportControlsSession _SMTCSession;

        public GlobalSystemMediaTransportControlsSession SMTCSession
        {
            get => _SMTCSession;
            set
            {
                if (value != null)
                {
                    UpdateSessionInfo(value);
                    value.MediaPropertiesChanged += Session_MediaPropertiesChanged;
                    value.PlaybackInfoChanged += Session_PlaybackInfoChanged;
                    value.TimelinePropertiesChanged += Session_TimelinePropertiesChanged;
                    _SMTCSession = value;
                }
                else
                {
                    DisposeSession();
                }
            }
        }

        #endregion

        public SessionControl()
        {
            InitializeComponent();
            SongName.Text = "";
            SongArtist.Text = "";
            PreviousButton.Click += (_, __) => PreviousTrack();
            PlayPauseButton.Click += (_, __) => PlayOrPause();
            NextButton.Click += (_, __) => NextTrack();
            ShuffleButton.Click += (_, __) => ShuffleTrack();
            RepeatButton.Click += (_, __) => ChangeRepeatMode();
            StopButton.Click += (_, __) => StopTrack();
        }

        private async void Session_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession session, PlaybackInfoChangedEventArgs args)
        {
            await Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (session != null && session.GetPlaybackInfo() != null)
                {
                    UpdatePlaybackInfo(session);
                }
            }));
        }

        private async void Session_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession session, MediaPropertiesChangedEventArgs args)
        {
            await Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
            {
                if (session != null && session.GetPlaybackInfo() != null)
                {
                    UpdateSessionInfo(session);
                }
            }));
        }

        private async void Session_TimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession session, TimelinePropertiesChangedEventArgs args)
        {
            await Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (session != null && session.GetTimelineProperties() != null)
                {
                    UpdateTimelineInfo(session);
                }
            }));
        }

        #region Session Methods

        private async void PreviousTrack()
        {
            try
            {
                if (_SMTCSession != null)
                {
                    await _SMTCSession.TrySkipPreviousAsync();
                }
            }
            catch { }
        }

        private async void PlayOrPause()
        {
            try
            {
                if (_SMTCSession != null)
                {
                    var playback = _SMTCSession.GetPlaybackInfo();
                    if (playback != null)
                    {
                        if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                        {
                            await _SMTCSession.TryPauseAsync();
                        }
                        else if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                        {
                            await _SMTCSession.TryPlayAsync();
                        }
                    }
                }
            }
            catch { }
        }

        private async void NextTrack()
        {
            try
            {
                if (_SMTCSession != null)
                {
                    await _SMTCSession.TrySkipNextAsync();
                }
            }
            catch { }
        }

        private async void ShuffleTrack()
        {
            try
            {
                if (_SMTCSession != null)
                {
                    var playback = _SMTCSession.GetPlaybackInfo();
                    if (playback != null)
                    {
                        if (playback.IsShuffleActive.HasValue)
                        {
                            if (playback.IsShuffleActive.Value)
                            {
                                await _SMTCSession.TryChangeShuffleActiveAsync(false);
                            }
                            else
                            {
                                await _SMTCSession.TryChangeShuffleActiveAsync(true);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private async void ChangeRepeatMode()
        {
            try
            {
                if (_SMTCSession != null)
                {
                    var playback = _SMTCSession.GetPlaybackInfo();
                    if (playback != null)
                    {
                        if (playback.AutoRepeatMode == MediaPlaybackAutoRepeatMode.None)
                        {
                            await _SMTCSession.TryChangeAutoRepeatModeAsync(MediaPlaybackAutoRepeatMode.List);
                        }
                        else if (playback.AutoRepeatMode == MediaPlaybackAutoRepeatMode.List)
                        {
                            await _SMTCSession.TryChangeAutoRepeatModeAsync(MediaPlaybackAutoRepeatMode.Track);
                        }
                        else if (playback.AutoRepeatMode == MediaPlaybackAutoRepeatMode.Track)
                        {
                            await _SMTCSession.TryChangeAutoRepeatModeAsync(MediaPlaybackAutoRepeatMode.None);
                        }
                    }
                }
            }
            catch { }
        }

        private async void StopTrack()
        {
            try
            {
                if (_SMTCSession != null)
                {
                    await _SMTCSession.TryStopAsync();
                }
            }
            catch { }
        }

        #endregion

        #region Updating View

        private void UpdatePlaybackInfo(GlobalSystemMediaTransportControlsSession session)
        {
            UpdatePlayPauseButtonIcon(session);
            UpdateShuffleButton(session);
            UpdateRepeatButton(session);
        }

        private void UpdatePlayPauseButtonIcon(GlobalSystemMediaTransportControlsSession session)
        {
            try
            {
                if (session != null)
                {
                    var playback = session.GetPlaybackInfo();
                    if (playback != null)
                    {
                        if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                        {
                            PlayPauseIcon.Glyph = CommonGlyphs.Pause;
                            PlayPauseButton.ToolTip = "Pause";
                        }
                        else if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                        {
                            PlayPauseIcon.Glyph = CommonGlyphs.Play;
                            PlayPauseButton.ToolTip = "Play";
                        }
                    }
                }
            }
            catch { }
        }


        private void UpdateShuffleButton(GlobalSystemMediaTransportControlsSession session)
        {
            try
            {
                if (session != null)
                {
                    var playback = session.GetPlaybackInfo();
                    if (playback != null)
                    {
                        if (playback.IsShuffleActive.HasValue)
                        {
                            if (playback.IsShuffleActive.Value)
                            {
                                ShuffleButton.IsChecked = true;
                                ShuffleButton.ToolTip = "Shuffle: on";
                            }
                            else
                            {
                                ShuffleButton.IsChecked = false;
                                ShuffleButton.ToolTip = "Shuffle: off";
                            }
                        }
                        else
                        {
                            ShuffleButton.IsChecked = false;
                        }
                    }
                }
            }
            catch { }
        }

        private void UpdateRepeatButton(GlobalSystemMediaTransportControlsSession session)
        {
            try
            {
                if (session != null)
                {
                    var playback = session.GetPlaybackInfo();
                    if (playback != null)
                    {
                        if (playback.AutoRepeatMode == MediaPlaybackAutoRepeatMode.None)
                        {
                            RepeatButton.IsChecked = false;
                            RepeatIcon.Glyph = CommonGlyphs.RepeatAll;
                            RepeatButton.ToolTip = "Repeat: off";
                        }
                        else if (playback.AutoRepeatMode == MediaPlaybackAutoRepeatMode.Track)
                        {
                            RepeatButton.IsChecked = true;
                            RepeatIcon.Glyph = CommonGlyphs.RepeatOne;
                            RepeatButton.ToolTip = "Repeat: one";
                        }
                        else if (playback.AutoRepeatMode == MediaPlaybackAutoRepeatMode.List)
                        {
                            RepeatButton.IsChecked = true;
                            RepeatIcon.Glyph = CommonGlyphs.RepeatAll;
                            RepeatButton.ToolTip = "Repeat: all";
                        }
                    }
                }
            }
            catch { }
        }

        private void UpdateTimelineInfo(GlobalSystemMediaTransportControlsSession session)
        {
            try
            {
                var timeline = session.GetTimelineProperties();

                if (session.GetPlaybackInfo().Controls.IsPlaybackPositionEnabled && timeline != null)
                {
                    TimeBar.Minimum = timeline.StartTime.TotalSeconds;
                    TimeBar.Maximum = timeline.EndTime.TotalSeconds;
                    TimeBar.Value = timeline.Position.TotalSeconds;

                    StartTimeBlock.Text = timeline.StartTime.ToString();
                    EndTimeBlock.Text = timeline.EndTime.ToString();

                    TimelineInfo.Visibility = Visibility.Visible;
                }
                else
                {
                    TimeBar.Minimum = 0;
                    TimeBar.Maximum = 100;
                    TimeBar.Value = 0;

                    TimelineInfo.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private async void UpdateSessionInfo(GlobalSystemMediaTransportControlsSession session)
        {
            try
            {
                var mediaInfo = await session.TryGetMediaPropertiesAsync();

                if (mediaInfo != null)
                {
                    SongName.Text = mediaInfo.Title;
                    SongArtist.Text = mediaInfo.Artist;
                }

                var playback = session.GetPlaybackInfo();

                if (playback != null)
                {
                    NextButton.IsEnabled = playback.Controls.IsNextEnabled;
                    PreviousButton.IsEnabled = playback.Controls.IsPreviousEnabled;
                    PlayPauseButton.IsEnabled = playback.Controls.IsPauseEnabled || playback.Controls.IsPlayEnabled;
                    ShuffleButton.IsEnabled = playback.Controls.IsShuffleEnabled;
                    RepeatButton.IsEnabled = playback.Controls.IsRepeatEnabled;
                    StopButton.IsEnabled = playback.Controls.IsStopEnabled;
                }

                UpdateTimelineInfo(session);

                UpdatePlaybackInfo(session);

                await SetThumbnailAsync(mediaInfo.Thumbnail, playback.PlaybackType);
            }
            catch { }
        }

        #region Thumbnail

        private async Task SetThumbnailAsync(IRandomAccessStreamReference thumbnail, MediaPlaybackType? playbackType)
        {
            if (thumbnail != null)
            {
                using var strm = await thumbnail.OpenReadAsync();
                if (strm != null)
                {
                    using var nstream = strm.AsStream();
                    if (nstream != null && nstream.Length > 0)
                    {
                        thumb.ImageSource = BitmapFrame.Create(nstream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        thumbBackground.ImageSource = thumb.ImageSource;
                        return;
                    }
                }
            }

            thumb.ImageSource = GetDefaultThumbnail(playbackType);
            thumbBackground.ImageSource = null;
        }

        private ImageSource GetDefaultThumbnail(MediaPlaybackType? playbackType)
        {
            return playbackType switch
            {
                MediaPlaybackType.Image => AudioHelper.GetDefaultImageThumbnail(),
                MediaPlaybackType.Music => AudioHelper.GetDefaultAudioThumbnail(),
                MediaPlaybackType.Video => AudioHelper.GetDefaultVideoThumbnail(),
                MediaPlaybackType.Unknown => null,
                _ => null
            };
        }

        #endregion

        #endregion

        public void DisposeSession()
        {
            if (_SMTCSession != null)
            {
                _SMTCSession.MediaPropertiesChanged -= Session_MediaPropertiesChanged;
                _SMTCSession.PlaybackInfoChanged -= Session_PlaybackInfoChanged;
                _SMTCSession.TimelinePropertiesChanged -= Session_TimelinePropertiesChanged;
            }
            _SMTCSession = null;
        }
    }
}