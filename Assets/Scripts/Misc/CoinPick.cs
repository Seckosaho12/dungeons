using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPick : Singleton<CoinPick>
{
    public CoinCounter cm;
    public void AddOneCoin()
    {
        cm.coinAmount++;
        AudioManager.instance.PlayCoinSFX();
    }
}
