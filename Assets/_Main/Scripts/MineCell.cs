using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MineCell : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    private int x;
    private int y;
    private bool hasMine;
    private bool revealed;
    private MinesweeperGame game;

    public bool HasMine => hasMine;
    public bool Revealed => revealed;

    private void Reset()
    {
        button = GetComponent<Button>();
        label = GetComponentInChildren<TMP_Text>();
    }

    public void Setup(int xPosition, int yPosition, bool mine, MinesweeperGame gameReference)
    {
        x = xPosition;
        y = yPosition;
        hasMine = mine;
        game = gameReference;
        revealed = false;

        label.text = "?";
        button.interactable = true;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (revealed) return;

        game.RevealCell(this);
    }

    public void RevealSafe(int nearbyMines)
    {
        revealed = true;
        button.interactable = false;

        if (nearbyMines > 0)
        {
            label.text = nearbyMines.ToString();
        }
        else
        {
            label.text = "";
        }
    }

    public void RevealMine()
    {
        revealed = true;
        button.interactable = false;
        label.text = "X";
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }
}