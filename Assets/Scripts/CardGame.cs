using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public enum Phases { DRAW, TRADE, FIGHTORFLEE, RESULTS }
public enum Turns { FIRSTREADY, FIRSTTURN, SECONDREADY, SECONDTURN } // 'READY' refers to the ready screen in between turns
                                                                     // 'TURN' refers to the actual turn where scanning is enabled

public class CardGame : MonoBehaviour
{
    // Game objects to hook up in inspector
    public RawImage cover;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI infoText;
    public Transform progressButton;
    TextMeshProUGUI progressText;
    public Transform secondaryButton;
    TextMeshProUGUI secondaryText;
    public Transform historyButton;
    public Transform revealedToRedHistory;
    public Transform revealedToBlueHistory;
    public RawImage errorPopup;
    TextMeshProUGUI errorMessage;
    public TextMeshProUGUI roundDisplay;
    public RawImage phaseDisplay;
    public Texture[] phaseIcons = new Texture[3]; // Cards, Handshake, Lightning
    public RawImage roleDisplay;
    public Texture[] roleIcons = new Texture[2]; // Sword, Shield

    // [System.NonSerialized] hides in inspector and allows modification within the script
    // - Public variables default can be changed in the inspector, but defaults to the first value given within scripts
    // - E.g. if I change 'round' to 2, it will still be considered as 1 
    // Public to give Card.cs access

    [System.NonSerialized] public Phases phase = Phases.DRAW; // The current phase
    [System.NonSerialized] public Turns nextTurn = Turns.FIRSTTURN; // Next turn as the progress buttons are at the end of a turn and needs to know what to move to
    [System.NonSerialized] public int round = 1;
    [System.NonSerialized] public int currentCardsScanned = 0; // During the Draw phase, to let the player know how many cards have been scanned and registered
    bool revealHistory = false;

    // There is a Red player and a Blue player, any reference to colour refers to the player
    // bool for colour: true = Red, false = Blue
    [System.NonSerialized] public bool attackerColour = true; // Red is Attacker first
    [System.NonSerialized] public bool defenderColour = false; // Blue is Defender first
    // If they choose to accept/decline trade or to Fight/Flee (Fight being true)
    [System.NonSerialized] public bool attackerAccept = false;
    [System.NonSerialized] public bool defenderAccept = false;

    [System.NonSerialized] public int cardsInHand = 4; // How many cards are in a hand
    [System.NonSerialized] public List<string> redHand = new List<string>();
    [System.NonSerialized] public List<string> blueHand = new List<string>();
    int redTrueSum = 0;
    int blueTrueSum = 0;
    // During the Trade phase, the cards selected for trade
    [System.NonSerialized] public List<string> redTrade = new List<string>();
    [System.NonSerialized] public List<string> blueTrade = new List<string>();
    // For the display of the opponent's true sum minus the below (provided the trade was accepted)
    [System.NonSerialized] public int redTrueSumTraded = 0; // Public to give 'Card.cs' access to know whether a trade has occurred (either variable works)
    int blueTrueSumTraded = 0;

    // In comments, True Values will be shortened to TVs - variable names will stay as the full version for clarity
    // True sum refers to the total of all the TVs of a hand
    // TVs calculated through the index so the order will be randomised
    [System.NonSerialized] public string[] rankTrueValues = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13" };
    [System.NonSerialized] public string[] suitTrueValues = new string[] { "C", "D", "H", "S" };
    System.Random random = new System.Random();

    // To translate the card names to the physical ranks and suits for better readability (used for the history)
    Dictionary<string, string> cardNameTranslations = new Dictionary<string, string>()
    {
        { "01C", "A♣" }, { "01D", "A♦" }, { "01H", "A♥" }, { "01S", "A♠" },
        { "02C", "2♣" }, { "02D", "2♦" }, { "02H", "2♥" }, { "02S", "2♠" },
        { "03C", "3♣" }, { "03D", "3♦" }, { "03H", "3♥" }, { "03S", "3♠" },
        { "04C", "4♣" }, { "04D", "4♦" }, { "04H", "4♥" }, { "04S", "4♠" },
        { "05C", "5♣" }, { "05D", "5♦" }, { "05H", "5♥" }, { "05S", "5♠" },
        { "06C", "6♣" }, { "06D", "6♦" }, { "06H", "6♥" }, { "06S", "6♠" },
        { "07C", "7♣" }, { "07D", "7♦" }, { "07H", "7♥" }, { "07S", "7♠" },
        { "08C", "8♣" }, { "08D", "8♦" }, { "08H", "8♥" }, { "08S", "8♠" },
        { "09C", "9♣" }, { "09D", "9♦" }, { "09H", "9♥" }, { "09S", "9♠" },
        { "10C", "10♣"}, { "10D", "10♦"}, { "10H", "10♥"}, { "10S", "10♠"},
        { "11C", "J♣" }, { "11D", "J♦" }, { "11H", "J♥" }, { "11S", "J♠" },
        { "12C", "Q♣" }, { "12D", "Q♦" }, { "12H", "Q♥" }, { "12S", "Q♠" },
        { "13C", "K♣" }, { "13D", "K♦" }, { "13H", "K♥" }, { "13S", "K♠" },
    };



    // Start is called before the first frame update
    void Start()
    {
        progressText = progressButton.GetComponentInChildren<TextMeshProUGUI>();
        secondaryText = secondaryButton.GetComponentInChildren<TextMeshProUGUI>();
        errorMessage = errorPopup.GetComponentInChildren<TextMeshProUGUI>();

        // Randomise TVs every match
        rankTrueValues = rankTrueValues.OrderBy(x => random.Next()).ToArray();
        suitTrueValues = suitTrueValues.OrderBy(x => random.Next()).ToArray();

        // Begin with Red's turn to scan their hand
        ReadyTurn(true, "Red", $"Scan your hand of {cardsInHand} cards");
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
                    if (blueHand.Count == cardsInHand) // Checks 4 cards have been scanned
                    {
                        // Records the current round's TV reveals to the appropriate history
                        // - Uses the child order to place into the correct round (child 0 is the title)
                        TextMeshProUGUI redRevealsText = revealedToRedHistory.GetChild(round).GetComponent<TextMeshProUGUI>();
                        TextMeshProUGUI blueRevealsText = revealedToBlueHistory.GetChild(round).GetComponent<TextMeshProUGUI>();
                        redRevealsText.text = $"<u><b>Round {round}</b></u>";
                        blueRevealsText.text = $"<u><b>Round {round}</b></u>";
                        for (int i = 0; i < cardsInHand; i++)
                        {
                            redRevealsText.text += $"\n{cardNameTranslations[blueHand[i]]}→{GetTrueValue(blueHand[i].Substring(0, 2), blueHand[i].Substring(2, 1))}";
                            blueRevealsText.text += $"\n{cardNameTranslations[redHand[i]]}→{GetTrueValue(redHand[i].Substring(0, 2), redHand[i].Substring(2, 1))}";
                        }

                        // Set up for next phase (Trade)
                        phase = Phases.TRADE;
                        nextTurn = Turns.FIRSTTURN;
                        ReadyTurn(attackerColour, "Attacker", "Select the cards for trade");
                    }
                    else
                    {
                        DisplayErrorPopup($"All {cardsInHand} cards belonging to <color=blue>Blue</color> must be scanned");
                    }
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    if (redHand.Count == cardsInHand)
                    {
                        nextTurn = Turns.SECONDTURN;
                        ReadyTurn(false, "Blue", $"Scan your hand of {cardsInHand} cards");
                    }
                    else
                    {
                        DisplayErrorPopup($"All {cardsInHand} cards belonging to <color=red>Red</color> must be scanned");
                    }
                }

                else // If moving onto the actual turns
                {
                    if (nextTurn == Turns.FIRSTTURN)
                        nextTurn = Turns.SECONDREADY;
                    else
                        nextTurn = Turns.FIRSTREADY;

                    gameObject.SetActive(true); // Enable scanning
                    cover.gameObject.SetActive(false); // Close ready screen
                    infoText.gameObject.SetActive(true); // Number of cards currently scanned and registered
                    SwapButtonLayout(false, "Reset", "Confirm"); // 'Reset' to reset scanner if they misscan
                                                                 // 'Confirm' to lock in their hand
                }
                break;

            // ----- Trade Phase --------------------------------------------------
            case Phases.TRADE:
                if (nextTurn == Turns.FIRSTREADY)
                {
                    if (defenderAccept && redTrade.Count > 0) // If trade accepted provided there is one (cards have been selected)
                    {
                        // Trade the selected cards between the hands
                        for (int i = 0; i < redTrade.Count; i++) // Equal number of cards being traded so 'redTrade.Count' = 'blueTrade.Count'
                        {
                            // Move card(s) from Red to Blue
                            redHand.Remove(redTrade[i]);
                            blueHand.Add(redTrade[i]);

                            redTrueSumTraded += GetTrueValue(redTrade[i].Substring(0, 2), redTrade[i].Substring(2, 1));

                            // Move card(s) from Blue to Red
                            redHand.Add(blueTrade[i]);
                            blueHand.Remove(blueTrade[i]);

                            blueTrueSumTraded += GetTrueValue(blueTrade[i].Substring(0, 2), blueTrade[i].Substring(2, 1));
                        }
                        defenderAccept = false; // Reset as it now will be considered as Fleeing by default
                    }

                    // Calculate each hand's true sum
                    for (int i = 0; i < cardsInHand; i++)
                    {
                        redTrueSum += GetTrueValue(redHand[i].Substring(0, 2), redHand[i].Substring(2, 1));
                        blueTrueSum += GetTrueValue(blueHand[i].Substring(0, 2), blueHand[i].Substring(2, 1));
                    }

                    // Set up next phase (Fight-Or-Flee)
                    phase = Phases.FIGHTORFLEE;
                    nextTurn = Turns.FIRSTTURN;
                    ReadyTurn(attackerColour, "Attacker", "Choose <b>fight</b> or <b>flee</b>");
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    if (redTrade.Count == blueTrade.Count) // Ensures a valid trade (equal number of cards on both sides being traded)
                    {
                        nextTurn = Turns.SECONDTURN;
                        ReadyTurn(defenderColour, "Defender", "<b>Accept</b> or <b>decline</b> the trade");
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
                        SwapButtonLayout(true, "-", "Confirm"); // Confirm trade conditions
                    }
                    else
                    {
                        nextTurn = Turns.FIRSTREADY;
                        SwapButtonLayout(false, "Accept", "Decline"); // Accept or decline the trade
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
                        if (redTrueSum > blueTrueSum)
                            DisplayResults("<color=red>Red</color> wins!", Color.red);
                        else if (blueTrueSum > redTrueSum)
                            DisplayResults("<color=blue>Blue</color> wins!", Color.blue);
                        else
                            DisplayResults("Draw!", Color.white);
                    }
                    else if (round == 4) // If it's the last round but someone still Flees
                    {
                        DisplayResults("No contest!", Color.white);
                    }
                    else // Otherwise, next round
                    {
                        round++;
                        roundDisplay.text = round.ToString();

                        // Reset variables
                        infoText.text = $"0/{cardsInHand} Cards";
                        infoText.gameObject.SetActive(false);
                        attackerAccept = false;
                        defenderAccept = false;
                        redHand.Clear();
                        blueHand.Clear();
                        redTrueSum = 0;
                        blueTrueSum = 0;
                        redTrade.Clear();
                        blueTrade.Clear();
                        redTrueSumTraded = 0;
                        blueTrueSumTraded = 0;

                        // Swap roles
                        attackerColour = !attackerColour;
                        defenderColour = !defenderColour;

                        DisplayResults();
                    }
                }

                else if (nextTurn == Turns.SECONDREADY)
                {
                    nextTurn = Turns.SECONDTURN;
                    ReadyTurn(defenderColour, "Defender", "<b>Fight</b> or <b>flee</b> in response");
                }

                else
                {
                    if (nextTurn == Turns.FIRSTTURN)
                    {
                        nextTurn = Turns.SECONDREADY;
                        DisplayOpponentsTrueSum(attackerColour, true); // Pass in that it is the Attacker's turn, display Defender's hand's true sum
                    }
                    else
                    {
                        nextTurn = Turns.FIRSTREADY;
                        DisplayOpponentsTrueSum(defenderColour, false);
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
                // These three were disabled for the results screen so reenable for next round
                phaseDisplay.gameObject.SetActive(true);
                roleDisplay.gameObject.SetActive(true);
                historyButton.gameObject.SetActive(true);

                // Set up next phase (Draw)
                phase = Phases.DRAW;
                nextTurn = Turns.FIRSTTURN;
                ReadyTurn(true, "Red", $"Scan your hand of {cardsInHand} cards");
                break;
        }
    }


    // Using the secondary button, prepare anything needed before progression
    public void PrepareProgress()
    {
        if (phase == Phases.DRAW) // Reset scans if they mistakenly scan the wrong card(s) to ensure correct hand
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
        else if (phase == Phases.TRADE || phase == Phases.FIGHTORFLEE) // Stores choice to allow progress based on choice
        {
            // False by default - the button calling this method is 'Accept'/'Fight'
            if (nextTurn == Turns.FIRSTREADY)
                defenderAccept = true;
            else if (nextTurn == Turns.SECONDREADY)
                attackerAccept = true;
            ProgressGame();
        }
    }


    // Enables the ready screen where the mobile device is to be passed to the shown colour (player)
    void ReadyTurn(bool colour, string turn, string instructions)
    {
        gameObject.SetActive(false); // Disables scanning
        infoText.gameObject.SetActive(false);
        SwapButtonLayout(true, "-", "Ready");

        if (colour) // If Red
        {
            turnText.text = $"<color=red>{turn}</color>";
            cover.color = Color.red;
            roleDisplay.color = Color.red;
        }
        else // If Blue
        {
            turnText.text = $"<color=blue>{turn}</color>";
            cover.color = Color.blue;
            roleDisplay.color = Color.blue;
        }

        if (colour && attackerColour) // If the colour passed in is the Attacker, display sword
            roleDisplay.texture = roleIcons[0];
        else // Otherwise, display shield
            roleDisplay.texture = roleIcons[1];

        phaseDisplay.texture = phaseIcons[(int)phase]; // Change phase icon to reflect the current phase
        instructionsText.text = instructions;
        cover.gameObject.SetActive(true); // Enables ready screen
    }


    // Swap between the single-button layout and the dual-button layout along with text changes
    void SwapButtonLayout(bool singleButton, string secondary, string progress)
    {
        Vector3 progressPosition = progressButton.localPosition; // Can't directly change the x-coordinate so a copy is made
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
        progressButton.localPosition = progressPosition; // Update position with the correct x-coordinate
    }


    // Game objects that needs to be disabled/hidden for the results screen
    // A separate method just to avoid having to copy this twice for both 'DisplayResults()'
    void DisableForResults()
    {
        gameObject.SetActive(false);
        infoText.gameObject.SetActive(false);
        phaseDisplay.gameObject.SetActive(false);
        roleDisplay.gameObject.SetActive(false);
        historyButton.gameObject.SetActive(false);
    }

    // Enables the results screen which is to be shown to both players
    void DisplayResults() // Used when proceeding to the next round
    {
        DisableForResults();
        SwapButtonLayout(true, "-", "Next Round");
        turnText.text = "Someone fled";
        instructionsText.text = "Discard the current hands and redraw";
        cover.color = Color.white;
        cover.gameObject.SetActive(true);
    }
    void DisplayResults(string result, Color colour) // Used when the match has ended
    {
        DisableForResults();
        // Disable progress buttons as the match has ended
        progressButton.gameObject.SetActive(false);
        secondaryButton.gameObject.SetActive(false);

        turnText.text = result;
        instructionsText.text = "Return to the Main Menu";
        cover.color = colour; // Colour of the winner or default if it's a draw
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


    // Uses the index of the True Value arrays to calculate the TV and returns it
    public int GetTrueValue(string rank, string suit)
    {
        return Array.IndexOf(rankTrueValues, rank) + Array.IndexOf(suitTrueValues, suit) + 2; // +2 due to both arrays starting at 0
    }



    // TV reveals from passed rounds (including current) are recorded which the player can view
    public void ToggleRevealHistory()
    {
        revealHistory = !revealHistory;
        if (revealHistory)
        {
            if (nextTurn == Turns.FIRSTTURN || nextTurn == Turns.SECONDREADY) // If Red's turn
                revealedToRedHistory.gameObject.SetActive(true);
            else // If Blue's turn
                revealedToBlueHistory.gameObject.SetActive(true);
        }
        else
        {
            revealedToRedHistory.gameObject.SetActive(false);
            revealedToBlueHistory.gameObject.SetActive(false);
        }
    }


    // During the Fight-Or-Flee phase, display the opponent's hand's true sum to ease the calculation burden
    // - Note the *opponent* so code below will reflect that, displaying the 'opposite'
    void DisplayOpponentsTrueSum(bool red, bool attacker)
    {
        string roleText;
        string trueSum;

        if (attacker) // If the Attacker's turn, display Defender
        {
            roleText = "Defender</color> has ";
        }
        else // If the Defender's turn, also display Attacker's choice
        {
            string choice = "flee";
            if (attackerAccept)
                choice = "fight";
            roleText = $"Attacker</color> chose <b>{choice}</b> with ";
        }

        if (red) // If Red's turn, display Blue's hand's true sum
        {
            if (redTrueSumTraded == 0) // If a trade didn't occur (no cards selected or declined)
                                       // - In the case of above, both 'redTrueSumTraded' and 'redTrueSumTraded' would be 0 as they only change after a successful trade
                trueSum = $"<b>{blueTrueSum}</b>";
            else
                trueSum = $"<b>{blueTrueSum - blueTrueSumTraded}(+?)</b>";
            infoText.text = $"<color=blue>{roleText}{trueSum}";
            
        }
        else
        {
            if (redTrueSumTraded == 0) 
                trueSum = $"<b>{redTrueSum}</b>";
            else
                trueSum = $"<b>{redTrueSum - redTrueSumTraded}(+?)</b>";
            infoText.text = $"<color=red>{roleText}{trueSum}";
        }
        infoText.gameObject.SetActive(true);
    }
}
