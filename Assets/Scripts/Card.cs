using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Card : MonoBehaviour
{
    public CardGame cardGame;
    string cardName, number, suit;
    Transform trueValueObj;

    // Start is called before the first frame update
    void Start()
    {
        cardName = gameObject.name;
        number = cardName.Substring(0, 2);
        suit = cardName.Substring(2, 1);
        trueValueObj = gameObject.transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cardPos = transform.position;
        Vector3 camPos = Camera.main.transform.position;
        cardPos.y = 0;
        camPos.y = 0;
        trueValueObj.transform.rotation = Quaternion.LookRotation(cardPos - camPos);
    }

    public void UpdateCard()
    {
        int trueValue = Array.IndexOf(cardGame.numberTVs, number) + Array.IndexOf(cardGame.suitTVs, suit) + 2;
        trueValueObj.GetComponent<TextMesh>().text = trueValue.ToString();
    }
}
