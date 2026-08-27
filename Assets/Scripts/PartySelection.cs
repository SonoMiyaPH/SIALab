// PartySelection.cs
// Tiny static holder, same pattern as CurrentLevel.cs - lets the
// CharacterSelect scene tell the Battle scene which 3 characters
// the player picked, without needing to pass data through the scene load.

using System.Collections.Generic;

public static class PartySelection
{
    public static List<CharacterClass> ChosenParty = new List<CharacterClass>();
}
