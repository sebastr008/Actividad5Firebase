using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panelHome;
    [SerializeField] private GameObject panelLeaderboard;

    [Header("UI")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Button buttonShowLeaderboard;
    [SerializeField] private Button buttonCloseLeaderboard;

    [Header("Config")]
    [SerializeField] private int maxResults = 5;

    private DatabaseReference usersReference;

    private void Start()
    {
        usersReference = FirebaseDatabase.DefaultInstance.GetReference("users");

        buttonShowLeaderboard.onClick.AddListener(OpenLeaderboard);
        buttonCloseLeaderboard.onClick.AddListener(CloseLeaderboard);

        panelLeaderboard.SetActive(false);
    }

    public void OpenLeaderboard()
    {
        panelHome.SetActive(false);
        panelLeaderboard.SetActive(true);

        LoadLeaderboard();
    }

    public void CloseLeaderboard()
    {
        panelLeaderboard.SetActive(false);
        panelHome.SetActive(true);
    }

    private void LoadLeaderboard()
    {
        ClearRows();

        usersReference
            .OrderByChild("score")
            .LimitToLast(maxResults)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Error cargando la tabla de puntajes.");
                    return;
                }

                List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

                foreach (DataSnapshot child in task.Result.Children)
                {
                    string username = "Sin nombre";
                    int score = 0;

                    if (child.Child("username").Value != null)
                    {
                        username = child.Child("username").Value.ToString();
                    }

                    if (child.Child("score").Value != null)
                    {
                        int.TryParse(child.Child("score").Value.ToString(), out score);
                    }

                    entries.Add(new LeaderboardEntry(username, score));
                }

                entries.Sort((a, b) => b.score.CompareTo(a.score));

                for (int i = 0; i < entries.Count; i++)
                {
                    GameObject rowObject = Instantiate(rowPrefab, content);
                    TMP_Text rowText = rowObject.GetComponent<TMP_Text>();

                    if (rowText == null)
                    {
                        rowText = rowObject.GetComponentInChildren<TMP_Text>();
                    }

                    rowText.text = $"{i + 1}. {entries[i].username} - {entries[i].score} pts";
                }
            });
    }

    private void ClearRows()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    private class LeaderboardEntry
    {
        public string username;
        public int score;

        public LeaderboardEntry(string username, int score)
        {
            this.username = username;
            this.score = score;
        }
    }
}