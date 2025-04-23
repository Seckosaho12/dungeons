using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPick : Singleton<CoinPick>
{
    public void AddOneCoin()
    {
        CoinCounter.Instance.coinAmount++;
        AudioManager.instance.PlayCoinSFX();
    }
}
