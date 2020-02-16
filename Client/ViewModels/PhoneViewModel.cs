using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Client.Models;
using System.Runtime.CompilerServices;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Collections;

namespace Client.ViewModels {
    class PhoneViewModel : INotifyPropertyChanged {
        private Card selectedCard;
        private bool isEditing = false; // идет ли в данный момент редактирование
        private bool isError = false;  // проверка на наличие ошибки при работе с сервером,
                                       // в случае ошибки блокируем UI кроме кнопки "Обновить"
        private bool isSelected = false;  // проверка на налиичие выбранного элемента в ListBox
        public static Card SelectedCardRestored; // объект, сохраненный перед редактированием
        public bool IsAddOrEdit = false;  // добавление (true) или удаление (false)
        public ObservableCollection<Card> Cards { get; set; }

        private DelegateCommand getAllCommand;
        public DelegateCommand GetAllCommand {
            get {
                return getAllCommand ??
                  (getAllCommand = new DelegateCommand(obj => {
                      Cards.Clear();
                      GetCards();
                      if (Cards.Any()) {
                          SelectedCard = Cards.First();
                      }
                  },
                  (obj) => !IsEditing));
            }
        }

        private DelegateCommand addCommand;
        public DelegateCommand AddCommand {
            get {
                return addCommand ??
                  (addCommand = new DelegateCommand(obj => {
                      IsAddOrEdit = true;
                      IsEditing = true;
                      Card card = new Card();
                      Cards.Insert(0, card);
                      SelectedCard = card;
                  },
                  (obj) => !IsEditing));
            }
        }

        private DelegateCommand removeCommand;
        public DelegateCommand RemoveCommand {
            get {
                return removeCommand ??
                  (removeCommand = new DelegateCommand(obj => {
                      var items = new ObservableCollection<Card>();
                      // удаление выбранных в ListBox элементов
                      foreach (var i in (IList)obj) {
                          items.Add((Card)i);
                      }
                      foreach (var i in items) {
                          if (DeleteCards(i.Id)) {
                              Cards.Remove(i);
                          }
                      }
                  },
                 (obj) => (Cards.Count > 0 && !IsEditing))); // кнопка удаления неактивна если нету карточек либо идет редактирование
            }
        }

        private DelegateCommand editCommand;
        public DelegateCommand EditCommand {
            get {
                return editCommand ??
                  (editCommand = new DelegateCommand(obj => {
                      if (obj is Card card) {
                          // сохраняем состояние объекта перед редактированием
                          SelectedCardRestored = new Card { Id = card.Id, Title = card.Title, Body = card.Body };
                      }
                      IsAddOrEdit = false;
                      IsEditing = true;
                  },
                 (obj) => !IsEditing));
            }
        }

        private DelegateCommand saveCommand;
        public DelegateCommand SaveCommand {
            get {
                return saveCommand ??
                  (saveCommand = new DelegateCommand(obj => {
                      if (obj is Card card) {
                          if (IsAddOrEdit) {
                              // если карточка не дошла до сервера, не сохраняем и на клиенте
                              if (!PostCards(card)) {
                                  Cards.Remove(card);
                              }
                          }
                          else {
                              if (!PutCards(card)) {
                                  Card editCard = Cards.FirstOrDefault(i => i.Id == card.Id);
                                  if (editCard != null) {
                                      editCard.Id = SelectedCardRestored.Id;
                                      editCard.Title = SelectedCardRestored.Title;
                                      editCard.Body = SelectedCardRestored.Body;

                                  }
                              }
                          }
                      }
                      IsEditing = false;
                  },
                  (obj) => (SelectedCard?.Title != null && SelectedCard?.Title != ""
                            && SelectedCard?.Body?.Length > 0))); 
                    // кнопка Save не активна в случае пустого названия и отсутствующего изображения
            }
        }

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand {
            get {
                return cancelCommand ??
                  (cancelCommand = new DelegateCommand(obj => {
                      if (obj is Card card) {
                          if (IsAddOrEdit) {
                              Cards.Remove(card);
                          }
                          // при отмене редактирования, возвращаем карточку какой она была до редактирования
                          else {
                              Card editCard = Cards.FirstOrDefault(i => i.Id == card.Id);
                              if (editCard != null) {
                                  editCard.Id = selectedCard.Id;
                                  editCard.Title = SelectedCardRestored.Title;
                                  editCard.Body = SelectedCardRestored.Body;
                              }
                          }
                      }
                      IsEditing = false;
                  }));
            }
        }

        private DelegateCommand openFileCommand;
        public DelegateCommand OpenFileCommand {
            get {
                return openFileCommand ??
                  (openFileCommand = new DelegateCommand(obj => {
                      SelectedCard.Body = File.ReadAllBytes(obj.ToString());
                  },
                  (obj) => File.Exists(obj.ToString())));
            }
        }

        public Card SelectedCard {
            get { return selectedCard; }
            set {
                IsSelected = value == null ? false : true;
                selectedCard = value;
                OnPropertyChanged("SelectedCard");
            }
        }

        public bool IsEditing {
            get { return isEditing; }
            set {
                isEditing = value;
                OnPropertyChanged("IsEditing");
            }
        }

        public bool IsError {
            get { return isError; }
            set {
                isError = value;
                OnPropertyChanged("IsError");
            }
        }

        public bool IsSelected {
            get { return isSelected; }
            set {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }


        public PhoneViewModel() {
            Cards = new ObservableCollection<Card>();
            GetCards();
        }

        private bool GetCards() {
            List<CardResponse> cardResponseList = new List<CardResponse>();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:56698/api/cards/");
            request.ContentType = "application/json";
            request.Method = "GET";
            try {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK) {
                    using (Stream stream = response.GetResponseStream()) {
                        using (StreamReader reader = new StreamReader(stream)) {
                            string line = "";
                            while ((line = reader.ReadLine()) != null) {
                                cardResponseList = JsonConvert.DeserializeObject<List<CardResponse>>(line);
                            }
                        }
                    }
                    response.Close();
                    foreach (var i in cardResponseList) {
                        Cards.Add(CardResponseToCard(i));
                    }
                    IsError = false;
                    return true;
                }
            }
            catch {
                IsError = true;
            }
            return false;
        }

        private bool PostCards(Card card) {
            CardResponse cardResponse = new CardResponse();
            string cardData = JsonConvert.SerializeObject(card);
            var data = Encoding.ASCII.GetBytes(cardData);
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:56698/api/cards/");
                request.ContentType = "application/json";
                request.Method = "POST";
                request.ContentLength = cardData.Length;
                using (Stream stream = request.GetRequestStream()) {
                    stream.Write(data, 0, data.Length);
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK) {
                    response.Close();
                    IsError = false;
                    return true;
                }
            }
            catch {
                IsError = true;
            }
            return false;
        }

        private bool PutCards(Card card) {
            CardResponse cardResponse = new CardResponse();
            string cardData = JsonConvert.SerializeObject(card);
            var data = Encoding.ASCII.GetBytes(cardData);
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:56698/api/cards/");
                request.ContentType = "application/json";
                request.Method = "PUT";
                request.ContentLength = cardData.Length;
                using (Stream stream = request.GetRequestStream()) {
                    stream.Write(data, 0, data.Length);
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK) {
                    response.Close();
                    IsError = false;
                    return true;
                }
            }
            catch {
                IsError = true;
            }
            return false;
        }

        private bool DeleteCards(int id) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:56698/api/cards/" + id.ToString());
            request.ContentType = "application/json";
            request.Method = "DELETE";
            try {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK) {
                    response.Close();
                    IsError = false;
                    return true;
                }
            }
            catch {
                IsError = true;
            }
            return false;
        }

        private Card CardResponseToCard(CardResponse cardResponse) {
            return new Card { Id = cardResponse.Id, Title = cardResponse.Title, Body = Convert.FromBase64String(cardResponse.Body) };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
