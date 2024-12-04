using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IMP.ViewModels
{
    public class SettingsViewModel : BindableObject
    {
        private readonly string _userId;

        // Właściwość UserId dostępna w XAML
        public string UserId => _userId;

        public ICommand NavigateToSectionsControlCommand { get; }
        public ICommand NavigateToChangePasswordCommand { get; }
        public ICommand DeleteAccountCommand { get; }

        public SettingsViewModel(string userId)
        {
            _userId = userId;

            // Inicjalizacja komend
            NavigateToSectionsControlCommand = new Command(NavigateToSectionsControl);
            NavigateToChangePasswordCommand = new Command(NavigateToChangePassword);
            DeleteAccountCommand = new Command(DeleteAccount);
        }

        // Funkcja do nawigacji do strony sterowania sekcjami
        private async void NavigateToSectionsControl()
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new SectionsPage(_userId));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", $"Nie udało się otworzyć strony sterowania sekcjami: {ex.Message}", "OK");
            }
        }

        // Funkcja do nawigacji do strony zmiany hasła
        private async void NavigateToChangePassword()
        {
            try
            {
                // Implementacja zmiany hasła - strona do stworzenia
                //await Application.Current.MainPage.Navigation.PushAsync(new ChangePasswordPage(_userId));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", $"Nie udało się otworzyć strony zmiany hasła: {ex.Message}", "OK");
            }
        }

        // Funkcja do usuwania konta
        private async void DeleteAccount()
        {
            bool confirmDelete = await Application.Current.MainPage.DisplayAlert(
                "Usuń konto",
                "Czy na pewno chcesz usunąć swoje konto? Ta akcja jest nieodwracalna.",
                "Tak", "Nie");

            if (!confirmDelete) return;

            try
            {
                // Połączenie z Firebase Database
                var firebaseClient = new FirebaseClient("https://impdb-557fa-default-rtdb.europe-west1.firebasedatabase.app");

                // Sprawdź, czy użytkownik istnieje w bazie danych
                var user = await firebaseClient
                    .Child("users")
                    .Child(_userId)
                    .OnceSingleAsync<User>();

                if (user == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Błąd", "Nie znaleziono użytkownika o podanym ID.", "OK");
                    return;
                }

                // Usuń użytkownika z Firebase Database
                await firebaseClient
                    .Child("users")
                    .Child(_userId)
                    .DeleteAsync();

                // Powiadomienie o sukcesie
                await Application.Current.MainPage.DisplayAlert("Sukces", "Konto zostało usunięte.", "OK");

                // Przekierowanie do strony logowania
                await Application.Current.MainPage.Navigation.PopToRootAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", $"Wystąpił błąd podczas usuwania konta: {ex.Message}", "OK");
            }
        }

    }

    // Klasa User dopasowana do struktury bazy danych
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
