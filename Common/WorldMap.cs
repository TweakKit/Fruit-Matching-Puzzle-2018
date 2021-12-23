using System.Linq;
using System.Collections.Generic;

public class WorldMap
{
    #region Members

    public List<Map> maps;

    #endregion Members

    #region Class Methods

    public WorldMap()
    {
        maps = new List<Map>();
    }

    public void AddMapLevel(int mapID, int levelID)
    {
        if (maps.Any((map) => map.id == mapID))
        {
            MapLevel mapLevel = new MapLevel(levelID);
            Map map = maps.Find((m) => m.id == mapID);
            map.AddLevel(mapLevel);
        }
        else
        {
            Map map = new Map(mapID);
            MapLevel mapLevel = new MapLevel(levelID);
            map.AddLevel(mapLevel);
            maps.Add(map);
        }
    }

    public void Init()
    {
        if (maps.Count > 0)
            maps[0].Unlock();
    }

    public Map GetMap(int mapID)
    {
        if (!maps.Any((map) => map.id == mapID))
            return null;
        else
            return maps.Find((map) => map.id == mapID);
    }

    public bool ExistMapLevel(int mapID, int mapLevelID)
    {
        if (maps.Any((map) => map.id == mapID))
        {
            Map map = maps.Find((m) => m.id == mapID);
            if (map.GetMapLevel(mapLevelID) != null)
                return true;
        }

        return false;
    }

    public int GetAllAchievedStars()
    {
        int stars = 0;

        maps.ForEach((map) =>
        {
            if (map.isUnlocked)
                stars += map.GetAllAchievedStars();
        });

        return stars;
    }


    #endregion Class Methods
}

public class Map
{
    #region Members

    public int id;
    public bool isUnlocked;
    public string name;
    public List<MapLevel> mapLevels;

    #endregion Members

    #region Class Methods

    public Map()
    {
        id = -1;
        this.name = "";
        isUnlocked = false;
        mapLevels = new List<MapLevel>();
    }

    public Map(int id)
    {
        this.id = id;
        isUnlocked = false;
        mapLevels = new List<MapLevel>();
    }

    public void AddLevel(MapLevel mapLevel)
    {
        mapLevels.Add(mapLevel);
    }

    public MapLevel GetMapLevel(int mapLevelID)
    {
        if (!mapLevels.Any((mapLevel) => mapLevel.id == mapLevelID))
            return null;
        else
            return mapLevels.Find((mapLevel) => mapLevel.id == mapLevelID);
    }

    public void Unlock()
    {
        isUnlocked = true;
        if (mapLevels.Count > 0)
            mapLevels[0].Unlock();
    }

    public int GetAllAchievedStars()
    {
        int stars = 0;

        mapLevels.ForEach((mapLevel) =>
        {
            if (mapLevel.isUnlocked)
                stars += mapLevel.achievedStars;
        });

        return stars;
    }

    public int GetAllStars()
    {
        int stars = 0;
        mapLevels.ForEach((mapLevel) => stars += 3);
        return stars;
    }

    #endregion Class Methods
}

public class MapLevel
{
    #region Members

    public int id;
    public bool isUnlocked;
    public int achievedStars;

    #endregion Members

    #region Class Methods

    public MapLevel()
    {
        id = -1;
        isUnlocked = false;
        achievedStars = 0;
    }

    public MapLevel(int id)
    {
        this.id = id;
        isUnlocked = false;
        achievedStars = 0;
    }

    public void Unlock()
    {
        isUnlocked = true;
    }

    public void SetAchievedStars(int achievedStars)
    {
        this.achievedStars = achievedStars;
    }

    #endregion Class Methods
}