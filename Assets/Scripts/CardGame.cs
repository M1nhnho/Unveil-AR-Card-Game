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
    public RawImage cover;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI infoText;
    public Transform progressButton;
    TextMeshProUGUI progressText;
    public Transform secondaryButton;
    TextMeshProUGUI secondaryText;
    public RawImage errorPopup;
    TextMeshProUGUI errorMessage;

    public TextMeshProUGUI roundDisplay;
    public RawImage phaseDisplay;
    public Texture[] phaseIcons = new Texture[3]; // Cards, Handshake, Lightning
    public RawImage roleDisplay;
    public Texture[] roleIcons = new Texture[2]; // Sword, Shield

    // Hides in inspector and stops it from staying as the original value
    // So allows me to actually change the value in the script here instead of needing to change in the inspector
    [System.NonSerialized] public Phases phase = Phases.DRAW; // The current phase
    [System.NonSerialized] public Turns nextTurn = Turns.FIRSTTURN; // As the progress button changes the current turn to the next turn
    [System.NonSerialized] public int round = 1;
    [System.NonSerialized] public int currentCardsScanned = 0;

    // There is a Red player and a Blue player, any reference to colour refers to the player
    // bool for colour: true = Red, false = Blue
    [System.NonSerialized] public bool attackerColour = true; // Red is Attacker first
    [System.NonSerialized] public bool defenderColour = false; // Blue is Defender first
    // If they choose to accept/decline trade or to Fight/Flee
    [System.NonSerialized] public bool attackerAccept = false;
    [System.NonSerialized] public bool defenderAccept = false;

    [System.NonSerialized] public List<string> redHand = new List<string>();
    [System.NonSerialized] public List<string> blueHand = new List<string>();
    [System.NonSerialized] public List<string> redTrade = new List<string>();
    [System.NonSerialized] public List<string> blueTrade = new List<string>();

    [System.NonSerialized] public string[] numberTrueValues = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13" };
    [System.NonSerialized] public string[] suitTrueValues = new string[] { "C", "D", "H", "S" };
    System.Random random = new System.Random();


    // Start is called before the first frame update
    void Start()
    {
        progressText = progressButton.GetComponentInChildren<TextMeshProUGUI>();
        secondaryText = secondaryButton.GetComponentInChildren<TextMeshProUGUI>();
        errorMessage = errorPopup.GetComponentInChildren<TextMeshProUGUI>();

        // Randomise True Values (their index+1 is their True Value)
        numberTrueValues = numberTrueValues.OrderBy(x => random.Next()).ToArray();
        suitTrueValues = suitTrueValues.OrderBy(x => random.Next()).ToArray();

        // Begin with Red's turn to scan cards
        ReadyTurn(true, "<color=red>Red</color>", "Draw your 5 cards and scan them");
    }

    // The overall game process is handled here
    // Called whenever the progress/secondary button is pressed, progressing to the next turn/phase
    public void ProgressGame()
    {
        switch (phase)
        {
            // ----- Draw Phase --------------------------------------------------
            case Phases.DRAW:
                if (nextTurn == Turns.FIRSTREADY)
                {
                    if (blueHand.Count == 5) // Checks 5 cards have been scanned
                    {
                        // Set up for next phase (Trade)
                        phase = Phases.TRADE;
                        nextTurn = Turns.FIRSTTURN;
                        ReadyTurn(attackerColour, "Attacker", "Select which cards you wish to trade");
                    }
                    else
                    {
                        DisplayErrorPopup("All 5 cards belonging to <color=blue>Blue</color> must be scanned");
                    }
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    if (redHand.Count == 5)
                    {
                        nextTurn = Turns.SECONDTURN;
                        ReadyTurn(false, "<color=blue>Blue</color>", "Draw your 5 cards and scan them");
                    }
                    else
                    {
                        DisplayErrorPopup("All 5 cards belonging to <color=red>Red</color> must be scanned");
                    }
                }

                else
                {
                    if (nextTurn == Turns.FIRSTTURN)
                        nextTurn = Turns.SECONDREADY;
                    else
                        nextTurn = Turns.FIRSTREADY;

                    gameObject.SetActive(true);
                    cover.gameObject.SetActive(false);
                    infoText.gameObject.SetActive(true);
                    SwapButtonLayout(false, "Reset", "Confirm");
                }
                break;

            // ----- Trade Phase --------------------------------------------------
            case Phases.TRADE:
                if (nextTurn == Turns.FIRSTREADY)
                {
                    if (defenderAccept && redTrade.Count > 0) // Trade accepted if there is one
                    {
                        for (int i = 0; i < redTrade.Count; i++)
                        {
                            // Move card(s) from Red to Blue
                            redHand.Remove(redTrade[i]);
                            blueHand.Add(redTrade[i]);

                            // Move card(s) from Blue to Red
                            redHand.Add(blueTrade[i]);
                            blueHand.Remove(blueTrade[i]);
                        }
                    }

                    // Set up next phase (Fight-Or-Flee)
                    phase = Phases.FIGHTORFLEE;
                    nextTurn = Turns.FIRSTTURN;
                    ReadyTurn(attackerColour, "Attacker", "Decide to <b>fight</b> or to <b>flee</b>");
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    if (redTrade.Count == blueTrade.Count) // Ensures a valid trade (equal number of cards on both sides being traded)
                    {
                        nextTurn = Turns.SECONDTURN;
                        ReadyTurn(defenderColour, "Defender", "Do you <b>accept</b> or <b>decline</b> the trade?");
                    }
                    else
                    {
                        DisplayErrorPopup("Number of selected cards must be equal on both sides");
                    }
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
                    cover.gameObject.SetActive(false);
                }
                break;

            // ----- Fight-Or-Flee Phase --------------------------------------------------
            case Phases.FIGHTORFLEE:
                if (nextTurn == Turns.FIRSTREADY)
                {
                    phase = Phases.RESULTS;

                    if (attackerAccept && defenderAccept) // Both chose to Fight
                    {
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
                            DisplayResults("<color=red>Red</color> wins!", Color.red);
                        else if (blueSum > redSum)
                            DisplayResults("<color=blue>Blue</color> wins!", Color.blue);
                        else
                            DisplayResults("Draw!", Color.white);
                    }
                    else if (round == 3) // If it's the last round but someone still Flees
                    {
                        DisplayResults("No Contest!", Color.white);
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
                        redTrade.Clear();
                        blueTrade.Clear();

                        DisplayResults();
                    }
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    nextTurn = Turns.SECONDTURN;
                    ReadyTurn(defenderColour, "Defender", "Do you <b>fight</b> or <b>flee</b> in response?");
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

                        string colour = "blue";
                        if (attackerColour)
                            colour = "red";

                        string choice = "flee";
                        if (attackerAccept)
                            choice = "fight";

                        infoText.text = "<color=" + colour + ">Attacker</color> chose <b>" + choice + "</b>";
                        infoText.gameObject.SetActive(true);
                    }

                    gameObject.SetActive(true);
                    cover.gameObject.SetActive(false);
                    SwapButtonLayout(false, "Fight", "Flee");
                }
                break;

            // ----- Results Phase --------------------------------------------------
            // Purpose of this phase is just to display the results of the round/match
            // Also to disable the current 10 cards in play to prevent rescanning
            case Phases.RESULTS:
                // Set up next phase (Draw)
                phase = Phases.DRAW;
                nextTurn = Turns.FIRSTTURN;
                phaseDisplay.gameObject.SetActive(true);
                roleDisplay.gameObject.SetActive(true);
                ReadyTurn(true, "Red", "Draw your 5 cards and scan them");
                break;
        }
    }

    // Using the secondary button, prepare any 
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
            ProgressGame();
        }
    }

    // Swap between the single-button layout and the dual-button layout
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

    // Enables the ready screen where the mobile device is to be passed to the mentioned colour (player)
    void ReadyTurn(bool colour, string turn, string instructions)
    {
        gameObject.SetActive(false); // Disables scanning
        infoText.gameObject.SetActive(false);
        SwapButtonLayout(true, "-", "Ready");

        if (colour)
        {
            turnText.text = "<color=red>" + turn + "</color>";
            cover.color = Color.red;
            roleDisplay.color = Color.red;
        }
        else
        {
            turnText.text = "<color=blue>" + turn + "</color>";
            cover.color = Color.blue;
            roleDisplay.color = Color.blue;
        }

        if (colour && attackerColour) // If the colour passed in is the Attacker, display sword
            roleDisplay.texture = roleIcons[0];
        else // Otherwise, display shield
            roleDisplay.texture = roleIcons[1];

        phaseDisplay.texture = phaseIcons[(int)phase];
        instructionsText.text = instructions;
        cover.gameObject.SetActive(true);
    }

    // Enables the results screen which is to be shown to both players
    // - Next round
    void DisplayResults()
    {
        gameObject.SetActive(false);
        infoText.gameObject.SetActive(false);
        phaseDisplay.gameObject.SetActive(false);
        roleDisplay.gameObject.SetActive(false);

        SwapButtonLayout(true, "-", "Next Round");
        turnText.text = "Someone fled";
        instructionsText.text = "Proceed to the next round";
        cover.color = Color.white;
        cover.gameObject.SetActive(true);
    }

    // - End of match
    void DisplayResults(string result, Color colour)
    {
        gameObject.SetActive(false);
        infoText.gameObject.SetActive(false);
        phaseDisplay.gameObject.SetActive(false);
        roleDisplay.gameObject.SetActive(false);

        // Disable progress buttons as the match has ended
        progressButton.gameObject.SetActive(false);
        secondaryButton.gameObject.SetActive(false);

        turnText.text = result;
        instructionsText.text = "Return to the Main Menu";
        cover.color = colour;
        cover.gameObject.SetActive(true);
    }

    // Display an error popup if any issue prevents game progression
    public void DisplayErrorPopup(string error)
    {
        errorMessage.text = error;
        errorPopup.gameObject.SetActive(true);
    }

    // "OK" button closes the error popup
    public void CloseErrorPopup()
    {
        errorPopup.gameObject.SetActive(false);
    }
}
