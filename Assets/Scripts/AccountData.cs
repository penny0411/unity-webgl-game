using System;
using UnityEngine;

[System.Serializable]
public class AccountData
{
    public string userId;
    public string playerName;
    public string characterName;

    public string gender;
    public int age;
    public string personality;

    public AccountData()
    {
        userId = System.Guid.NewGuid().ToString();
    }
}