// PartySpawner.cs
// Attach to an empty GameObject in your BattleTest scene. Assign a
// prefab for each of the 5 character classes, and 3 spawn point
// Transforms (empty GameObjects positioned where you want your units
// to stand). On scene load, it instantiates PartySelection.ChosenParty
// at those points and fills BattleManager's Player Units list itself -
// you no longer need to manually drag player units into BattleManager.
//
// Runs in Awake() so it finishes BEFORE BattleManager's Start() begins -
// Unity always runs every object's Awake() before any object's Start()
// for objects already in the scene when it loads, so this ordering is
// guaranteed without extra setup.

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ClassPrefab
{
    public CharacterClass characterClass;
    public Unit prefab;
}

public class PartySpawner : MonoBehaviour
{
    public BattleManager battleManager;
    public List<ClassPrefab> classPrefabs;
    public Transform[] spawnPoints; // needs at least 3

    void Awake()
    {
        List<CharacterClass> party = PartySelection.ChosenParty;

        if (party == null || party.Count == 0)
        {
            Debug.LogWarning("PartySpawner: no party selected - did you skip Character Select? " +
                              "Falling back to whatever units are already manually placed in the scene.");
            return;
        }

        for (int i = 0; i < party.Count && i < spawnPoints.Length; i++)
        {
            Unit prefab = FindPrefab(party[i]);
            if (prefab == null)
            {
                Debug.LogWarning($"PartySpawner: no prefab assigned for {party[i]}");
                continue;
            }

            Unit spawned = Instantiate(prefab, spawnPoints[i].position, spawnPoints[i].rotation);
            battleManager.playerUnits.Add(spawned);
        }
    }

    Unit FindPrefab(CharacterClass c)
    {
        foreach (ClassPrefab cp in classPrefabs)
            if (cp.characterClass == c) return cp.prefab;
        return null;
    }
}
