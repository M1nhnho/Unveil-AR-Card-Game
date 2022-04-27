using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Vuforia;

public class Card : MonoBehaviour
{
    public CardGame cardGame;
    string cardName, number, suit;
    string reveal = "?";
    TextMesh trueValueText;
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
        trueValueText = gameObject.transform.GetChild(0).GetComponent<TextMesh>();

        //tradeSelectButton.RegisterOnButtonPressed(TradeSelect);
        selectGlow = gameObject.GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Number always faces camera
        Vector3 cardPos = transform.position;
        Vector3 camPos = Camera.main.transform.position;
        cardPos.y = 0; // y rotation locked so number stays vertical
        camPos.y = 0;
        trueValueText.transform.rotation = Quaternion.LookRotation(cardPos - camPos);
    }

    public void ScanFound()
    {
        switch (cardGame.phase)
        {
            case Phases.DRAW:
                if (reveal == "?") // If card hasn't been scanned already
                {
                    if (cardGame.turn < 0 && cardGame.redHand.Count < 5) // If Red's turn, reveal to opponent Blue; provided Red has < 5 cards
                        AddCardToHand(true);
                    else if (cardGame.turn >= 0 && cardGame.blueHand.Count < 5) // If Blue's turn, reveal to opponent Red; provided Blue has < 5 cards
                        AddCardToHand(false);
                }
                else
                {
                    trueValueText.text = "X";
                }
                break;


            case Phases.TRADE:
                bool colour;
                if (cardGame.turn < 0)
                    colour = cardGame.attackerColour;
                else
                    colour = cardGame.defenderColour;

                if ((colour && cardGame.blueHand.Contains(cardName)) || (!colour && cardGame.redHand.Contains(cardName)))
                {
                    int trueValue = Array.IndexOf(cardGame.numberTrueValues, number) + Array.IndexOf(cardGame.suitTrueValues, suit) + 2; // +2 due to both arrays starting at 0
                    trueValueText.text = trueValue.ToString();
                }
                else
                {
                    trueValueText.text = "?";
                }
                break;

        }
    }

    void AddCardToHand(bool colour)
    {
        if (colour)
        {
            reveal = "B";
            cardGame.redHand.Add(cardName);
            trueValueText.color = Color.red;
        }
        else
        {
            reveal = "R";
            cardGame.blueHand.Add(cardName);
            trueValueText.color = Color.blue;
        }

        scanned = true;
        cardGame.currentCardsScannedText.text = ++cardGame.currentCardsScanned + "/5 Cards";
    }

    public void ScanLost()
    {
        if (scanned) // By default the scan is already lost so this makes it so it only decrements if the card was scanned first
        {
            cardGame.currentCardsScannedText.text = --cardGame.currentCardsScanned + "/5 Cards";
            scanned = false;
            if (cardGame.phase == Phases.DRAW && (cardGame.turn == -1 || cardGame.turn == 1))
            {
                reveal = "?";
                trueValueText.color = Color.white;
                if (cardGame.turn < 0)
                    cardGame.redHand.Remove(cardName);
                else
                    cardGame.blueHand.Remove(cardName);
            }
        }
    }

    /*
    public void TradeSelect(VirtualButtonBehaviour button)
    {
        trueValueText.text = "YES";
        selected = !selected;
        selectGlow.gameObject.SetActive(selected);
    }
    */
}
