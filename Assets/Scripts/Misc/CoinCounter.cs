using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinCounter : MonoBehaviour
{
     public float coinAmount;
    public Text TextOfCoin;
    private void Update()
    {
        GetComponent<Text>();
        TextOfCoin.text = coinAmount.ToString();
    }
}
