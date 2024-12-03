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

        // Dodaj właściwość UserId, aby była dostępna w XAML
        public string UserId => _userId;

        public ICommand NavigateToSectionsControlCommand { get; }
        public ICommand NavigateToChangePasswordCommand { get; }
        public ICommand DeleteAccountCommand { get; }

        public SettingsViewModel(string userId)
        {
            _userId = userId;

            // Inicjalizowanie komend
            NavigateToSectionsControlCommand = new Command(NavigateToSectionsControl);
            NavigateToChangePasswordCommand = new Command(NavigateToChangePassword);
            DeleteAccountCommand = new Command(DeleteAccount);
        }

        // Funkcja do nawigacji do strony sterowania sekcjami
        private async void NavigateToSectionsControl()
        {
            try
            {
                // Przejście do strony sterowania sekcjami
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
                // Przejście do strony zmiany hasła
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

            if (confirmDelete)
            {
                try
                {
                    // 1. Uzyskaj dostęp do Firebase Realtime Database
                    var firebaseClient = new FirebaseClient("https://impdb-557fa-default-rtdb.europe-west1.firebasedatabase.app");

                    // 2. Pobierz dane użytkownika z Realtime Database na podstawie userId
                    var user = await firebaseClient
                        .Child("users")  // 'users' to ścieżka w bazie danych
                        .Child(_userId)   // Używamy _userId z SettingsViewModel
                        .OnceSingleAsync<User>();  // User to klasa odpowiadająca danym użytkownika

                    if (user == null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Błąd", "Nie znaleziono użytkownika o podanym ID.", "OK");
                        return;
                    }

                    // Logowanie danych użytkownika
                    Console.WriteLine($"User found: {user.Name}, {user.Email}");

                    // 3. Usuwamy użytkownika z bazy danych
                    await firebaseClient
                        .Child("users")
                        .Child(_userId)  // Usuń użytkownika na podstawie userId
                        .DeleteAsync();   // Usuwanie danych użytkownika

                    // 4. Usuwamy konto z Firebase Authentication
                    var authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyDNtwI02aWPPvuGGK22Hm8LskD6soyIpZY"));
                    var currentUser = await authProvider.SignInWithEmailAndPasswordAsync(user.Email, "user_password");  // Zastąp "user_password" odpowiednią metodą autoryzacji

                    if (currentUser == null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Błąd", "Nie udało się zalogować użytkownika, sprawdź dane.", "OK");
                        return;
                    }

                    // 5. Usuwamy konto z Firebase Authentication
                    //await authProvider.DeleteAccount(currentUser.FirebaseToken);

                    // 6. Powiadomienie o sukcesie
                    await Application.Current.MainPage.DisplayAlert("Usunięto konto", "Twoje konto zostało usunięte.", "OK");

                    // 7. Przekierowanie na stronę logowania
                    await Application.Current.MainPage.Navigation.PopToRootAsync();
                }
                catch (Exception ex)
                {
                    // Obsługa błędów
                    await Application.Current.MainPage.DisplayAlert("Błąd", $"Wystąpił błąd podczas usuwania konta: {ex.Message}", "OK");
                }
            }
        }
    }

    // Przykładowa klasa User, której używasz w bazie danych (dopasuj do swojej struktury)
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
