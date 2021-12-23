using System.Linq;
using System.Collections.Generic;

using UnityEngine;

public class Level
{
    #region Members

    public int width;
    public int height;
    public int mapID;
    public int levelID;
    public int numberOfItemTypes;
    public int numberOfLayers;
    public int numberOfTiles;
    public List<LevelEditorTile> levelEditorTiles;

    #endregion Members

    #region Class Methods

    public Level()
    {
        width = 1;
        height = 1;
        mapID = 1;
        levelID = 1;
        numberOfItemTypes = 1;
        numberOfLayers = 1;
        numberOfTiles = 3;
        levelEditorTiles = new List<LevelEditorTile>();
    }

    public List<Tile> GetTiles(TutorialType tutorialType)
    {
        List<Tile> tiles = new List<Tile>();

        foreach (var levelEditorTile in levelEditorTiles)
        {
            Vector3 tilePosition = Vector3.zero;

            if (levelEditorTile.layer % 2 != 0)
                tilePosition = new Vector3(levelEditorTile.indexX * TileDefinition.TileSize,
                                           levelEditorTile.indexY * TileDefinition.TileSize);
            else
                tilePosition = new Vector3(levelEditorTile.indexX * TileDefinition.TileSize + TileDefinition.TileHalfSize,
                                           levelEditorTile.indexY * TileDefinition.TileSize + TileDefinition.TileHalfSize);

            Tile tile = new Tile(levelEditorTile.layer, tilePosition);
            tiles.Add(tile);
        }

        List<TileType> levelTileTypes = new List<TileType>();
        List<TileType> originalRandomTileTypes = TileUtils.GetRandomTileTypes(TileUtils.GetAllTileTypes(), numberOfItemTypes);
        int equalSetsPerItemType = numberOfTiles / (numberOfItemTypes * GameDefinition.TilesComboSum);

        for (int i = 0; i < numberOfItemTypes; i++)
            for (int j = 0; j < GameDefinition.TilesComboSum * equalSetsPerItemType; j++)
                levelTileTypes.Add(originalRandomTileTypes[i]);

        int leftTiles = numberOfTiles - (numberOfItemTypes * GameDefinition.TilesComboSum * equalSetsPerItemType);
        int leftSets = leftTiles / GameDefinition.TilesComboSum;
        List<TileType> leftRandomTileTypes = TileUtils.GetRandomTileTypes(originalRandomTileTypes, leftSets);

        for (int i = 0; i < leftSets; i++)
            for (int j = 0; j < GameDefinition.TilesComboSum; j++)
                levelTileTypes.Add(originalRandomTileTypes[i]);

        levelTileTypes.Shuffle();
        for (int i = 0; i < levelTileTypes.Count; i++)
            tiles[i].TileType = levelTileTypes[i];

        switch (tutorialType)
        {
            case TutorialType.None:
                {
                    return tiles;
                }

            case TutorialType.Special:
                {
                    List<TutorialTile> tutorialTiles = new List<TutorialTile>();
                    tiles.ForEach((tile) => tutorialTiles.Add(tile.ToTutorialTile()));
                    tutorialTiles.ForEach((tutorialTile) => tutorialTile.SetTutorialTileLevel(levelID));

                    int highestLayer = tutorialTiles.Max((tutorialTile) => tutorialTile.Layer);
                    List<TutorialTile> filteredTutorialTiles = tutorialTiles.Where((tutorialTile) => tutorialTile.Layer == highestLayer).ToList();
                    float lowestPositionY = filteredTutorialTiles.Min((tutorialTile) => tutorialTile.CurrentPosition.y);
                    filteredTutorialTiles = filteredTutorialTiles.Where((tutorialTile) => tutorialTile.CurrentPosition.y == lowestPositionY).ToList();
                    List<TutorialTile> sampleTutorialTiles = new List<TutorialTile>();

                    int numberOfSampleTutorialTiles = GameDefinition.TilesComboSum;
                    while (--numberOfSampleTutorialTiles >= 0)
                    {
                        float highestPositionX = filteredTutorialTiles.Max((tutorialTile) => tutorialTile.CurrentPosition.x);
                        TutorialTile sampleTutorialTile = filteredTutorialTiles.Find((tutorialTile) => tutorialTile.CurrentPosition.x == highestPositionX);
                        sampleTutorialTiles.Add(sampleTutorialTile);
                        filteredTutorialTiles.Remove(sampleTutorialTile);
                    }

                    List<TutorialTile> checkedSampleTutorialTiles = new List<TutorialTile>();
                    checkedSampleTutorialTiles.Add(sampleTutorialTiles[0]);

                    for (int i = 1; i < GameDefinition.TilesComboSum; i++)
                    {
                        checkedSampleTutorialTiles.Add(sampleTutorialTiles[i]);
                        if (sampleTutorialTiles[i].TileType != sampleTutorialTiles[0].TileType)
                        {
                            foreach (var tutorialTile in tutorialTiles)
                            {
                                if (checkedSampleTutorialTiles.Any((checkedSampleTile) => checkedSampleTile == tutorialTile))
                                    continue;

                                if (tutorialTile.TileType == sampleTutorialTiles[0].TileType)
                                {
                                    TileType tempTileType = tutorialTile.TileType;
                                    tutorialTile.TileType = sampleTutorialTiles[i].TileType;
                                    sampleTutorialTiles[i].TileType = tempTileType;
                                    break;
                                }
                            }
                        }
                    }

                    checkedSampleTutorialTiles.ForEach((sampleTutorialTile) => sampleTutorialTile.IsPickabilityEnabled = true);
                    return tutorialTiles.Cast<Tile>().ToList();
                }

            case TutorialType.Normal:
                {
                    List<TutorialTile> tutorialTiles = new List<TutorialTile>();
                    tiles.ForEach((tile) => tutorialTiles.Add(tile.ToTutorialTile()));

                    tutorialTiles.ForEach((tutorialTile) =>
                    {
                        tutorialTile.IsPickabilityEnabled = true;
                        tutorialTile.SetTutorialTileLevel(levelID);
                    });

                    return tutorialTiles.Cast<Tile>().ToList();
                }
        }

        return tiles;
    }

    #endregion Class Methods
}