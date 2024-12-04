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

                // Usuń dane użytkownika z Firebase Database
                await firebaseClient
                    .Child("users")
                    .Child(_userId)
                    .DeleteAsync();

                // Usuń użytkownika z Firebase Authentication
                bool authDeleted = await DeleteUserFromFirebaseAuth();
                if (!authDeleted)
                {
                    await Application.Current.MainPage.DisplayAlert("Błąd", "Nie udało się usunąć konta z Firebase Authentication.", "OK");
                    return;
                }

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

        private async Task<bool> DeleteUserFromFirebaseAuth()
        {
            try
            {
                // Pobierz token użytkownika z SecureStorage
                var token = await SecureStorage.GetAsync("firebase_token");

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Błąd", "Brak tokena użytkownika. Zaloguj się ponownie.", "OK");
                    return false;
                }

                // Przygotuj żądanie do Firebase REST API
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"https://identitytoolkit.googleapis.com/v1/accounts:delete?key=AIzaSyDNtwI02aWPPvuGGK22Hm8LskD6soyIpZY"),
                    Content = new StringContent($"{{\"idToken\":\"{token}\"}}", System.Text.Encoding.Default, "application/json")
                };

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {error}");
                    return false;
                }

                // Usuń token z SecureStorage
                SecureStorage.Remove("firebase_token");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
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
