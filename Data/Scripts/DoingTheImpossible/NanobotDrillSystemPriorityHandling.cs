namespace SpaceEquipmentLtd.NanobotDrillSystem
{
  using Sandbox.Definitions;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Utils;
  using VRage.Game;
  using VRage.Utils;

  public enum ComponentClass
  {
    None = 1,
    Ingot,
    Ore,
    Stone,
    Gravel
  }

  public class NanobotDrillSystemDrillPriorityHandling : PriorityHandling<PrioItem, MyStringHash>
  {
    //private Dictionary<MyStringHash, OreClass> _ItemClassHash = new Dictionary<MyStringHash, OreClass>();

    public NanobotDrillSystemDrillPriorityHandling() : base()
    {
      VRage.Collections.DictionaryValuesReader<string, MyVoxelMaterialDefinition> materialDefinitions = MyDefinitionManager.Static.GetVoxelMaterialDefinitions();
      List<string> list = new List<string>();
      foreach (MyVoxelMaterialDefinition materialDefinition in materialDefinitions)
      {
        if (materialDefinition.Enabled && materialDefinition.Public && materialDefinition.CanBeHarvested && !string.IsNullOrEmpty(materialDefinition.MinedOre) && list.IndexOf(materialDefinition.MinedOre) < 0)
        {
          //if (materialDefinition.MinedOre == "Ice" || materialDefinition.MinedOre == "Stone")
          //{
            list.Add(materialDefinition.MinedOre);
          //}
        }
      }
      list.Sort();
      foreach (string ore in list)
      {
        MyStringHash hash = MyStringHash.GetOrCompute(ore);
        Mod.Log.Write(Logging.Level.Info, "Adding ore of type: {0}", ore);
        //if (ore == "Ice" || ore == "Stone")
        //{
          Add(new PrioItemState<PrioItem>(new PrioItem(hash.m_hash, ore), true, true));
        //}
        //else if (ore != "Ice" && ore != "Stone")
        //{
        //  Add(new PrioItemState<PrioItem>(new PrioItem(hash.m_hash, ore), false, true));
        //}
      }
      int iceIndex = FindIndex(kv => kv.PrioItem.Alias.Equals("Ice"));
      int stoneIndex = FindIndex(kv => kv.PrioItem.Alias.Equals("Stone"));
      this.Move(iceIndex, 0);
      this.Move(stoneIndex, 1);
    }

    /// <summary>
    /// Get the Block class
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public override int GetItemKey(MyStringHash a, bool real)
    {
      return a.m_hash;
    }

    /// <summary>
    /// Get the item alias
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public override string GetItemAlias(MyStringHash a, bool real)
    {
      return a.String;
    }

    public bool GetEnabled(string oresubtype)
    {
      return GetEnabled(MyStringHash.GetOrCompute(oresubtype));
    }

    public int GetPriority(string oresubtype)
    {
      return GetPriority(MyStringHash.GetOrCompute(oresubtype));
    }
  }

  public class NanobotDrillSystemComponentPriorityHandling : PriorityHandling<PrioItem, MyDefinitionId>
  {
    public NanobotDrillSystemComponentPriorityHandling() : base()
    {
      foreach (object item in Enum.GetValues(typeof(ComponentClass)))
      {
        Add(new PrioItemState<PrioItem>(new PrioItem((int)item, item.ToString()), true, true));
      }

      PrioItemState<PrioItem> keyValue = this.FirstOrDefault((kv) => kv.PrioItem.Key == (int)ComponentClass.None);
      if (keyValue != null)
      {
        keyValue.Enabled = false;
        keyValue.Visible = false;
      }
    }

    /// <summary>
    /// Get the Block class
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public override int GetItemKey(MyDefinitionId a, bool real)
    {
      if (a.TypeId == typeof(MyObjectBuilder_Ingot))
      {
        if (a.SubtypeName == "Stone")
        {
          return (int)ComponentClass.Gravel;
        }

        return (int)ComponentClass.Ingot;
      }
      if (a.TypeId == typeof(MyObjectBuilder_Ore))
      {
        if (a.SubtypeName == "Stone")
        {
          return (int)ComponentClass.Stone;
        }

        return (int)ComponentClass.Ore;
      }
      return (int)ComponentClass.None;
    }

    public override string GetItemAlias(MyDefinitionId a, bool real)
    {
      return ((ComponentClass)GetItemKey(a, real)).ToString();
    }
  }
}
