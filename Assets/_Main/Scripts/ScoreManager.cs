using System;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text textBestScore;

    public void SaveScore(int newScore)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("No hay usuario autenticado. No se puede guardar el puntaje.");
            return;
        }

        string userId = user.UserId;
        DatabaseReference userReference = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId);

        userReference.Child("score").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("No se pudo consultar el puntaje actual.");
                return;
            }

            int currentBestScore = 0;

            if (task.Result.Exists && task.Result.Value != null)
            {
                int.TryParse(task.Result.Value.ToString(), out currentBestScore);
            }

            if (newScore > currentBestScore)
            {
                userReference.Child("score").SetValueAsync(newScore);
                userReference.Child("updatedAt").SetValueAsync(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                if (textBestScore != null)
                {
                    textBestScore.text = "Mejor puntaje: " + newScore;
                }

                Debug.Log("Nuevo mejor puntaje guardado: " + newScore);
            }
            else
            {
                Debug.Log("El puntaje no superó el mejor puntaje anterior.");
            }
        });
    }
}