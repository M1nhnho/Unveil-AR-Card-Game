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

    public TextMeshProUGUI roundDisplay;
    public RawImage phaseDisplay;
    public Texture[] phaseIcons = new Texture[4]; // Cards, Handshake, Lightning, Trophy
    public RawImage roleDisplay;
    public Texture[] roleIcons = new Texture[2]; // Sword, Shield

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
        ReadyTurn(true, "Red", "Draw your 5 cards and scan them", "Ready");
    }

    public void ProgressPhase()
    {
        switch (phase)
        {
            // ----- Draw Phase --------------------------------------------------
            case Phases.DRAW:
                if (nextTurn == Turns.FIRSTREADY)
                {
                    if (blueHand.Count == 5) // Separate if statement as the first specifically checks which turn
                    {
                        // Set up for next phase (Trade)
                        phase = Phases.TRADE;
                        nextTurn = Turns.FIRSTTURN;
                        ReadyTurn(attackerColour, "Attacker", "Decide which cards you wish to trade", "Ready");
                    }
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    if (redHand.Count == 5)
                    {
                        nextTurn = Turns.SECONDTURN;
                        ReadyTurn(false, "Blue", "Draw your 5 cards and scan them", "Ready");
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
                    SwapButtonLayout(false, "Reset", "Confirm");
                }
                break;

            // ----- Trade Phase --------------------------------------------------
            case Phases.TRADE:
                if (nextTurn == Turns.FIRSTREADY)
                {
                    attackerAccept = false;
                    defenderAccept = false;

                    // Set up next phase (Fight-Or-Flee)
                    phase = Phases.FIGHTORFLEE;
                    nextTurn = Turns.FIRSTTURN;
                    ReadyTurn(attackerColour, "Attacker", "Decide to fight or to flee", "Ready");
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    nextTurn = Turns.SECONDTURN;
                    ReadyTurn(defenderColour, "Defender", "Do you accept or decline the trade?", "Ready");
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

            // ----- Fight-Or-Flee Phase --------------------------------------------------
            case Phases.FIGHTORFLEE:
                if (nextTurn == Turns.FIRSTREADY)
                {
                    phase = Phases.RESULTS;

                    if (attackerAccept && defenderAccept) // Both chose to Fight
                    {
                        // Disable progress buttons as the match has ended
                        progressButton.gameObject.SetActive(false);
                        secondaryButton.gameObject.SetActive(false);

                        // Calculate whose hand had the higher True Value sum
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
                            ReadyTurn(true, "Red Wins!", "Return to the Main Menu", "-");
                        else if (redSum < blueSum)
                            ReadyTurn(false, "Blue Wins!", "Return to the Main Menu", "-");
                        else
                            ReadyTurn(false, "Draw!", "Return to the Main Menu", "-");
                    }
                    else if (round == 3) // If it's the last round but someone still Flees
                    {
                        progressButton.gameObject.SetActive(false);
                        secondaryButton.gameObject.SetActive(false);
                        ReadyTurn(true, "No Contest!", "Return to the Main Menu", "-");
                    }
                    else // Otherwise, next round
                    {
                        // Reset back to Draw phase
                        round++;
                        roundDisplay.text = round.ToString();
                        infoText.text = "0/5 Cards";
                        infoText.gameObject.SetActive(false);

                        attackerColour = !attackerColour;
                        defenderColour = !defenderColour;
                        attackerAccept = false;
                        defenderAccept = false;
                        redHand.Clear();
                        blueHand.Clear();

                        ReadyTurn(defenderColour, "Someone Fled", "Move onto the next round", "Next Round");
                    }
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    nextTurn = Turns.SECONDTURN;
                    ReadyTurn(defenderColour, "Defender", "Do you fight or flee in response?", "Ready");
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

            // ----- Results Phase --------------------------------------------------
            case Phases.RESULTS:
                // Purpose of this phase is just to display the results of the round/match
                // Also to disable the current 10 cards in play to prevent rescanning

                // Set up next phase (Draw)
                phase = Phases.DRAW;
                nextTurn = Turns.FIRSTTURN;
                ReadyTurn(true, "Red", "Draw your 5 cards and scan them", "Ready");
                break;
        }
    }

    void ReadyTurn(bool colour, string turn, string instructions, string button)
    {
        gameObject.SetActive(false); // Disable scanning
        infoText.gameObject.SetActive(false);
        SwapButtonLayout(true, "-", button);

        if (colour)
        {
            background.color = Color.red;
            roleDisplay.color = Color.red;
        }
        else
        {
            background.color = Color.blue;
            roleDisplay.color = Color.blue;
        }

        if (colour && attackerColour) // If the colour passed in is the Attacker, display sword
            roleDisplay.texture = roleIcons[0];
        else // Otherwise, display shield
            roleDisplay.texture = roleIcons[1];

        phaseDisplay.texture = phaseIcons[(int)phase];
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

    public void PrepareProgress()
    {
        if (phase == Phases.DRAW) // Reset scans if they mistakenly scan the wrong card(s) to ensure correct hand
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
        else if (phase == Phases.TRADE || phase == Phases.FIGHTORFLEE) // Confirms choice to allow progress based on choices
        {
            if (nextTurn == Turns.FIRSTREADY)
                defenderAccept = true;
            else if (nextTurn == Turns.SECONDREADY)
                attackerAccept = true;
            ProgressPhase();
        }
    }
}
