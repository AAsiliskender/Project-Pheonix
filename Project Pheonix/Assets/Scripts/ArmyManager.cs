using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CreatePiece;
using static PieceMovement;

public class ArmyManager : MonoBehaviour
{

    // Creating a Unit list for each faction
    //public static List<Unit> AllyUnitList = new List<Unit>();
    //public static List<Unit> EnemyUnitList = new List<Unit>();
    // Creating a Dictionary list for each unit
    //public static Dictionary<int, Unit> AllyUnitDict = new Dictionary<int, Unit>();
    //public static Dictionary<int, Unit> EnemyUnitDict = new Dictionary<int, Unit>();
    // We can access each specific unit instance by using unit ID:
    // Unit firstUnit = unitDict[1]; for example
    
    // Initialisation Checker
    private bool isInitialized = false;

    // Army ID
    static int armyID = 0;

    // Debug
    public int debugLevel;

    // Armies
    public static List<Army> armyList = new List<Army>(); 
    public static Army playerArmy;// { get; set; }
    public static Army enemyArmy;// { get; set; }
    

    // Army stats:
    public class Army
    {
        public string ArmyName { get; set; }
        // Other common unit fields and properties...

        public float ArmyExperience; 
        public int ArmyLevel; // These two (above) are tied together
        public int[] Items; // Item(s) on Unit based on ItemId
        public int[] ArmyTalents; // Talent/Skills of unit
        public int UnitCount;
        public float PlayerMaxHP;
        public float PlayerCurrentHP; // Player Current HP, one way failure state begins
        public int MentalMaxFortitude;
        public int MentalCurrentFortitude; // Player Mental HP
        public int[] Status; // Uses id for buffs/debuffs/statuses

        // Stats:
        public int ArmyMobility;
        public int ArmyResilience;
        public int ArmyLeadership;
        public int ArmyDiscipline;

        // GameObject:
        public GameObject ThisGameObject;
        private GameObject factionObj; // Needed to set dependencies at setup
        
        
        
        // Army Faction (or Affiliation):
        public readonly Affiliation Faction;

        // Initiative Point and related Stats:
        public int TurnTokens;
        public int IP;
        public float ExtraIP;

        // Statuses:
        //public bool isEngaged;
        public bool isRouting;
        public bool isAttacker;
        public bool isAmbusher;
        public bool isOutOfAction;

        public readonly int ArmyID; // Will be new every run (0 player, others ally/enemy)
        

        // Unit list for army contained in army
        public List<Unit> UnitList;// { get; set; }
        // Dictionary list of units in army
        public Dictionary<int, Unit> UnitDict;// { get; set; }



        public Army(float armyExperience, int armyLevel, float playerMaxHP, float playerCurrentHP, int mentalMaxHP, int mentalCurrentHP, Affiliation faction)
        {

            Faction = faction; 
            ArmyID = armyID;

            ArmyExperience = armyExperience;
            ArmyLevel = armyLevel;

            PlayerMaxHP = playerMaxHP;
            PlayerCurrentHP = playerCurrentHP;

            MentalMaxFortitude = mentalMaxHP;
            MentalCurrentFortitude = mentalCurrentHP;

            
            ArmyMobility = 0;//armyMobility;
            ArmyResilience = 0;//armyResilience;
            ArmyLeadership = 0;//armyLeadership;
            ArmyDiscipline = 0;//armyDiscipline;

            
            IP = 0;
            ExtraIP = 0;

            // Unit List/Dictionary:
            UnitList = new List<Unit>();
            UnitDict = new Dictionary<int, Unit>();

            TurnTokens = 0;

            // Statuses:
            isRouting = false;
            isAttacker = false;
            isAmbusher = false;
            isOutOfAction = false;

            // Default values for other states:
            Items = null;
            ArmyTalents = null;
            Status = null;

            ////
            //// Applying functionals:
            ////            
            ArmyName = Faction.ToString() + " Army #"+armyID.ToString();

            // The army's GameObject
            ThisGameObject = new GameObject(ArmyName);

            armyList.Add(this);

            // Create (child) pieces gameobject, (grandchildren) on field and off field
            GameObject armyPiecesObj = new GameObject("Pieces");
            GameObject armyOnFieldObj = new GameObject("On_Field");
            GameObject armyOffFieldObj = new GameObject("Off_Field");
            // Put the correct dependencies (player->army->pieces->on/off_field)
            if (Faction == Affiliation.Player)
            {
                factionObj = GameObject.Find("Player").gameObject;
            } else if (Faction == Affiliation.Ally)
            {
                factionObj = GameObject.Find("Ally").gameObject;
            } else if (Faction == Affiliation.Enemy)
            {
                factionObj = GameObject.Find("Enemy").gameObject;
            }
            ThisGameObject.transform.parent = factionObj.transform;
            armyPiecesObj.transform.parent = ThisGameObject.transform;
            armyOnFieldObj.transform.parent = armyPiecesObj.transform;
            armyOffFieldObj.transform.parent = armyPiecesObj.transform;


            // Calculating Army Mobility (average of units in action)
            //CODE HERE

            // Calculating Army Resilience
            //CODE HERE


            // Calculating Army Leadership
            //CODE HERE

            // Calculating Army Discipline
            //CODE HERE

            armyID++;
            return;
        }
    }

    
    public abstract class Player {
        //public event Action<Move> onMoveChosen;

        public abstract void Update ();

        protected virtual void ChoseMove (Move move)
        {
			//onMoveChosen?.Invoke (move);
        }
            
    }


    // Start is called before the first frame update
    void Start() // THIS MAY BE CHANGED AS IT IS NOT SUPPOSED TO BE A ONE-OFF USE, WE WANT TO HAVE A PREEXISTING 'ARMY' FOR NOW TO CALL IN BATTLE
    {            // CAN USE THIS FOR PLAYER AND ENEMY (ENEMY ARMY IS CLEARED AFTER BATTLE (OR STORED UNTIL RUN END))


        CreatePiece CP = GetComponent<CreatePiece>();
    

        // I MUST CLEAR OUT THE LIST AT EVERY START
        //AllyUnitList.Clear();
        //AllyUnitDict.Clear();
        //EnemyUnitList.Clear();
        //EnemyUnitDict.Clear();

        // Create Player and Enemy Armies
        playerArmy = new Army(0f, 0, 100f, 100f, 100, 100, Affiliation.Player);
        enemyArmy = new Army(0f, 0, 100f, 100f, 100, 100, Affiliation.Enemy);

        CP.NewPawn(playerArmy); // WANNA MAKE 3 PAWNS AND 1 KNIGHT FOR NOW
        CP.NewPawn(playerArmy);
        CP.NewPawn(playerArmy);
        CP.NewKnight(playerArmy);
        
        CP.NewPawn(enemyArmy); // WANNA MAKE 3 PAWNS AND 1 KNIGHT FOR NOW
        CP.NewPawn(enemyArmy);
        CP.NewPawn(enemyArmy);
        CP.NewKnight(enemyArmy);

        isInitialized = true;
    }

    // MAKE CODE TO TRANSFER ALLEGIANCE OF UNITS ETC.

    // Add a method to check if initialization is complete
    public bool IsInitialized()
    {
        return isInitialized;
    }
}
