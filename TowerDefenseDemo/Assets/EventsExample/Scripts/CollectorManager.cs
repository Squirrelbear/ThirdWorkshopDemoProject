using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CollectorManager : MonoBehaviour
{
    // Based on code from: https://game.courses/c-events-vs-unityevents/?ref=15&utm_source=youtube&utm_medium=description&utm_campaign=unityevents

    public TMP_Text collectibleNumberText;
    public string includeObjName;
    public List<Collectible> collectibleList;
    public UnityEvent OnAllCollected;
    public List<Collectible> remainingToCollect;

    void OnEnable()
    {
        remainingToCollect = new List<Collectible>(collectibleList);

        foreach(var collectible in remainingToCollect)
        {
            collectible.OnPickup += HandlePickup;
            collectible.gameObject.SetActive(false);
        }

        UpdateText();
    }

    void HandlePickup(Collectible collectible)
    {
        remainingToCollect.Remove(collectible);
        UpdateText();

        if(remainingToCollect.Count == 0)
        {
            OnAllCollected.Invoke();
        }
    }

    void UpdateText()
    {
        collectibleNumberText.SetText($"Remaining: {remainingToCollect.Count}");
        collectibleNumberText.enabled = remainingToCollect.Count > 0;
    }

    public void showCollectibles()
    {
        foreach (var collectible in remainingToCollect)
        {
            collectible.gameObject.SetActive(true);
        }
        UpdateText();
    }

    [ContextMenu("AutoFill Collectibles")]
    void AutoFillCollectibles()
    {
        collectibleList = GetComponentsInChildren<Collectible>()
            .Where(t => t.name.ToLower().Contains(includeObjName.ToLower()))
            .ToList();
    }
}
