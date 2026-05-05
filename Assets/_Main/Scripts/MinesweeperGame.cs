using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinesweeperGame : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panelHome;
    [SerializeField] private GameObject panelGame;

    [Header("Board")]
    [SerializeField] private Transform gridContainer;
    [SerializeField] private GameObject cellPrefab;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button exitGameButton;

    [Header("Firebase")]
    [SerializeField] private ScoreManager scoreManager;

    [Header("Game Config")]
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;
    [SerializeField] private int mineCount = 5;
    [SerializeField] private int pointsPerSafeCell = 10;

    private MineCell[,] cells;
    private int score;
    private int safeCellsRevealed;
    private int totalSafeCells;
    private bool gameFinished;

    private void Start()
    {
        newGameButton.onClick.AddListener(StartNewGame);
        exitGameButton.onClick.AddListener(ExitGame);

        StartNewGame();
    }

    public void StartNewGame()
    {
        ClearBoard();

        score = 0;
        safeCellsRevealed = 0;
        gameFinished = false;
        totalSafeCells = (width * height) - mineCount;

        cells = new MineCell[width, height];

        List<Vector2Int> minePositions = GenerateMinePositions();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cellObject = Instantiate(cellPrefab, gridContainer);
                MineCell cell = cellObject.GetComponent<MineCell>();

                bool hasMine = minePositions.Contains(new Vector2Int(x, y));

                cell.Setup(x, y, hasMine, this);
                cells[x, y] = cell;
            }
        }

        UpdateUI("Selecciona una casilla");
    }

    public void RevealCell(MineCell cell)
    {
        if (gameFinished) return;

        if (cell.HasMine)
        {
            cell.RevealMine();
            LoseGame();
            return;
        }

        int nearbyMines = CountNearbyMines(cell.GetX(), cell.GetY());
        cell.RevealSafe(nearbyMines);

        score += pointsPerSafeCell;
        safeCellsRevealed++;

        if (safeCellsRevealed >= totalSafeCells)
        {
            WinGame();
        }
        else
        {
            UpdateUI("Bien, sigue jugando");
        }
    }

    private List<Vector2Int> GenerateMinePositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        while (positions.Count < mineCount)
        {
            int randomX = Random.Range(0, width);
            int randomY = Random.Range(0, height);

            Vector2Int position = new Vector2Int(randomX, randomY);

            if (!positions.Contains(position))
            {
                positions.Add(position);
            }
        }

        return positions;
    }

    private int CountNearbyMines(int cellX, int cellY)
    {
        int count = 0;

        for (int y = cellY - 1; y <= cellY + 1; y++)
        {
            for (int x = cellX - 1; x <= cellX + 1; x++)
            {
                if (x == cellX && y == cellY) continue;

                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    if (cells[x, y].HasMine)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    private void LoseGame()
    {
        gameFinished = true;
        RevealAllMines();
        UpdateUI("Perdiste. Tocaste una mina.");

        if (scoreManager != null)
        {
            scoreManager.SaveScore(score);
        }
    }

    private void WinGame()
    {
        gameFinished = true;
        UpdateUI("Ganaste. Revelaste todas las casillas seguras.");

        if (scoreManager != null)
        {
            scoreManager.SaveScore(score);
        }
    }

    private void RevealAllMines()
    {
        foreach (MineCell cell in cells)
        {
            if (cell.HasMine && !cell.Revealed)
            {
                cell.RevealMine();
            }
        }
    }

    private void UpdateUI(string status)
    {
        scoreText.text = "Puntos: " + score;
        statusText.text = status;
    }

    private void ClearBoard()
    {
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void ExitGame()
    {
        panelGame.SetActive(false);
        panelHome.SetActive(true);
    }

    public void OpenGame()
    {
        panelHome.SetActive(false);
        panelGame.SetActive(true);
        StartNewGame();
    }
}