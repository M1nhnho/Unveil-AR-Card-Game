using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CardGame : MonoBehaviour
{
    // TV = True Value
    [HideInInspector] public string[] numberTVs = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13" };
    [HideInInspector] public string[] suitTVs = new string[] { "C", "D", "H", "S" };
    [HideInInspector] public Dictionary<string, string> cardsDict = new Dictionary<string, string>()
    {
        { "01C", "BB" }, { "01D", "BB" }, { "01H", "BB" }, { "01S", "BB" }, // [B]lue, [R]ed
        { "02C", "BB" }, { "02D", "BB" }, { "02H", "BB" }, { "02S", "BB" }, // First character refers to who it is revealed to
        { "03C", "BB" }, { "03D", "BB" }, { "03H", "BB" }, { "03S", "BB" }, // Second character refers to ownership
        { "04C", "BB" }, { "04D", "BB" }, { "04H", "BB" }, { "04S", "BB" },
        { "05C", "BB" }, { "05D", "BB" }, { "05H", "BB" }, { "05S", "BB" },
        { "06C", "BB" }, { "06D", "BB" }, { "06H", "BB" }, { "06S", "BB" },
        { "07C", "BB" }, { "07D", "BB" }, { "07H", "BB" }, { "07S", "BB" },
        { "08C", "BB" }, { "08D", "BB" }, { "08H", "BB" }, { "08S", "BB" },
        { "09C", "BB" }, { "09D", "BB" }, { "09H", "BB" }, { "09S", "BB" },
        { "10C", "BB" }, { "10D", "BB" }, { "10H", "BB" }, { "10S", "BB" },
        { "11C", "BB" }, { "11D", "BB" }, { "11H", "BB" }, { "11S", "BB" },
        { "12C", "BB" }, { "12D", "BB" }, { "12H", "BB" }, { "12S", "BB" },
        { "13C", "BB" }, { "13D", "BB" }, { "13H", "BB" }, { "13S", "BB" }
    };
    System.Random random = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        numberTVs = numberTVs.OrderBy(x => random.Next()).ToArray();
        suitTVs = suitTVs.OrderBy(x => random.Next()).ToArray();
    }

    public void Scan()
    {
        gameObject.SetActive(true);
    }
}
