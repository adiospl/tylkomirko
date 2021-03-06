﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using WykopSDK.API.Models;
using WykopSDK.Parsers;

namespace Mirko.ViewModel
{
    public class EntryBaseViewModel : ViewModelBase
    {
        public EntryBase DataBase { get; set; }
        public EmbedViewModel EmbedVM { get; set; }

        private string _tappedHashtag = null;
        [JsonIgnore]
        public string TappedHashtag
        {
            get { return _tappedHashtag; }
            set { Set(() => TappedHashtag, ref _tappedHashtag, value); }
        }

        private bool _showVoters = false;
        [JsonIgnore]
        public bool ShowVoters
        {
            get { return _showVoters; }
            set { Set(() => ShowVoters, ref _showVoters, value); }
        }

        public EntryBaseViewModel()
        {
        }

        public EntryBaseViewModel(EntryBase d)
        {
            DataBase = d;
            if(d.Embed != null)
                EmbedVM = new EmbedViewModel(DataBase.Embed);
        }

        private RelayCommand _voteCommand = null;
        [JsonIgnore]
        public RelayCommand VoteCommand
        {
            get { return _voteCommand ?? (_voteCommand = new RelayCommand(async () => await ExecuteVoteCommand())); }
        }

        private async Task ExecuteVoteCommand(bool verbose = true)
        {
            if(App.ApiService.UserInfo == null)
            {
                StatusBarManager.ShowText("Musisz być zalogowany.");
                return;
            }

            if(verbose)
                StatusBarManager.ShowProgress();

            Vote reply = null;
            if (DataBase is EntryComment)
            {
                var c = DataBase as EntryComment;
                reply = await App.ApiService.VoteEntry(id: c.EntryID, commentID: c.ID, upVote: !c.Voted, isItEntry: false);
            }
            else
            {
                reply = await App.ApiService.VoteEntry(id: DataBase.ID, upVote: !DataBase.Voted);
            }

            if (reply != null)
            {
                DataBase.VoteCount = reply.VoteCount;
                DataBase.Voted = !DataBase.Voted;
                DataBase.Voters = reply.Voters;

                App.TelemetryClient.TrackEvent(DataBase.Voted ? "Upvote" : "Downvote");

                if (verbose)
                    StatusBarManager.ShowText(DataBase.Voted ? "Dodano plusa." : "Cofnięto plusa.");
            }
            else
            {
                if(verbose)
                    StatusBarManager.ShowText("Nie udało się oddać głosu.");
            }
        }

        private RelayCommand _replyCommand = null;
        [JsonIgnore]
        public RelayCommand ReplyCommand
        {
            get { return _replyCommand ?? (_replyCommand = new RelayCommand(ExecuteReplyCommand)); }
        }

        private void ExecuteReplyCommand()
        {
            var VM = SimpleIoc.Default.GetInstance<NewEntryViewModel>();

            if(DataBase is EntryComment)
            {
                var d = DataBase as EntryComment;
                VM.NewEntry.EntryID = d.EntryID;
            }
            else
            {
                VM.NewEntry.EntryID = DataBase.ID;
            }
            
            VM.NewEntry.CommentID = 0;
            VM.NewEntry.IsEditing = false;
            VM.GoToNewEntryPage(new List<EntryBaseViewModel>() { this });
        }

        private RelayCommand _deleteCommand = null;
        [JsonIgnore]
        public RelayCommand DeleteCommand
        {
            get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(ExecuteDeleteCommand)); }
        }

        private async void ExecuteDeleteCommand()
        {
            if (DataBase is EntryComment)
            {
                var d = DataBase as EntryComment;
                var result = await App.ApiService.DeleteEntry(rootID: d.EntryID, id: d.ID, isComment: true);
                if(result == d.ID)
                {
                    StatusBarManager.ShowText("Komentarz został usunięty.");
                    Messenger.Default.Send(new Tuple<uint, uint>(d.EntryID, d.ID), "Remove comment");
                }
            }
            else
            {
                var result = await App.ApiService.DeleteEntry(id: DataBase.ID);
                if(result == DataBase.ID)
                {
                    StatusBarManager.ShowText("Wpis został usunięty.");
                    Messenger.Default.Send<uint>(result, "Remove entry");
                }
            }
        }

        private RelayCommand _editCommand = null;
        [JsonIgnore]
        public RelayCommand EditCommand
        {
            get { return _editCommand ?? (_editCommand = new RelayCommand(ExecuteEditCommand)); }
        }

        private void ExecuteEditCommand()
        {
            var VM = SimpleIoc.Default.GetInstance<NewEntryViewModel>();

            if (DataBase is EntryComment)
            {
                var d = DataBase as EntryComment;
                VM.NewEntry.EntryID = d.EntryID;
                VM.NewEntry.CommentID = d.ID;
            }
            else
            {
                VM.NewEntry.EntryID = DataBase.ID;
            }

            VM.NewEntry.IsEditing = true;
            VM.GoToNewEntryPage(new List<EntryBaseViewModel>() { this });
        }

        private RelayCommand _refreshCommand = null;
        [JsonIgnore]
        public RelayCommand RefreshCommand
        {
            get { return _refreshCommand ?? (_refreshCommand = new RelayCommand(ExecuteRefreshCommand)); }
        }

        private async void ExecuteRefreshCommand()
        {
            if (DataBase == null) return;

            await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpis...");
            var newEntry = await App.ApiService.GetEntry(DataBase.ID);
            if (newEntry == null)
            {
                await StatusBarManager.ShowTextAsync("Nie udało się pobrać wpisu.");
            }
            else
            {
                var newVM = new EntryViewModel(newEntry);
                Messenger.Default.Send(newVM, "Update");

                await StatusBarManager.HideProgressAsync();
            }
        }

        private RelayCommand _shareCommand = null;
        [JsonIgnore]
        public RelayCommand ShareCommand
        {
            get { return _shareCommand ?? (_shareCommand = new RelayCommand(ExecuteShareCommand)); }
        }

        private void ExecuteShareCommand()
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                DataRequestedEventArgs>(ShareLinkHandler);

            DataTransferManager.ShowShareUI();
        }

        private void ShareLinkHandler(DataTransferManager sender, DataRequestedEventArgs args)
        {
            string url, body;

            if(DataBase is EntryComment)
            {
                var d = DataBase as EntryComment;
                url = string.Format("http://wykop.pl/wpis/{0}/#comment-{1}", d.EntryID, d.ID);
            }
            else
            {
                url = string.Format("http://wykop.pl/wpis/{0}", DataBase.ID);
            }

            body = HtmlToText.Convert(DataBase.Text.Replace("\n", " "));
            var splittedBody = new List<string>(body.Split(' '));
            const int wordsToGet = 4;

            var title = string.Join(" ", splittedBody.GetRange(0, (splittedBody.Count >= wordsToGet) ? wordsToGet : splittedBody.Count));
            if (splittedBody.Count > wordsToGet)
                title += "...";

            DataRequest request = args.Request;
            request.Data.Properties.Title = title;
            request.Data.SetWebLink(new Uri(url));
        }

        private RelayCommand<List<EntryBaseViewModel>> _voteMultiple = null;
        [JsonIgnore]
        public RelayCommand<List<EntryBaseViewModel>> VoteMultiple
        {
            get { return _voteMultiple ?? (_voteMultiple = new RelayCommand<List<EntryBaseViewModel>>(ExecuteVoteMultiple)); }
        }

        private async void ExecuteVoteMultiple(List<EntryBaseViewModel> list)
        {
            var onlyUpVotes = list.Where(x => !x.DataBase.Voted);
            if (onlyUpVotes.Count() == 0)
            {
                await StatusBarManager.ShowTextAsync("Plusa możesz dać tylko raz ( ͡° ͜ʖ ͡°)");
                return;
            }

            double progress = 0;
            double progressStep = 1 / (double)onlyUpVotes.Count();

            foreach(var entry in onlyUpVotes)
            {
                await StatusBarManager.ShowProgressAsync(progress);
                await entry.ExecuteVoteCommand(false);
                progress += progressStep;
            }

            await StatusBarManager.ShowTextAsync("Plusy zostały przyznane.");
        }
    }
}
