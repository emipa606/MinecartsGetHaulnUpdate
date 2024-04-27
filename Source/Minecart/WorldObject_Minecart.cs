using System;
using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Minecart;

public class WorldObject_Minecart : WorldObject
{
    public float direction;
    public List<Building_Minecart> minecarts;
    public int NextTile;

    private float tileProgress;

    public override Vector3 DrawPos => Vector3.Lerp(
        Find.WorldGrid.GetTileCenter(Tile),
        Find.WorldGrid.GetTileCenter(NextTile),
        tileProgress
    );

    public override void PostAdd()
    {
        var neighbors = new List<int>();
        Find.WorldGrid.GetTileNeighbors(Tile, neighbors);
        neighbors.RemoveAll(t => Find.WorldGrid.GetRoadDef(Tile, t) == null);
        if (neighbors.Count > 0)
        {
            NextTile = neighbors.MinBy(t =>
                180 - Math.Abs(Math.Abs(direction - Find.WorldGrid.GetHeadingFromTo(Tile, t)) - 180));
            direction = Find.WorldGrid.GetHeadingFromTo(Tile, NextTile);
        }
        else
        {
            NextTile = Tile;
        }
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();

        sb.Append("MGHU.SpeedInspect".Translate(minecarts[0].Speed.ToStringWithSign()));

        return sb.ToString();
    }

    public override void Tick()
    {
        if (minecarts.Count > 0)
        {
            tileProgress += minecarts[0].Speed / CaravanTicksPerMoveUtility.CellToTilesConversionRatio;
        }
        else
        {
            tileProgress += 0.05f;
        }

        if (!(tileProgress >= 1))
        {
            return;
        }

        Tile = NextTile;
        var mapParent = Find.WorldObjects.MapParentAt(Tile);
        if (mapParent is { HasMap: true })
        {
            var map = mapParent.Map;
            foreach (var c in CellRect.WholeMap(map).ContractedBy(GenGrid.NoBuildEdgeWidth).EdgeCells)
            {
                if (c.GetTerrain(map).defName.Contains("Rail"))
                {
                    continue;
                }

                foreach (var minecart in minecarts)
                {
                    minecart.SpawnSetup(map, false);
                    minecart.Position = c;
                }
            }
        }


        var neighbors = new List<int>();
        Find.WorldGrid.GetTileNeighbors(Tile, neighbors);
        neighbors.RemoveAll(t => Find.WorldGrid.GetRoadDef(Tile, t) == null);
        if (neighbors.Count > 0)
        {
            NextTile = neighbors.MinBy(t =>
                180 - Math.Abs(Math.Abs(direction - Find.WorldGrid.GetHeadingFromTo(Tile, t)) - 180));
            direction = Find.WorldGrid.GetHeadingFromTo(Tile, NextTile);
        }

        tileProgress--;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref direction, "direction");
        Scribe_Values.Look(ref NextTile, "nextTile");
        Scribe_Values.Look(ref tileProgress, "subtile");
        Scribe_Deep.Look(ref minecarts, "minecarts");
    }
}