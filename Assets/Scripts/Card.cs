using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Card : MonoBehaviour
{
    public CardGame cardGame;
    string cardName, number, suit;
    TextMesh trueValueText;

    // Start is called before the first frame update
    void Start()
    {
        cardName = gameObject.name;
        number = cardName.Substring(0, 2);
        suit = cardName.Substring(2, 1);
        trueValueText = gameObject.transform.GetChild(0).GetComponent<TextMesh>();
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
        cardGame.currentCardsScannedText.text = ++cardGame.currentCardsScanned + "/5 Cards";
        if ((int)cardGame.phase == 0) // Draw Phase
        {
            int trueValue = Array.IndexOf(cardGame.numberTVs, number) + Array.IndexOf(cardGame.suitTVs, suit) + 2;
            trueValueText.text = trueValue.ToString();
            if (cardGame.turn < 0) // If Red's turn, assign owner to Red, reveal to opponent Blue
            {
                cardGame.cardsDict[cardName] = "RB";
                trueValueText.color = Color.red;
            }
            else // If Blue's turn, assign owner to Blue, reveal to opponent Red
            {
                cardGame.cardsDict[cardName] = "BR";
                trueValueText.color = Color.blue;
            }
        }
    }

    public void ScanLost()
    {
        cardGame.currentCardsScannedText.text = --cardGame.currentCardsScanned + "/5 Cards";
    }
}
