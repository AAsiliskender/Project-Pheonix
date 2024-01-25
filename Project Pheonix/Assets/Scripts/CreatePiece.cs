using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ArmyManager;

public class CreatePiece : MonoBehaviour
{
    

    public enum Affiliation
    {
        Player,
        Ally,
        Neutral,
        Enemy
    }

    public enum Direction
    {
        Forward,
        Backward,
        Rightward,
        Leftward
    }

    public enum PieceClass
    {
        Pawn,
        Knight,
        Bishop,
        Rook,      
        Ranger,
        Siege,
        Queen,
        King
    }

    public enum KnightVariant
    {
        Basic,
        LightCavalry,
        HeavyCavalry,
        Juggernaut
    }

    public enum PawnVariant
    {
        Basic,
        Pikemen,
        Beserker,
        Juggernaut,
        Immortal,
        ImmortalJuggernaut
    }
    
    

    /// EXPERIMENTAL ///
    
    // Unused code (for associating a unit with a piece)
    public class Piece
    {
        public PieceClass Type { get; set; }
        public Unit PieceUnit { get; set; }

        public Piece(PieceClass type, Unit unit)
        {
            Type = type;
            PieceUnit = unit;
        }
    }

    public class Unit // TODO: NEED TO DIRECTLY ASSOCIATE A UNIT'S GAMEOBJECT WITH THE UNIT (TO MAKE ITS RENDERING AND DATA ACQUISITION FASTER)
    {
        public string UnitName { get; set; }
        //public float Health { get; set; }
        // Other common unit fields and properties...

        //public readonly string UnitName;
        //public int UnitClass; //
        public PieceClass UnitClass;
        public float UnitExperience; 
        public int UnitLevel; // These two (above) are tied together
        public readonly int UnitId; // Will be new every run (0-1000?)
        public int[] Items; // Item(s) on Unit based on ItemId
        public int[] Talents; // Talent/Skills of unit
        public int EntityMaxCount;
        public int EntityCurrentCount;
        public float EntityMaxHP;
        public float EntityCurrentHP;
        public float MaxHealth;
        public float CurrentHealth;
        public int Stress;
        public int[] Status; // Uses id for buffs/debuffs/statuses
        public bool IsOutOfAction;
        public Direction Orientation; 
        
        // Stats:
        public int Mobility;
        public int Resilience;
        public int Leadership;
        public int Discipline;

        //public PieceClass Type { get; set; }
        //public PieceClass Type;
        
        // Dependencies:
        public Army AttachedArmy; // Army the piece is bound to
        public GameObject ThisGameObject; // The GameObject of the piece

        // Stats:
        public Vector2Int Position; // X and Y position on board, if [999,999], out of board
        // Movesets:
        public Vector2Int[][] ActiveSlideMoveSet; // Moveset where pieces are attacked/captured (offensive) (vector2int[][] as it has pathing)
        public int[] ActiveSlideMoveSize; // Size of moveset (number of singular vector paths)
        public Vector2Int[][] PassiveSlideMoveSet; // Moveset where pieces are not attacking (passive/defensive)
        public int[] PassiveSlideMoveSize;
        public Vector2Int[] ActiveJumpMoveSet; // Jumping moves, ignoring pathing
        public int[] ActiveJumpMoveSize; 
        public Vector2Int[] PassiveJumpMoveSet; // Rare defensive jump moves
        public int[] PassiveJumpMoveSize;

        // Statuses:
        public bool isEngaged;
        public bool isStunned;
        public bool isWavering;
        public bool isHasted;
        public bool isFogged;
        public bool isBlinded;
        public bool isSelected;
        public bool isTemporary;

        // Unit Constructor
        public Unit(PieceClass unitClass, float unitExperience, int unitLevel, int entityMaxCount, int entityCurrentCount, 
                    float entityMaxHP, float entityCurrentHP, int stress, bool isOutOfAction, int mobility, int resilience, 
                    int leadership, int discipline, Army attachedArmy, Vector2Int[][] activeSlideMoveSet, 
                    int[] activeSlideMoveSize, Vector2Int[][] passiveSlideMoveSet, int[] passiveSlideMoveSize, 
                    Vector2Int[] activeJumpMoveSet, int[] activeJumpMoveSize, Vector2Int[] passiveJumpMoveSet, 
                    int[] passiveJumpMoveSize)
        {
            UnitId = unitID;

            UnitClass = unitClass;
            UnitExperience = unitExperience;
            UnitLevel = unitLevel;
            EntityMaxCount = entityMaxCount;
            EntityCurrentCount = entityCurrentCount;
            EntityMaxHP = entityMaxHP;
            EntityCurrentHP = entityCurrentHP;
            Stress = stress;
            
            Mobility = mobility;
            Resilience = resilience;
            Leadership = leadership;
            Discipline = discipline;

            IsOutOfAction = isOutOfAction;
            AttachedArmy = attachedArmy;
            Position = new Vector2Int(999,999);

            ActiveSlideMoveSet = activeSlideMoveSet;
            ActiveSlideMoveSize = activeSlideMoveSize;
            PassiveSlideMoveSet = passiveSlideMoveSet;
            PassiveSlideMoveSize = passiveSlideMoveSize;

            ActiveJumpMoveSet = activeJumpMoveSet;
            ActiveJumpMoveSize = activeJumpMoveSize;
            PassiveJumpMoveSet = passiveJumpMoveSet;
            PassiveJumpMoveSize = passiveJumpMoveSize;

            // Statuses:
            isEngaged = false;
            isStunned = false;
            isWavering = false;
            isHasted = false;
            isFogged = false;
            isBlinded = false;
            isSelected = false;
            isTemporary = false;

            // Default values for other states:
            Items = null;
            Talents = null;
            Status = null;

                        //Type = PieceClass.Basic;

            ////
            //// Applying functionals:
            ////
            // Initial HP is always EntityHP*Count
            MaxHealth = entityMaxHP * (float)(EntityMaxCount); 
            CurrentHealth = entityCurrentHP * (float)(EntityCurrentCount); 

            // Set orientation as forward now, change movement depending on orientation (changing orientations must be a fnction)
            Orientation = Direction.Forward;
            // Set basic orientation based on affiliation of army (may change with conditions later)
            //if (AttachedArmy.Faction == Affiliation.Player || AttachedArmy.Faction == Affiliation.Ally)
            //{
            //    Orientation = Direction.Forward;
            //} else if (AttachedArmy.Faction == Affiliation.Enemy)
            //{
            //    Orientation = Direction.Backward;
            //}
            
            
            unitID++; // Increase ID

            return;
        }
    }

    public class Knight : Unit
    {
        // Other knight-specific fields and properties
        public int Armor { get; set; }
        public KnightVariant KnightType;
        

        public Knight(PieceClass unitClass, float unitExperience, int unitLevel, int entityMaxCount, int entityCurrentCount,
                    float entityMaxHP, float entityCurrentHP, int stress, bool isOutOfAction, int mobility, int resilience, 
                    int leadership, int discipline, Army attachedArmy, Vector2Int[][] activeSlideMoveSet, 
                    int[] activeSlideMoveSize, Vector2Int[][] passiveSlideMoveSet, int[] passiveSlideMoveSize, 
                    Vector2Int[] activeJumpMoveSet, int[] activeJumpMoveSize, Vector2Int[] passiveJumpMoveSet, 
                    int[] passiveJumpMoveSize, KnightVariant knightType, int armor)
            : base(unitClass, unitExperience, unitLevel, entityMaxCount, entityCurrentCount,
                    entityMaxHP, entityCurrentHP, stress, isOutOfAction, mobility, resilience, 
                    leadership, discipline, attachedArmy, activeSlideMoveSet, 
                    activeSlideMoveSize, passiveSlideMoveSet, passiveSlideMoveSize, 
                    activeJumpMoveSet, activeJumpMoveSize, passiveJumpMoveSet, passiveJumpMoveSize)
        {
            // Initialize knight-specific properties
            Armor = armor;
            KnightType = knightType;

            UnitName = "Knight #"+unitID.ToString();

            // The unit's GameObject
            ThisGameObject = new GameObject(UnitName);
        }
    }

    public class Pawn : Unit
    {
        // Other pawn-specific fields and properties
        public int Resolve { get; set; }
        public PawnVariant PawnType;
        

        public Pawn(PieceClass unitClass, float unitExperience, int unitLevel, int entityMaxCount, int entityCurrentCount,
                    float entityMaxHP, float entityCurrentHP, int stress, bool isOutOfAction, int mobility, int resilience, 
                    int leadership, int discipline, Army attachedArmy, Vector2Int[][] activeSlideMoveSet, 
                    int[] activeSlideMoveSize, Vector2Int[][] passiveSlideMoveSet, int[] passiveSlideMoveSize, 
                    Vector2Int[] activeJumpMoveSet, int[] activeJumpMoveSize, Vector2Int[] passiveJumpMoveSet, 
                    int[] passiveJumpMoveSize, PawnVariant pawnType, int resolve)
            : base(unitClass, unitExperience, unitLevel, entityMaxCount, entityCurrentCount,
                    entityMaxHP, entityCurrentHP, stress, isOutOfAction, mobility, resilience, 
                    leadership, discipline, attachedArmy, activeSlideMoveSet, 
                    activeSlideMoveSize, passiveSlideMoveSet, passiveSlideMoveSize, 
                    activeJumpMoveSet, activeJumpMoveSize, passiveJumpMoveSet, passiveJumpMoveSize)
        {
            // Initialize pawn-specific properties
            Resolve = resolve;
            PawnType = pawnType;
            
            UnitName = "Pawn #"+unitID.ToString();

            // The unit's GameObject
            ThisGameObject = new GameObject(UnitName);
        }
    }

    
    static int unitID = 0;
    
    //
    // Constructor - Unit(type,XP,level,Entity,EntHP,stress,oOA,mob,res,lead,disc,faction,ActSlideMovSet,ActSlideMovSize,PasSlideMovSet,PasSlideMovSize,ActJumpMovSet,ActJumpMovSize,PasJumpMovSet,PasJumpMovSize)

    public void NewPawn(Army army)
    {
        Vector2Int[][] pasSlideMovSet = new Vector2Int[2][];
        pasSlideMovSet[0] = new Vector2Int[1] { new Vector2Int(0, 1) };
        //pasSlideMovSet[1] = new Vector2Int[2] { new Vector2Int(0, 1), new Vector2Int(0, 1) };
        int[] pasSlideMovSize = new int[] {2};//{1, 1};
        
        Vector2Int[][] actSlideMovSet = null;
        int[] actSlideMovSize = null;
        //Vector2Int[][] pasSlideMovSet = null;
        //int[] pasSlideMovSize = null;

        Vector2Int[] actJumpMovSet = null;
        int[] actJumpMovSize = null;
        Vector2Int[] pasJumpMovSet = null;
        int[] pasJumpMovSize = null;

        PieceClass unitType = PieceClass.Pawn;
        PawnVariant unitClass = PawnVariant.Basic;
        int resolve = 10;

        //Unit newUnit = new Unit(0, 0, 0, 100, 2.0f, 0, false, 100, 100, 100, 100, faction, actSlideMovSet, actSlideMovSize, pasSlideMovSet, pasSlideMovSize, actJumpMovSet, actJumpMovSize, pasJumpMovSet, pasJumpMovSize);
        
        Unit newUnit = new Pawn(unitType, 0f, 0, 100, 100, 2.0f, 2.0f, 0, true, 100, 100, 100, 100, army, actSlideMovSet, actSlideMovSize, pasSlideMovSet, pasSlideMovSize, actJumpMovSet, actJumpMovSize, pasJumpMovSet, pasJumpMovSize, unitClass, resolve);

        newUnit = AddMove(new Vector2Int (1,1), newUnit, "Active", "Jump", new string[] {"LRsym"}, 1);
        
        
        Debug.Log("Unit name: " + newUnit.UnitName + ". Army: " + army.ArmyName);
        
        // Add unit to relevant list and dictionary
        // Change to add to List and Dictionary to relevant host army (unrelated to faction,
        // can have NPC ally units in a different army)

        army.UnitList.Add(newUnit);
        army.UnitDict.Add(newUnit.UnitId, newUnit);

        return;
    }

    public void NewKnight(Army army)
    {
        //Vector2Int[][] actSlideMovSet = new Vector2Int[2][];
        //actSlideMovSet[0] = new Vector2Int[1] { new Vector2Int(1, 1) };
        //actSlideMovSet[1] = new Vector2Int[1] { new Vector2Int(-1, 1) };
        //int[] actSlideMovSize = new int[] {1, 1};
        

        //Vector2Int[][] pasSlideMovSet = new Vector2Int[2][];
        //pasSlideMovSet[0] = new Vector2Int[1] { new Vector2Int(0, 1) };
        //pasSlideMovSet[1] = new Vector2Int[1] { new Vector2Int(0, 2) };
        //int[] pasSlideMovSize = new int[] {1, 1};

        Vector2Int[][] actSlideMovSet = null;
        int[] actSlideMovSize = null;
        Vector2Int[][] pasSlideMovSet = null;
        int[] pasSlideMovSize = null;
        
        
        //Vector2Int[] actJumpMovSet = new Vector2Int[8];
        //actJumpMovSet[0] = new Vector2Int(1, 2);
        //actJumpMovSet[1] = new Vector2Int(-1, 2);
        //actJumpMovSet[2] = new Vector2Int(1, -2);
        //actJumpMovSet[3] = new Vector2Int(-1, -2);
        //actJumpMovSet[4] = new Vector2Int(2, 1);
        //actJumpMovSet[5] = new Vector2Int(2, -1);
        //actJumpMovSet[6] = new Vector2Int(-2, 1);
        //actJumpMovSet[7] = new Vector2Int(-2, -1);
        //int[] actJumpMovSize = new int[] {1, 1, 1, 1, 1, 1, 1, 1};
        Vector2Int[] actJumpMovSet = null;
        int[] actJumpMovSize = null;
        Vector2Int[] pasJumpMovSet = null;
        int[] pasJumpMovSize = null;

        int armor = 999;
        PieceClass unitType = PieceClass.Knight;
        KnightVariant unitClass = KnightVariant.Basic;

        //Unit newUnit = new Unit(10, 0, 0, 30, 3.0f, 0, false, 500, 100, 100, 120, faction, actSlideMovSet, actSlideMovSize, pasSlideMovSet, pasSlideMovSize, actJumpMovSet, actJumpMovSize, pasJumpMovSet, pasJumpMovSize);
        Unit newUnit = new Knight(unitType, 0f, 0, 30, 30, 3.0f, 3.0f, 0, true, 500, 100, 100, 120, army, actSlideMovSet, actSlideMovSize, pasSlideMovSet, pasSlideMovSize, actJumpMovSet, actJumpMovSize, pasJumpMovSet, pasJumpMovSize, unitClass, armor);
 
        newUnit = AddMove(new Vector2Int (2,1), newUnit, "Active", "Jump", new string[] {"LRsym", "UDsym", "Diagsym"}, 1);
        //newUnit = AddMove(new Vector2Int[3] {new Vector2Int (1,0), new Vector2Int (0,1), new Vector2Int (0,1)}, newUnit, "Active", "Slide", new string[] {"LRsym", "UDsym", "Diagsym"}, 1);
        

        Debug.Log("Unit name: " + newUnit.UnitName + ". Army: " + army.ArmyName);

        // Add unit to relevant list and dictionary
        army.UnitList.Add(newUnit);
        army.UnitDict.Add(newUnit.UnitId, newUnit);

        return;
    }

    // Takes piece, and depending on moveStyle ("Passive"/"Active") and moveType ("Slide"/"Jump"),
    // and moveSymmetry (LRsym,UDsym,Diagsym), and moveVector alongside its moveSize, add the move(s) to
    // the piece. LR sym is left/right (vertical axis) 'symmetry' so it copies that while UD is up/down,
    // Diagsym copies diagonally (swapping x and y)
    public Unit AddMove(Vector2Int newMoveVector, Unit piece, string moveStyle, string moveType, string[] moveSymmetry, int moveSize)
    {
        // Guard Function
        if(!(moveStyle != "Passive" ^ moveStyle != "Active")) // If not passive nor active (XOR)
        {
            throw new System.ArgumentException("The move style has to be \"Passive\" or \"Active\")");
            return piece;
        }
        if (moveType != "Jump") // If not jump
        {
            throw new System.ArgumentException("The move type has to be \"Jump\") (based on input)");
            return piece;
        }

        string moveSetFieldName = moveStyle + moveType + "MoveSet";
        string moveSizeFieldName = moveStyle + moveType + "MoveSize";

        // Initialised combined vars
        Vector2Int[] newMoveSetList = null;
        int[] newMoveSizeList = null;

        // Get the type of the Unit object
        Type unitType = piece.GetType();

        // Get the FieldInfo object for the moveSet and moveSize field
        FieldInfo moveSetField = unitType.GetField(moveSetFieldName);
        FieldInfo moveSizeField = unitType.GetField(moveSizeFieldName);

        // Get the current value of the moveSet and moveSize field
        object moveSetObject = moveSetField.GetValue(piece);
        object moveSizeObject = moveSizeField.GetValue(piece);
        

        // Cast the moveSetObject/moveSizeObject to the appropriate type (e.g. Vector2Int[]) and modify it
        Vector2Int[] moveSetList = (Vector2Int[])moveSetObject;
        int[] moveSizeList = (int[])moveSizeObject;

        int moveAddLength = 1;
        List<Vector2Int> tempList = new List<Vector2Int>();
        tempList.Add(newMoveVector); // Original vector
        Vector2Int tempVector = newMoveVector; // Used as placeholder for intialising

        // Create a new array with length X (1-4 depending on 'symmetries')
        // Left-Right Symmetry (reflect along y axis [x -> -x])
        if (moveSymmetry.Contains("LRsym", StringComparer.OrdinalIgnoreCase))
        {
            moveAddLength++;
            tempVector = new Vector2Int (-newMoveVector.x, newMoveVector.y);
            tempList.Add(tempVector);
        }
        // Up-Down Symmetry (reflect along x axis [y -> -y])
        if (moveSymmetry.Contains("UDsym", StringComparer.OrdinalIgnoreCase))
        {
            moveAddLength++;
            tempVector = new Vector2Int (newMoveVector.x, -newMoveVector.y);
            tempList.Add(tempVector);
        }
        // Left-Right Symmetry AND Up-Down Symmetry (copy to 3rd quadrant, [x -> -x, y -> -y])
        if (moveSymmetry.Contains("LRsym", StringComparer.OrdinalIgnoreCase) && moveSymmetry.Contains("UDsym", StringComparer.OrdinalIgnoreCase))
        {
            moveAddLength++;
            tempVector = new Vector2Int (-newMoveVector.x, -newMoveVector.y);
            tempList.Add(tempVector);
        }
        // Diagonal Symmetry (reflect along x=y axis [x -> y, y -> x], and if there are any other symmetries, repeat them)
        if (moveSymmetry.Contains("Diagsym", StringComparer.OrdinalIgnoreCase))
        {
            moveAddLength++;
            tempVector = new Vector2Int (newMoveVector.y, newMoveVector.x);
            tempList.Add(tempVector);
            // Left-Right Symmetry (diagonally reflected)
            if (moveSymmetry.Contains("LRsym", StringComparer.OrdinalIgnoreCase))
            {
                moveAddLength++;
                tempVector = new Vector2Int (-newMoveVector.y, newMoveVector.x);
                tempList.Add(tempVector);
            }
            // Up-Down Symmetry (diagonally reflected)
            if (moveSymmetry.Contains("UDsym", StringComparer.OrdinalIgnoreCase))
            {
                moveAddLength++;
                tempVector = new Vector2Int (newMoveVector.y, -newMoveVector.x);
                tempList.Add(tempVector);
            }
            // Left-Right Symmetry AND Up-Down Symmetry (diagonally reflected)
            if (moveSymmetry.Contains("LRsym", StringComparer.OrdinalIgnoreCase) && moveSymmetry.Contains("UDsym", StringComparer.OrdinalIgnoreCase))
            {
                moveAddLength++;
                tempVector = new Vector2Int (-newMoveVector.y, -newMoveVector.x);
                tempList.Add(tempVector);
            }
        }


        if (moveSetList == null && moveSizeList == null)
        {
            // Convert list into array (Vector2Int[])
            newMoveSetList = tempList.ToArray();

            // Create move size array (int[])
            newMoveSizeList = Enumerable.Repeat(moveSize, moveAddLength).ToArray();

        } else {
            // Combine to original move list and change into array
            List<Vector2Int> combinedMoveSetList = moveSetList.ToList();
            combinedMoveSetList = combinedMoveSetList.Concat(tempList).ToList();
            newMoveSetList = combinedMoveSetList.ToArray();

            // Combine to original move size array (int[])
            List<int> combinedMoveSizeList = moveSizeList.ToList();
            List<int> tempSizeList = Enumerable.Repeat(moveSize, moveAddLength).ToList();
            combinedMoveSizeList = combinedMoveSizeList.Concat(tempSizeList).ToList();
            newMoveSizeList = combinedMoveSizeList.ToArray();
        }

        // Set the new value of the move set in the Unit object
        if (moveStyle == "Active")
        {
            piece.ActiveJumpMoveSet = newMoveSetList;
            piece.ActiveJumpMoveSize = newMoveSizeList;

        } else if (moveStyle == "Passive")
        {
            piece.PassiveJumpMoveSet = newMoveSetList;
            piece.PassiveJumpMoveSize = newMoveSizeList;
        }

        return piece;
    }

    public Unit AddMove(Vector2Int[] newMoveVector, Unit piece, string moveStyle, string moveType, string[] moveSymmetry, int moveSize)
    {
        // Guard Function
        if(!(moveStyle != "Passive" ^ moveStyle != "Active")) // If not passive nor active (XOR)
        {
            throw new System.ArgumentException("The move style has to be \"Passive\" or \"Active\")");
            return piece;
        }
        if (moveType != "Slide") // If not slide
        {
            throw new System.ArgumentException("The move type has to be \"Slide\") (based on input)");
            return piece;
        }
        
        string moveSetFieldName = moveStyle + moveType + "MoveSet";
        string moveSizeFieldName = moveStyle + moveType + "MoveSize";

        // Initialised combined vars
        Vector2Int[][] newMoveSetList = null;
        int[] newMoveSizeList = null;

        // Get the type of the Unit object
        Type unitType = piece.GetType();

        // Get the FieldInfo object for the moveSet and moveSize field
        FieldInfo moveSetField = unitType.GetField(moveSetFieldName);
        FieldInfo moveSizeField = unitType.GetField(moveSizeFieldName);

        // Get the current value of the moveSet and moveSize field
        object moveSetObject = moveSetField.GetValue(piece);
        object moveSizeObject = moveSizeField.GetValue(piece);
        

        // Cast the moveSetObject/moveSizeObject to the appropriate type (e.g. Vector2Int[][]) and modify it
        Vector2Int[][] moveSetList = (Vector2Int[][])moveSetObject;
        int[] moveSizeList = (int[])moveSizeObject;

        int moveAddLength = 1;
        List<Vector2Int[]> tempList = new List<Vector2Int[]>();
        List<Vector2Int> tempPathList = new List<Vector2Int>();

        Vector2Int[] tempVector = newMoveVector; // Used as placeholder for intialising
        Vector2Int tempPathVector = newMoveVector[0];

        for (int i = 0; i < newMoveVector.Length; i++)
        {
                tempPathVector = newMoveVector[i]; 
                tempPathList.Add(tempPathVector); // Add path to list
        }
        tempVector = tempPathList.ToArray();
        tempPathList.Clear();
        tempList.Add(tempVector); //Original Vector

        // Create a new array with length X (1-8 depending on 'symmetries') // REMAKE THIS PART OF THE CODE
        // Left-Right Symmetry (reflect along y axis [x -> -x])
        if (moveSymmetry.Contains("LRsym", StringComparer.OrdinalIgnoreCase))
        {
            moveAddLength++;
            for (int i = 0; i < newMoveVector.Length; i++)
            {
                tempPathVector = new Vector2Int (-newMoveVector[i].x, newMoveVector[i].y); 
                tempPathList.Add(tempPathVector); // Add path to list
            }
            tempVector = tempPathList.ToArray();
            tempPathList.Clear();
            tempList.Add(tempVector);
        }
        // Up-Down Symmetry (reflect along x axis [y -> -y])
        if (moveSymmetry.Contains("UDsym", StringComparer.OrdinalIgnoreCase))
        {
            moveAddLength++;
            for (int i = 0; i < newMoveVector.Length; i++)
            {
                tempPathVector = new Vector2Int (newMoveVector[i].x, -newMoveVector[i].y);
                tempPathList.Add(tempPathVector); // Add path to list
            }
            tempVector = tempPathList.ToArray();
            tempPathList.Clear();
            tempList.Add(tempVector);
        }
        // Left-Right Symmetry AND Up-Down Symmetry (copy to 3rd quadrant, [x -> -x, y -> -y])
        if (moveSymmetry.Contains("LRsym", StringComparer.OrdinalIgnoreCase) && moveSymmetry.Contains("UDsym", StringComparer.OrdinalIgnoreCase))
        {
            moveAddLength++;
            for (int i = 0; i < newMoveVector.Length; i++)
            {
                tempPathVector = new Vector2Int (-newMoveVector[i].x, -newMoveVector[i].y);
                tempPathList.Add(tempPathVector); // Add path to list
            }
            tempVector = tempPathList.ToArray();
            tempPathList.Clear();
            tempList.Add(tempVector);
        }
        // Diagonal Symmetry (reflect along x=y axis [x -> y, y -> x], and if there are any other symmetries, repeat them)
        if (moveSymmetry.Contains("Diagsym", StringComparer.OrdinalIgnoreCase))
        {
            moveAddLength++;
            for (int i = 0; i < newMoveVector.Length; i++)
            {
                tempPathVector = new Vector2Int (newMoveVector[i].y, newMoveVector[i].x);
                tempPathList.Add(tempPathVector); // Add path to list
            }
            tempVector = tempPathList.ToArray();
            tempPathList.Clear();
            tempList.Add(tempVector);
            // Left-Right Symmetry (diagonally reflected)
            if (moveSymmetry.Contains("LRsym", StringComparer.OrdinalIgnoreCase))
            {
                moveAddLength++;
                for (int i = 0; i < newMoveVector.Length; i++)
                {
                    tempPathVector = new Vector2Int (-newMoveVector[i].y, newMoveVector[i].x);
                    tempPathList.Add(tempPathVector); // Add path to list
                }
                tempVector = tempPathList.ToArray();
                tempPathList.Clear();
                tempList.Add(tempVector);
            }
            // Up-Down Symmetry (diagonally reflected)
            if (moveSymmetry.Contains("UDsym", StringComparer.OrdinalIgnoreCase))
            {
                moveAddLength++;
                for (int i = 0; i < newMoveVector.Length; i++)
                {
                    tempPathVector = new Vector2Int (newMoveVector[i].y, -newMoveVector[i].x);
                    tempPathList.Add(tempPathVector); // Add path to list
                }
                tempVector = tempPathList.ToArray();
                tempPathList.Clear();
                tempList.Add(tempVector);
            }
            // Left-Right Symmetry AND Up-Down Symmetry (diagonally reflected)
            if (moveSymmetry.Contains("LRsym", StringComparer.OrdinalIgnoreCase) && moveSymmetry.Contains("UDsym", StringComparer.OrdinalIgnoreCase))
            {
                moveAddLength++;
                for (int i = 0; i < newMoveVector.Length; i++)
                {
                    tempPathVector = new Vector2Int (-newMoveVector[i].y, newMoveVector[i].x);
                    tempPathList.Add(tempPathVector); // Add path to list
                }
                tempVector = tempPathList.ToArray();
                tempPathList.Clear();
                tempList.Add(tempVector);
            }
        }
        Debug.Log(tempList.Count);

        if (moveSetList == null && moveSizeList == null)
        {
            // Convert list into array (Vector2Int[][])
            newMoveSetList = tempList.ToArray();
            Debug.Log(newMoveSetList.Length);

            // Create move size array (int[])
            newMoveSizeList = Enumerable.Repeat(moveSize, moveAddLength).ToArray();

        } else {
            // Combine to original move list and change into array
            List<Vector2Int[]> combinedMoveSetList = moveSetList.ToList();
            combinedMoveSetList = combinedMoveSetList.Concat(tempList).ToList();
            newMoveSetList = combinedMoveSetList.ToArray();

            // Combine to original move size array (int[])
            List<int> combinedMoveSizeList = moveSizeList.ToList();
            List<int> tempSizeList = Enumerable.Repeat(moveSize, moveAddLength).ToList();
            combinedMoveSizeList = combinedMoveSizeList.Concat(tempSizeList).ToList();
            newMoveSizeList = combinedMoveSizeList.ToArray();
        }

        // Set the new value of the move set in the Unit object
        if (moveStyle == "Active")
        {
            piece.ActiveSlideMoveSet = newMoveSetList;
            piece.ActiveSlideMoveSize = newMoveSizeList;

        } else if (moveStyle == "Passive")
        {
            piece.PassiveSlideMoveSet = newMoveSetList;
            piece.PassiveSlideMoveSize = newMoveSizeList;
        }

        return piece;
    }

}
