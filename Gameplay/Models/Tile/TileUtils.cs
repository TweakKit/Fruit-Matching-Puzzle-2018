using System;
using System.Collections.Generic;

public static class TileUtils
{
    public static List<TileType> GetAllTileTypes()
    {
        List<TileType> tileTypes = new List<TileType>();

        foreach (var tileType in Enum.GetValues(typeof(TileType)))
            tileTypes.Add((TileType)tileType);

        return tileTypes;
    }

    public static List<TileType> GetRandomTileTypes(List<TileType> inputTileTypes, int amount)
    {
        inputTileTypes.Shuffle();
        return inputTileTypes.GetRange(0, amount);
    }

    public static TileType GetTileType(int tileCode)
    {
        return (TileType)tileCode;
    }
}