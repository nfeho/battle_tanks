using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotProfile
{
    public string profileName;
    public float hideChance;
    public float coverMateChance;
    public float attackChance;
    public int hideDuration;
    public int nearbyExploringDuration;


    // Data class for storing bot profiles
    public BotProfile(string profileName, float hideChance, float coverMateChance, float attackChance, int hideDuration, int nearbyExploringDuration)
    {
        this.profileName = profileName;
        this.hideChance = hideChance;
        this.coverMateChance = coverMateChance;
        this.attackChance = attackChance;
        this.hideDuration = hideDuration;
        this.nearbyExploringDuration = nearbyExploringDuration;
    }
}
