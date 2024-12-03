using Firebase.Database;

namespace IMP;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        // Inicjalizacja Firebase Database
        var firebaseUrl = "https://impdb-557fa-default-rtdb.europe-west1.firebasedatabase.app/";
        var firebaseClient = new FirebaseClient(firebaseUrl);

        return builder.Build();
    }
}
