//You can change item types here
public enum ItemType
{
    Weapon,
    Armour,
    Ingredient,
    Artefact,
    Junk
}


//You can change rareness ranks here
public enum Rareness
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    None
}


//Single celled means any item will take only one slot in the inventory
//Rectangular means that item can take more than one slots, but they
//will be 1x1 or 1x2 or 2x2 or 2x3 or 3x3 etc.
//AnySize means they can take any slots in any shape as they want
public enum InventoryType
{
    SingleCelled,
    Rectangular,
    AnySize
}


//It contains result of the placing new item in the inventory
public class NewItemPlacementResult
{
    public ItemStack stackReplaced=null;
    public bool invalidPosition=false;
    public bool stackCapReached=false;
    public bool weightLimitReached=false;
    public bool areaIsOccupied=false;
    public bool failedItemRestrictions = false;
}