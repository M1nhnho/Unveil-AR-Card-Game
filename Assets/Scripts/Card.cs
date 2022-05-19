using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Vuforia;

public class Card : MonoBehaviour, IPointerDownHandler
{
    public CardGame cardGame;

    string cardName; // 'cardName' as 'name' is already used by Unity
    TextMeshPro trueValueText;
    char revealTo = '?';
    bool drawed = false; // Refers to scanning during the Draw phase
    SpriteRenderer selectGlow;
    bool selected = false;

    // Start is called before the first frame update
    void Start()
    {
        // All cards names follow the standard XXY
        // - XX represents the card rank as a number (e.g. King as 13)
        // - Y represents the suit as its initial (e.g. Spades as S)
        cardName = gameObject.name;
        trueValueText = gameObject.GetComponentInChildren<TextMeshPro>();
        selectGlow = gameObject.GetComponentInChildren<SpriteRenderer>();
        selectGlow.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // TV always faces camera
        Vector3 trueValuePosition = trueValueText.transform.position;
        Vector3 cameraPosition = Camera.main.transform.position;
        trueValuePosition.y = 0; cameraPosition.y = 0; // Keeps TV level (vertical) as this makes it not consider the difference in height
        trueValueText.transform.rotation = Quaternion.LookRotation(trueValuePosition - cameraPosition);
    }

    // Called whenever a card is scanned
    public void ScanFound()
    {
        switch (cardGame.phase)
        {
            case Phases.DRAW:
                if (revealTo == '?') // If the card hasn't been scanned and registered before
                {
                    if (cardGame.nextTurn == Turns.SECONDREADY && cardGame.redHand.Count < cardGame.cardsInHand) // During Red's turn and limit of 3 card scan registers
                        AddCardToHand(true); // Registers to Red Hand
                    else if (cardGame.nextTurn == Turns.FIRSTREADY && cardGame.blueHand.Count < cardGame.cardsInHand && !cardGame.redHand.Contains(cardName)) // During Blue's turn and limit of 3 card scan registers (provided they're not Red's)
                        AddCardToHand(false); // Registers to Blue Hand
                }
                break;


            case Phases.TRADE:
                // Only enables cards currently in play to be selected
                if (revealTo == 'R' || revealTo == 'B')
                    selectGlow.gameObject.SetActive(true);

                RevealTrueValues();
                break;


            case Phases.FIGHTORFLEE:
                selectGlow.gameObject.SetActive(false); // Disables selection as Trade phase is over
                RevealTrueValues();
                break;
        }
    }

    // Called whenever the card is either disabled or lost from sight
    public void ScanLost()
    {
        // By default the scan is already lost so this makes it so it only decrements if the card was scanned first
        if (drawed) // 'drawed' can only be true during Draw phase
        {
            drawed = false;
            cardGame.infoText.text = $"{--cardGame.currentCardsScanned}/{cardGame.cardsInHand} Cards"; // '--' before to decrement first then update text

            // Reset card if the scan's lost during a turn
            if (cardGame.nextTurn == Turns.FIRSTREADY || cardGame.nextTurn == Turns.SECONDREADY)
            {
                revealTo = '?';
                trueValueText.color = Color.white;
                if (cardGame.nextTurn == Turns.SECONDREADY)
                    cardGame.redHand.Remove(cardName);
                else
                    cardGame.blueHand.Remove(cardName);
            }
        }

        // Update the owners (colour of TV) after an accepted trade
        if (cardGame.phase == Phases.FIGHTORFLEE && cardGame.redTrueSumTraded > 0)
        {
            if (cardGame.redTrade.Contains(cardName))
                trueValueText.color = Color.blue;
            else if (cardGame.blueTrade.Contains(cardName))
                trueValueText.color = Color.red;
        }

        // At the end of each round, the current 10 cards in play are disabled to prevent rescanning
        if (cardGame.phase == Phases.RESULTS)
        {
            revealTo = 'X';
            trueValueText.text = "X";
        }
    }

    // During the Draw phase, add the scanned card to the appropriate hand
    void AddCardToHand(bool colour)
    {
        if (colour) // If Red
        {
            revealTo = 'B'; // Allow the opponent (Blue) to see the TV
            trueValueText.color = Color.red; // Red text denotes Red's ownership
            cardGame.redHand.Add(cardName);
        }
        else // If Blue
        {
            revealTo = 'R';
            trueValueText.color = Color.blue;
            cardGame.blueHand.Add(cardName);
        }

        // Card has been successfully registered to the appropriate hand
        drawed = true;
        cardGame.infoText.text = $"{++cardGame.currentCardsScanned}/{cardGame.cardsInHand} Cards";
    }

    // During the Trade phase, allow the Attacker to select cards for trade
    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectGlow.gameObject.activeInHierarchy && cardGame.nextTurn == Turns.SECONDREADY) // Attacker can only select current cards in play
        {
            selected = !selected;
            if (selected)
            {
                // When selected, change the glow to reflect the current owner and register as up for trade (not yet actually traded as Defender needs to accept)
                if (cardGame.redHand.Contains(cardName))
                {
                    selectGlow.color = Color.red;
                    cardGame.redTrade.Add(cardName);
                }
                else if (cardGame.blueHand.Contains(cardName))
                {
                    selectGlow.color = Color.blue;
                    cardGame.blueTrade.Add(cardName);
                }
            }
            else
            {
                // Reset card if it's unselected
                selectGlow.color = Color.white; // White denotes unselected
                if (cardGame.redHand.Contains(cardName))
                    cardGame.redTrade.Remove(cardName);
                else if (cardGame.blueHand.Contains(cardName))
                    cardGame.blueTrade.Remove(cardName);
            }
        }
    }

    // Updates the text above the card to either hide or reveal their TV
    void RevealTrueValues()
    {
        // Gets the colour of the current turn
        bool colour;
        if (cardGame.nextTurn == Turns.SECONDREADY)
            colour = cardGame.attackerColour;
        else
            colour = cardGame.defenderColour;

        // Reveals TV if the card belongs to the opponent (or originally did after a trade as 'revealTo' remains unchanged)
        if ((colour && revealTo == 'R') || (!colour && revealTo == 'B')) // If the colour (player) of the current turn matches the card's 'revealTo'
        {
            int trueValue = cardGame.GetTrueValue(cardName);
            trueValueText.text = trueValue.ToString();
        }
        // Otherwise, hide TV (this only changes the currect cards in play)
        else if (revealTo == 'R' || revealTo == 'B')
        {
            trueValueText.text = "?";
        }
    }
}
