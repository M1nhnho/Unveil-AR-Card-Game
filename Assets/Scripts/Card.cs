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

    string cardName, number, suit;
    TextMeshPro trueValueText;
    char revealTo = '?';
    bool drawed = false; // Refers to scanning during the Draw phase
    SpriteRenderer selectGlow;
    bool selected = false;

    // Start is called before the first frame update
    void Start()
    {
        cardName = gameObject.name;
        number = cardName.Substring(0, 2);
        suit = cardName.Substring(2, 1);
        trueValueText = gameObject.GetComponentInChildren<TextMeshPro>();
        selectGlow = gameObject.GetComponentInChildren<SpriteRenderer>();
        selectGlow.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // True Value always faces camera
        Vector3 trueValuePosition = trueValueText.transform.position;
        Vector3 cameraPosition = Camera.main.transform.position;
        trueValuePosition.y = 0; cameraPosition.y = 0; // Keeps True Value level as this makes it not consider the difference in height
        trueValueText.transform.rotation = Quaternion.LookRotation(trueValuePosition - cameraPosition);
    }

    // Called whenever a card is scanned
    public void ScanFound()
    {
        switch (cardGame.phase)
        {
            case Phases.DRAW:
                if (revealTo == '?') // If card hasn't been scanned already
                {
                    if (cardGame.nextTurn == Turns.SECONDREADY && cardGame.redHand.Count < 5)
                        AddCardToHand(true); // Add to Red Hand
                    else if (cardGame.nextTurn == Turns.FIRSTREADY && cardGame.blueHand.Count < 5 && !cardGame.redHand.Contains(cardName))
                        AddCardToHand(false); // Add to Blue Hand
                }
                break;


            case Phases.TRADE:
                // Only enables cards currently in play to be selected
                if (revealTo == 'R' || revealTo == 'B')
                    selectGlow.gameObject.SetActive(true);

                RevealTrueValues();
                break;


            case Phases.FIGHTORFLEE:
                selectGlow.gameObject.SetActive(false);
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
            cardGame.infoText.text = --cardGame.currentCardsScanned + "/5 Cards";

            // Reset card if scan lost during turn
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

        // Update the owners (colour of True Value) after an accepted trade
        if (cardGame.phase == Phases.FIGHTORFLEE && cardGame.defenderAccept)
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
            revealTo = 'B'; // Allow opponent (Blue) to see the True Value
            trueValueText.color = Color.red; // Red text denotes Red's ownership
            cardGame.redHand.Add(cardName);
        }
        else // If Blue
        {
            revealTo = 'R';
            trueValueText.color = Color.blue;
            cardGame.blueHand.Add(cardName);
        }

        // Card has been successfully scanned and added to the appropriate hand
        drawed = true;
        cardGame.infoText.text = ++cardGame.currentCardsScanned + "/5 Cards";
    }

    // During the Trade phase, allow the Attacker to select cards for trade
    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectGlow.gameObject.activeInHierarchy && cardGame.nextTurn == Turns.SECONDREADY) // Attacker can only select current cards in play
        {
            selected = !selected;
            if (selected)
            {
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
                selectGlow.color = Color.white;
                if (cardGame.redHand.Contains(cardName))
                    cardGame.redTrade.Remove(cardName);
                else if (cardGame.blueHand.Contains(cardName))
                    cardGame.blueTrade.Remove(cardName);
            }
        }
    }

    // Updates the text above the card to either hide or reveal their True Value
    void RevealTrueValues()
    {
        // Gets the colour of the current turn
        bool colour;
        if (cardGame.nextTurn == Turns.SECONDREADY)
            colour = cardGame.attackerColour;
        else
            colour = cardGame.defenderColour;

        // Reveals True Value if the card (originally) belonged to the opponent
        if ((colour && revealTo == 'R') || (!colour && revealTo == 'B'))
        {
            int trueValue = Array.IndexOf(cardGame.numberTrueValues, number) + Array.IndexOf(cardGame.suitTrueValues, suit) + 2; // +2 due to both arrays starting at 0
            trueValueText.text = trueValue.ToString();
        }
        // Otherwise, hide True Value (this only changes the currect cards in play)
        else if (revealTo == 'R' || revealTo == 'B')
        {
            trueValueText.text = "?";
        }
    }
}
