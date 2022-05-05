using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public enum Phases { DRAW, TRADE, FIGHTORFLEE, RESULTS }
public enum Turns { FIRSTREADY, FIRSTTURN, SECONDREADY, SECONDTURN }

public class CardGame : MonoBehaviour
{
    public RawImage background;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI infoText;
    public Transform progressButton;
    TextMeshProUGUI progressText;
    public Transform secondaryButton;
    TextMeshProUGUI secondaryText;

    public Transform roundDisplay;
    public Transform phaseDisplay;
    public Transform roleDisplay;

    // Hides in inspector and stops it from staying as the original value
    // So allows me to actually change the value in the script here instead of needing to change in the inspector
    [System.NonSerialized] public Phases phase = Phases.DRAW; // The current phase
    [System.NonSerialized] public Turns nextTurn = Turns.FIRSTTURN; // As the progress button changes the current turn to the next turn
    [System.NonSerialized] public int round = 1;
    [System.NonSerialized] public int currentCardsScanned = 0;

    // bool for colour: true = Red, false = Blue
    [System.NonSerialized] public bool attackerColour = true; // Red is Attacker first
    [System.NonSerialized] public bool defenderColour = false; // Blue is Defender first
    bool attackerAccept = false; // If they choose to accept/decline trade or to fight/flee
    bool defenderAccept = false;
    [System.NonSerialized] public List<string> redHand = new List<string>();
    [System.NonSerialized] public List<string> blueHand = new List<string>();

    [System.NonSerialized] public string[] numberTrueValues = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13" };
    [System.NonSerialized] public string[] suitTrueValues = new string[] { "C", "D", "H", "S" };
    System.Random random = new System.Random();


    // Start is called before the first frame update
    void Start()
    {
        progressText = progressButton.GetComponentInChildren<TextMeshProUGUI>();
        secondaryText = secondaryButton.GetComponentInChildren<TextMeshProUGUI>();

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
                if (nextTurn == Turns.FIRSTREADY)
                {
                    if (blueHand.Count == 5) // Separate if statement as the first specifically checks which turn
                    {
                        // Set up for next phase (Trade)
                        phase = Phases.TRADE;
                        nextTurn = Turns.FIRSTTURN;
                        ReadyTurn(attackerColour, "Attacker", "Decide which cards you wish to trade");
                    }
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    if (redHand.Count == 5)
                    {
                        nextTurn = Turns.SECONDTURN;
                        ReadyTurn(false, "Blue", "Draw your 5 cards and scan them");
                    }
                }

                else
                {
                    if (nextTurn == Turns.FIRSTTURN)
                        nextTurn = Turns.SECONDREADY;
                    else
                        nextTurn = Turns.FIRSTREADY;

                    gameObject.SetActive(true);
                    background.gameObject.SetActive(false);
                    infoText.gameObject.SetActive(true);
                }
                break;


            case Phases.TRADE:
                if (nextTurn == Turns.FIRSTREADY)
                {
                    // Set up for next phase (Fight-Or-Flee)
                    phase = Phases.FIGHTORFLEE;
                    nextTurn = Turns.FIRSTTURN;
                    SwapButtonLayout(true, "-", "Ready");
                    ReadyTurn(attackerColour, "Attacker", "Decide to fight or to flee");
                    attackerAccept = false;
                    defenderAccept = false;
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    nextTurn = Turns.SECONDTURN;
                    ReadyTurn(defenderColour, "Defender", "Do you accept or decline the trade?");
                }

                else
                {
                    if (nextTurn == Turns.FIRSTTURN)
                    {
                        nextTurn = Turns.SECONDREADY;
                    }
                    else
                    {
                        nextTurn = Turns.FIRSTREADY;
                        SwapButtonLayout(false, "Accept", "Decline");
                    }

                    gameObject.SetActive(true);
                    background.gameObject.SetActive(false);
                }
                break;


            case Phases.FIGHTORFLEE:
                if (nextTurn == Turns.FIRSTREADY)
                {
                    phase = Phases.RESULTS;
                    if (attackerAccept && defenderAccept)
                    {
                        progressButton.gameObject.SetActive(false);
                        secondaryButton.gameObject.SetActive(false);

                        int redSum = 0;
                        int blueSum = 0;
                        string number, suit;
                        for (int i = 0; i < 5; i++)
                        {
                            number = redHand[i].Substring(0, 2);
                            suit = redHand[i].Substring(2, 1);
                            redSum += Array.IndexOf(numberTrueValues, number) + Array.IndexOf(suitTrueValues, suit) + 2;

                            number = blueHand[i].Substring(0, 2);
                            suit = blueHand[i].Substring(2, 1);
                            blueSum += Array.IndexOf(numberTrueValues, number) + Array.IndexOf(suitTrueValues, suit) + 2;
                        }

                        if (redSum > blueSum)
                            ReadyTurn(true, "Red Wins!", "Return to the Main Menu");
                        else if (redSum < blueSum)
                            ReadyTurn(false, "Blue Wins!", "Return to the Main Menu");
                        else
                            ReadyTurn(false, "Draw!", "Return to the Main Menu");
                    }
                    else if (round == 3) // If it's the last round but someone still flees
                    {
                        progressButton.gameObject.SetActive(false);
                        secondaryButton.gameObject.SetActive(false);
                        ReadyTurn(true, "No Contest!", "Return to the Main Menu");
                    }
                    else
                    {
                        round++;
                        roundDisplay.GetChild(0).GetComponent<TextMeshProUGUI>().text = round.ToString();
                        infoText.text = "0/5 Cards";
                        infoText.gameObject.SetActive(false);

                        attackerColour = !attackerColour;
                        defenderColour = !defenderColour;
                        attackerAccept = false;
                        defenderAccept = false;
                        redHand.Clear();
                        blueHand.Clear();

                        SwapButtonLayout(true, "-", "Next Round");
                        ReadyTurn(defenderColour, "Someone Fled", "Move onto the next round");
                    }
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    nextTurn = Turns.SECONDTURN;
                    SwapButtonLayout(true, "-", "Ready");
                    ReadyTurn(defenderColour, "Defender", "Do you fight or flee in response?");
                }

                else
                {
                    if (nextTurn == Turns.FIRSTTURN)
                    {
                        nextTurn = Turns.SECONDREADY;
                    }
                    else
                    {
                        nextTurn = Turns.FIRSTREADY;
                        if (attackerAccept)
                            infoText.text = "Attacker chose Fight";
                        else
                            infoText.text = "Attacker chose Flee";
                        infoText.gameObject.SetActive(true);
                    }

                    gameObject.SetActive(true);
                    background.gameObject.SetActive(false);
                    SwapButtonLayout(false, "Fight", "Flee");
                }
                break;


            case Phases.RESULTS:
                phase = Phases.DRAW;
                nextTurn = Turns.FIRSTTURN;
                ReadyTurn(true, "Red", "Draw your 5 cards and scan them");
                break;
        }
    }

    void ReadyTurn(bool colour, string turn, string instructions)
    {
        gameObject.SetActive(false);
        infoText.gameObject.SetActive(false);

        if (colour)
            background.color = Color.red;
        else
            background.color = Color.blue;
        turnText.text = turn;
        instructionsText.text = instructions;
        background.gameObject.SetActive(true);
    }

    void SwapButtonLayout(bool singleButton, string secondary, string progress)
    {
        Vector3 progressPosition = progressButton.localPosition;
        if (singleButton)
        {
            progressPosition.x = 0;
            secondaryButton.gameObject.SetActive(false);
            progressText.text = progress;
        }
        else
        {
            progressPosition.x = 300;
            secondaryButton.gameObject.SetActive(true);
            progressText.text = progress;
            secondaryText.text = secondary;
        }
        progressButton.localPosition = progressPosition;
    }

    public void ConfirmAcception()
    {
        if (phase == Phases.TRADE || phase == Phases.FIGHTORFLEE)
        {
            if (nextTurn == Turns.FIRSTREADY)
                defenderAccept = true;
            else if (nextTurn == Turns.SECONDREADY)
                attackerAccept = true;
        }
        ProgressPhase();
    }
}
