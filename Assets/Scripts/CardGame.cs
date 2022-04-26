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

    // Hides in inspector and stops it from staying as the original value
    // So allows me to actually change the value in the script here
    [System.NonSerialized] public Phases phase = Phases.DRAW;
    [System.NonSerialized] public int round = 1;
    [System.NonSerialized] public int turn = -2; // Negative is the first turn (Red/Attacker) in each round, positive second turn (Blue/Defender)
                                                 // -2 and 0 is to ready the turn
                                                 // -1 and 1 is the actual turn

    // bool for colour: true = Red, false = Blue
    [System.NonSerialized] public bool attackerColour = true; // Red is attacker first
    [System.NonSerialized] public bool defenderColour = false; // Blue is defender first
    [System.NonSerialized] public List<string> redHand = new List<string>();
    [System.NonSerialized] public List<string> blueHand = new List<string>();

    [System.NonSerialized] public string[] numberTrueValues = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13" };
    [System.NonSerialized] public string[] suitTrueValues = new string[] { "C", "D", "H", "S" };
    System.Random random = new System.Random();

    [System.NonSerialized] public int currentCardsScanned = 0;
    public TextMeshProUGUI currentCardsScannedText;


    // Start is called before the first frame update
    void Start()
    {
        // Randomise True Values
        numberTrueValues = numberTrueValues.OrderBy(x => random.Next()).ToArray();
        suitTrueValues = suitTrueValues.OrderBy(x => random.Next()).ToArray();

        // Begin with Red's turn to scan cards
        ReadyTurn(true, "Red", "Draw your 5 cards and scan them");
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
                    currentCardsScannedText.gameObject.SetActive(true);
                }

                else if (turn == 0)
                {
                    ReadyTurn(false, "Blue", "Draw your 5 cards and scan them");
                }

                else if (turn == 2)
                {
                    // Set up for next phase (Trade)
                    ReadyTurn(attackerColour, "Attacker", "Decide which cards you wish to trade");
                    turn = -2;
                    phase = Phases.TRADE;
                }
                break;


            case Phases.TRADE:
                turn++;
                if (turn == -1 || turn == 1)
                {
                    gameObject.SetActive(true);
                    background.gameObject.SetActive(false);
                }

                else if (turn == 0)
                {
                    ReadyTurn(defenderColour, "Defender", "Do you accept or decline the trade?");
                }

                else if (turn == 2)
                {
                    // Set up for next phase (Trade)
                    ReadyTurn(attackerColour, "Attacker", "Decide to fight or to flee");
                    turn = -2;
                    phase = Phases.FIGHTORFLEE;
                }
                break;


            case Phases.UPDATE: // MAY NOT BE NECESSARY -> If the cards have buttons, it can automatically update the ownership of cards
                turn++;
                if (turn == 2)
                {
                    turn = -2;
                    phase = Phases.FIGHTORFLEE;
                }
                break;


            case Phases.FIGHTORFLEE:
                round++;
                attackerColour = !attackerColour;
                defenderColour = !defenderColour;
                redHand.Clear();
                blueHand.Clear();

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

    void ReadyTurn(bool colour, string turn, string instructions)
    {
        gameObject.SetActive(false);
        currentCardsScannedText.gameObject.SetActive(true);

        if (colour)
            background.color = Color.red;
        else
            background.color = Color.blue;
        turnText.text = turn;
        instructionsText.text = instructions;
        background.gameObject.SetActive(true);
    }
}
