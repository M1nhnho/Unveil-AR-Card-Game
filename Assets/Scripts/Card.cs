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
    string reveal = "?";
    TextMeshPro trueValueText;
    bool scanned = false;

    //public VirtualButtonBehaviour tradeSelectButton;
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
        trueValuePosition.y = 0; cameraPosition.y = 0; // Keeps True Value horizontal as this makes it not consider the difference in height
        trueValueText.transform.rotation = Quaternion.LookRotation(trueValuePosition - cameraPosition);
    }

    public void ScanFound()
    {
        switch (cardGame.phase)
        {
            case Phases.DRAW:
                if (reveal == "?") // If card hasn't been scanned already
                {
                    if (cardGame.nextTurn == Turns.SECONDREADY && cardGame.redHand.Count < 5)
                        AddCardToHand(true); // Add to Red Hand
                    else if (cardGame.nextTurn == Turns.FIRSTREADY && cardGame.blueHand.Count < 5 && !cardGame.redHand.Contains(cardName))
                        AddCardToHand(false); // Add to Blue Hand
                }
                break;


            case Phases.TRADE:
                // Only enables cards currently in play to be selected
                if (reveal == "R" || reveal == "B")
                    selectGlow.gameObject.SetActive(true);

                RevealTrueValues();
                break;


            case Phases.FIGHTORFLEE:
                selectGlow.gameObject.SetActive(false);
                RevealTrueValues();
                break;
        }
    }

    void AddCardToHand(bool colour)
    {
        if (colour)
        {
            reveal = "B";
            trueValueText.color = Color.red;
            cardGame.redHand.Add(cardName);
        }
        else
        {
            reveal = "R";
            trueValueText.color = Color.blue;
            cardGame.blueHand.Add(cardName);
        }

        scanned = true;
        cardGame.infoText.text = ++cardGame.currentCardsScanned + "/5 Cards";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Attacker can only select current cards in play
        if (selectGlow.gameObject.activeInHierarchy && cardGame.nextTurn == Turns.SECONDREADY)
        {
            selected = !selected;
            if (selected)
            {
                if (cardGame.redHand.Contains(cardName))
                {
                    selectGlow.color = Color.red;
                }
                else if (cardGame.blueHand.Contains(cardName))
                {
                    selectGlow.color = Color.blue;
                }
            }
            else
            {
                selectGlow.color = Color.black;
            }
        }
    }

    void RevealTrueValues()
    {
        // Gets which colour the current turn is
        bool colour;
        if (cardGame.nextTurn == Turns.SECONDREADY)
            colour = cardGame.attackerColour;
        else
            colour = cardGame.defenderColour;

        // Checks current colour and reveals opponent's True Values
        if ((colour && reveal == "R") || (!colour && reveal == "B"))
        {
            int trueValue = Array.IndexOf(cardGame.numberTrueValues, number) + Array.IndexOf(cardGame.suitTrueValues, suit) + 2; // +2 due to both arrays starting at 0
            trueValueText.text = trueValue.ToString();
        }
        else if (reveal == "R" || reveal == "B") // Otherwise, hide True Value (this only changes the currect cards in play)
        {
            trueValueText.text = "?";
        }
    }

    public void ScanLost()
    {
        // By default the scan is already lost so this makes it so it only decrements if the card was scanned first
        if (scanned)
        {
            scanned = false;
            cardGame.infoText.text = --cardGame.currentCardsScanned + "/5 Cards";
            
            // Reset card if scan lost during turn
            if (cardGame.phase == Phases.DRAW && (cardGame.nextTurn == Turns.FIRSTREADY || cardGame.nextTurn == Turns.SECONDREADY))
            {
                reveal = "?";
                trueValueText.color = Color.white;
                if (cardGame.nextTurn == Turns.SECONDREADY)
                    cardGame.redHand.Remove(cardName);
                else
                    cardGame.blueHand.Remove(cardName);
            }
        }

        // At the end of each round, the current 10 cards in play are disabled to prevent rescanning
        if (cardGame.phase == Phases.RESULTS)
        {
            reveal = "X";
            trueValueText.text = "X";
        }
    }
}
