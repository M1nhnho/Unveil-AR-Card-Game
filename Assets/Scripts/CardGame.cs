using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public enum Phases { DRAW, TRADE, UPDATE, FIGHTORFLEE, RESULTS }

public class CardGame : MonoBehaviour
{
    public RawImage background;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI instructionsText;
    public Transform phaseButton;

    public Transform roundDisplay;
    public Transform phaseDisplay;
    public Transform roleDisplay;

    [HideInInspector] public Phases phase = Phases.DRAW;
    [HideInInspector] public int round;
    [HideInInspector] public int turn; 
    // false = Red, true = Blue
    [HideInInspector] public bool attackerColour = false; // Red is attacker first
    [HideInInspector] public bool defenderColour = true; // Blue is defender first

    // TV = True Value
    [HideInInspector] public string[] numberTVs = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13" };
    [HideInInspector] public string[] suitTVs = new string[] { "C", "D", "H", "S" };
    [HideInInspector] public int currentCardsScanned = 0;
    public TextMeshProUGUI currentCardsScannedText;
    [HideInInspector] public Dictionary<string, string> cardsDict = new Dictionary<string, string>()
    {
        { "01C", "??" }, { "01D", "??" }, { "01H", "??" }, { "01S", "??" }, // [B]lue, [R]ed
        { "02C", "??" }, { "02D", "??" }, { "02H", "??" }, { "02S", "??" }, // First character refers to ownership
        { "03C", "??" }, { "03D", "??" }, { "03H", "??" }, { "03S", "??" }, // Second character refers to who it is revealed to
        { "04C", "??" }, { "04D", "??" }, { "04H", "??" }, { "04S", "??" },
        { "05C", "??" }, { "05D", "??" }, { "05H", "??" }, { "05S", "??" },
        { "06C", "??" }, { "06D", "??" }, { "06H", "??" }, { "06S", "??" },
        { "07C", "??" }, { "07D", "??" }, { "07H", "??" }, { "07S", "??" },
        { "08C", "??" }, { "08D", "??" }, { "08H", "??" }, { "08S", "??" },
        { "09C", "??" }, { "09D", "??" }, { "09H", "??" }, { "09S", "??" },
        { "10C", "??" }, { "10D", "??" }, { "10H", "??" }, { "10S", "??" },
        { "11C", "??" }, { "11D", "??" }, { "11H", "??" }, { "11S", "??" },
        { "12C", "??" }, { "12D", "??" }, { "12H", "??" }, { "12S", "??" },
        { "13C", "??" }, { "13D", "??" }, { "13H", "??" }, { "13S", "??" }
    };
    System.Random random = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        // Disable cards and randomise TVs
        gameObject.SetActive(false);
        numberTVs = numberTVs.OrderBy(x => random.Next()).ToArray();
        suitTVs = suitTVs.OrderBy(x => random.Next()).ToArray();

        // Begin with Red's turn to scan cards
        round = 1;
        turn = -2; // Negative is the first turn (red/attacker) in each round, positive second turn (blue/defender)
                   // -2 and 0 is to ready the turn
                   // -1 and 1 is the actual turn
        background.color = Color.red;
        background.gameObject.SetActive(true);
        turnText.text = "Red";
        instructionsText.text = "Draw your 5 cards and scan them";
    }

    public void ProgressPhase()
    {
        switch (phase)
        {
            case Phases.DRAW:
                turn++;
                if (turn == -1 || turn == 1)
                {
                    gameObject.SetActive(true);
                    background.gameObject.SetActive(false);
                }

                else if (turn == 0)
                {
                    gameObject.SetActive(false);
                    background.color = Color.blue;
                    background.gameObject.SetActive(true);
                    turnText.text = "Blue";
                }

                else if (turn == 2)
                {
                    gameObject.SetActive(false);
                    background.color = Color.red;
                    background.gameObject.SetActive(true);
                    turnText.text = "Attacker";
                    instructionsText.text = "Decide which cards you wish to trade";
                    turn = -2;
                    phase = Phases.TRADE;
                }
                break;


            case Phases.TRADE:
                turn++;
                if (turn == 2)
                {
                    turn = -2;
                    phase = Phases.UPDATE;
                }
                break;


            case Phases.UPDATE:
                turn++;
                if (turn == 2)
                {
                    turn = -2;
                    phase = Phases.FIGHTORFLEE;
                }
                break;


            case Phases.FIGHTORFLEE:
                attackerColour = !attackerColour;
                defenderColour = !defenderColour;
                round++;
                if (round == 4)
                {
                    gameObject.SetActive(false);
                    background.color = Color.red;
                    background.gameObject.SetActive(true);
                    turnText.text = "Red Wins!";
                    instructionsText.text = "Return to Main Menu on the bottom left";
                    phase = Phases.RESULTS;
                }
                else
                {
                    roundDisplay.GetChild(0).GetComponent<TextMeshProUGUI>().text = round.ToString();

                    background.color = Color.red;
                    background.gameObject.SetActive(true);
                    turnText.text = "Red";
                    instructionsText.text = "Draw your 5 cards and scan them";
                    phase = Phases.DRAW;
                }
                break;


            case Phases.RESULTS:
                break;
        }
    }
}
